using System.Diagnostics.CodeAnalysis;

namespace EyeExamApi.Core.Parsing;

readonly struct Row
{
    Row(string?[]? cells, string? note)
    {
        if (cells == null && note == null)
            throw new ArgumentException("Must be either a note and normal row");

        if (cells != null && note != null)
            throw new ArgumentException("Cannot be both a note and normal row");

        Cells = cells ?? new string[0];
        Note = note;
        IsNoteRow = note is not null;
    }

    [MemberNotNullWhen(true, nameof(IsNoteRow))]
    public string? Note { get; }
    public string?[] Cells { get; }
    public bool IsNoteRow { get; }

    public static Row New(string?[] cells) => new Row(cells, null);

    public static Row NewNoteRow(string value) => new Row(null, value);
}
