using EyeExamApi.DTOs;
using EyeExamApi.Interfaces;
using MediatR;
using System.Text.RegularExpressions;

namespace EyeExamApi.Domain;

public record GetSchedulesRequest() : IRequest<IEnumerable<ParsedScheduleNoticeOfLease>>;

public class GetSchedulesRequestHandler : IRequestHandler<GetSchedulesRequest, IEnumerable<ParsedScheduleNoticeOfLease>>
{
    readonly IRawScheduleDataService _service;

    public GetSchedulesRequestHandler(IRawScheduleDataService service)
    {
        _service = service;
    }

    public Task<IEnumerable<ParsedScheduleNoticeOfLease>> Handle(GetSchedulesRequest request, CancellationToken cancellationToken)
    {
        var rawSchedules = _service.GetRawScheduleNoticeOfLeases();

        return Task.FromResult(GetSchedules(rawSchedules));
    }

    IEnumerable<ParsedScheduleNoticeOfLease> GetSchedules(IEnumerable<RawScheduleNoticeOfLease> rawSchedules)
    {
        foreach (var rawSchedule in rawSchedules)
        {
            yield return new ParsedScheduleNoticeOfLeaseBuilder()
                .Build(rawSchedule);
        }
    }

    static class ColumnCalculator
    {
        public const int MaxRowLength = 73;

        const int SecondColumnIndex = 16;
        const int ThirdColumnIndex = 46;
        const int FourthColumnIndex = 62;

        const int FirstColumnLength = 16;
        const int SecondColumnLength = ThirdColumnIndex - SecondColumnIndex;
        const int ThirdColumnLength = FourthColumnIndex - ThirdColumnIndex;

        public static Row CalculateCells(string row, int columnOffset)
        {
            if (row.StartsWith("Note"))
                return Row.NewNoteRow(row);

            var cells = GetCells(row, columnOffset);
            return Row.New(cells);
        }

        static string?[] GetCells(string row, int columnOffset)
        {
            var cells = new string?[5];

            for (var column = 0; column < 5; column++)
            {
                var (index, length) = GetCellLocation(column, columnOffset);

                if (row.Length < index)
                    continue;

                if (row.Length < index + length)
                    length = row.Length - index;

                var value = row.Substring(index, length).Trim();

                cells[column] = value;
            }

            return cells;
        }

        static (int index, int length) GetCellLocation(int column, int columnOffset)
        {
            return (column + columnOffset) switch
            {
                0 => (0, FirstColumnLength),
                1 => (SecondColumnIndex, SecondColumnLength),
                2 => (ThirdColumnIndex, ThirdColumnLength),
                3 => (FourthColumnIndex, MaxRowLength),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    readonly struct Row
    {
        Row(string?[] cells, bool isNoteRow)
        {
            Cells = cells;
            IsNoteRow = isNoteRow;
        }

        public string?[] Cells { get; }
        public bool IsNoteRow { get; }

        public static Row New(string?[] cells) => new Row(cells, true);

        public static Row NewNoteRow(string value)
        {
            return new Row(new[] { value }, true);
        }
    }

    public class ParsedScheduleNoticeOfLeaseBuilder
    {
        public const int MaxRowLength = 73;

        const int SecondColumnIndex = 16;
        const int ThirdColumnIndex = 46;
        const int FourthColumnIndex = 62;

        const int FirstColumnLength = 16;
        const int SecondColumnLength = ThirdColumnIndex - SecondColumnIndex;
        const int ThirdColumnLength = FourthColumnIndex - ThirdColumnIndex;

        readonly ParsedScheduleNoticeOfLease _lease = new();
        readonly ShiftCalculator _shiftCalculator = new();

        public ParsedScheduleNoticeOfLeaseBuilder()
        {
        }

        public ParsedScheduleNoticeOfLease Build(RawScheduleNoticeOfLease rawSchedule)
        {
            _lease.EntryNumber = int.Parse(rawSchedule.EntryNumber);

            ParseFirstRow(rawSchedule.EntryText[0]);

            for (int i = 1; i < rawSchedule.EntryText.Count; i++)
            {
                var row = _shiftCalculator.AddShift(rawSchedule.EntryText[i]);
                ParseRow(row);
            }

            return _lease;
        }

        void ParseFirstRow(string row)
        {
            if (row.Length != MaxRowLength)
                throw new ArgumentException($"Invalid first row with length {row.Length} and value {row}", nameof(row));

            _lease.RegistrationDateAndPlanRef += GetValue(row, 0, FirstColumnLength, false);
            _lease.PropertyDescription += GetValue(row, SecondColumnIndex, SecondColumnLength, false);
            _lease.DateOfLeaseAndTerm += GetValue(row, ThirdColumnIndex, ThirdColumnLength, false);
            _lease.LesseesTitle += GetValue(row, FourthColumnIndex, row.Length, false);
        }

        void ParseRow(string row)
        {
            if (row.StartsWith("NOTE"))
            {
                _lease.Notes.Add(row);
                return;
            }

            row = row.TrimEnd();

            _lease.RegistrationDateAndPlanRef += GetValue(row, 0, FirstColumnLength);

            if (row.Length > SecondColumnIndex)
                _lease.PropertyDescription += GetValue(row, SecondColumnIndex, SecondColumnLength);
            if (row.Length > ThirdColumnIndex)
                _lease.DateOfLeaseAndTerm += GetValue(row, ThirdColumnIndex, ThirdColumnLength);
        }

        string? GetValue(string row, int columnStart, int columnLength, bool addLeadingSpace = true)
        {
            if (row.Length < columnStart + columnLength)
                columnLength = row.Length - columnStart;

            var value = row.Substring(columnStart, columnLength).Trim();

            if (string.IsNullOrWhiteSpace(value))
                return null;

            _shiftCalculator.CalculateShift(value, columnStart, columnStart + columnLength);

            if (addLeadingSpace)
                value = ' ' + value;

            return value;
        }
    }

    class ShiftCalculator
    {
        readonly ISet<string> _conjunctions = new HashSet<string>
        {
            "and"
        };

        int _shift = 0;

        public void CalculateShift(string entry, int entryStart, int entryEnd)
        {
            if (entryStart == 0)
                return;

            if (entry.Contains('(') && !entry.Contains(')'))
            {
                _shift = entryStart;
                return;
            }

            if (entry.EndsWith(')'))
            {
                _shift = entryEnd;
                return;
            }

            var words = entry.Split();

            if (_conjunctions.Contains(words.Last()))
                _shift = entryStart;
        }

        public string AddShift(string row)
        {
            if (row.Length == ParsedScheduleNoticeOfLeaseBuilder.MaxRowLength || row.StartsWith("NOTE"))
                return row;

            var padding = new string(' ', _shift);
            return padding + row;
        }
    }
}

enum TokenType
{
    Unknown,
    LeeseeTitle,
    RegistrationDateAndPlanRef,
    PropertyDescription,
    DateOfLeaseAndTerm,
    Note,
}
