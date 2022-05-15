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

    public class ParsedScheduleNoticeOfLeaseBuilder
    {
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
            if (row.Length != 73)
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
            var padding = new string(' ', _shift);
            return padding + row;
        }
    }

    //void SetItem(string token, Action<string> modify, TokenType type)
    //{
    //    var containsOpeningBracket = token.Contains('(');
    //    var containsClosingBracket = token.Contains(')');

    //    if (!(containsOpeningBracket ^ containsClosingBracket))
    //    {
    //        modify(token);
    //        return;
    //    }

    //    if (containsOpeningBracket)
    //}

    //void ParseTokenInRowWithSingleEntry(string token)
    //{
    //    if (token.StartsWith("NOTE"))
    //    {
    //        _instance.Notes.Add(token);
    //        return;
    //    }
    //}

    //void ParseTokenInRowWithTwoEntries(string token, int rowNumber, int columnNumber, int rowLength)
    //{
    //    var containsOpeningBracket = token.Contains('(');
    //    var containsClosingBracket = token.Contains(')');

    //    if (!containsOpeningBracket && containsClosingBracket)
    //    {
    //        var type = _tokensWithUnfulfilledBrackets ?? 
    //            throw new InvalidOperationException("Closing bracket detected without opening bracket");

    //        _tokensWithUnfulfilledBrackets = null;
    //        return type;
    //    }

    //    if (containsOpeningBracket && !containsClosingBracket)
    //    {
    //        if (_tokensWithUnfulfilledBrackets != null)
    //            throw new InvalidOperationException("Unable to determine token type");
    //        _tokensWithUnfulfilledBrackets = type;
    //    }
    //}

    //void AddToken(TokenType type, string value)
    //{
    //    var containsOpeningBracket = value.Contains('(');
    //    var containsClosingBracket = value.Contains(')');

    //    if (containsOpeningBracket && !containsClosingBracket)
    //    {
    //        if (_tokensWithUnfulfilledBrackets != null)
    //            throw new InvalidOperationException("Unable to determine token type");
    //        _tokensWithUnfulfilledBrackets = type;
    //    }

    //    if (!containsOpeningBracket && containsClosingBracket)
    //    {
    //        _tokensWithUnfulfilledBrackets = null;
    //    }

    //    if (type )
    //}
}

public enum TokenType
{
    Unknown,
    LeeseeTitle,
    RegistrationDateAndPlanRef,
    PropertyDescription,
    DateOfLeaseAndTerm,
    Note,
}
