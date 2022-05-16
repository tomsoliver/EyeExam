namespace EyeExamApi.Core.Parsing;

class RowParser
{
    const int SecondColumnIndex = 16;
    const int ThirdColumnIndex = 46;
    const int FourthColumnIndex = 62;
    const int MaxRowLength = 73;

    int _previousColumnOffset;

    public IEnumerable<Row> Parse(IReadOnlyList<string> entryRows)
    {
        for (int i = 0; i < entryRows.Count - 1; i++)
            yield return Parse(entryRows[i]);

        yield return Parse(entryRows[^1], isLastRow: true);
    }

    Row Parse(string entry, bool isLastRow = false)
    {
        if (entry.StartsWith("Note", StringComparison.OrdinalIgnoreCase))
            return Row.NewNoteRow(entry);

        var cells = GetCells(entry, isLastRow);
        return Row.New(cells);
    }

    string?[] GetCells(string row, bool isLastRow)
    {
        var cells = new string?[4];

        var columnOffset = GetColumnOffset(row, isLastRow);

        for (var column = columnOffset; column < 4; column++)
        {
            var (index, length) = GetCellIndexAndLength(column - columnOffset);
            cells[column] = GetCellValue(row, index, length);
        }

        return cells;
    }

    int GetColumnOffset(string row, bool isLastRow)
    {
        if (isLastRow)
            return _previousColumnOffset;

        return _previousColumnOffset = row.Length switch
        {
            <= SecondColumnIndex => 3,
            <= ThirdColumnIndex => 2,
            <= FourthColumnIndex => 1,
            <= MaxRowLength => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(row),
                $"Row too long with length {row.Length} and value {row}")
        };
    }

    static (int index, int length) GetCellIndexAndLength(int column)
    {
        return column switch
        {
            0 => (0, SecondColumnIndex),
            1 => (SecondColumnIndex, ThirdColumnIndex - SecondColumnIndex),
            2 => (ThirdColumnIndex, FourthColumnIndex - ThirdColumnIndex),
            3 => (FourthColumnIndex, MaxRowLength - FourthColumnIndex),
            _ => throw new ArgumentOutOfRangeException(nameof(column), $"Unknown column {column}")
        };
    }

    static string? GetCellValue(string row, int index, int length)
    {
        if (row.Length < index)
            return null;

        if (row.Length < index + length)
            length = row.Length - index;

        var value = row.Substring(index, length).Trim();

        return value;
    }
}
