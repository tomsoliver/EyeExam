using EyeExamApi.Core.DTOs;

namespace EyeExamApi.Core.Parsing;

public class RawScheduleParser
{
    readonly ParsedScheduleNoticeOfLease _lease = new();
    readonly RowParser _rowParser = new();
    readonly RawScheduleNoticeOfLease _rawSchedule;

    public RawScheduleParser(RawScheduleNoticeOfLease rawSchedule)
    {
        _rawSchedule = rawSchedule;
    }

    public ParsedScheduleNoticeOfLease Parse()
    {
        _lease.EntryNumber = int.Parse(_rawSchedule.EntryNumber);

        ParseEntryText();

        return _lease;
    }

    void ParseEntryText()
    {
        var rows = _rowParser.Parse(_rawSchedule.EntryText)
            .ToArray();

        AddFirstRow(rows[0]);

        for (int i = 1; i < _rawSchedule.EntryText.Count; i++)
            AddRow(rows[i]);
    }

    void AddFirstRow(Row row)
    {
        if (row.IsNoteRow)
            throw new ArgumentException("first row cannot be a note row");

        _lease.RegistrationDateAndPlanRef += row.Cells[0];
        _lease.PropertyDescription += row.Cells[1];
        _lease.DateOfLeaseAndTerm += row.Cells[2];
        _lease.LesseesTitle += row.Cells[3];
    }

    void AddRow(Row row)
    {
        if (row.IsNoteRow)
        {
            _lease.Notes.Add(row.Note);
            return;
        }

        _lease.RegistrationDateAndPlanRef += AppendIfNotNull(row.Cells[0]);
        _lease.PropertyDescription += AppendIfNotNull(row.Cells[1]);
        _lease.DateOfLeaseAndTerm += AppendIfNotNull(row.Cells[2]);
    }

    static string? AppendIfNotNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return ' ' + value;
    }
}
