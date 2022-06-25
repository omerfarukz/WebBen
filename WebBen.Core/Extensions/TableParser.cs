using System.Text;

namespace WebBen.Core.Extensions;

/// <summary>
///     Adapted from
///     How To: Best way to draw table in console app (C#)
///     https://stackoverflow.com/questions/856845/how-to-best-way-to-draw-table-in-console-app-c
/// </summary>
internal static class TableParser
{
    private const char Pipe = '│';
    private const char Line = '─';
    private const char TopLeftCorner = '╭';
    private const char TopCross = '┬';
    private const char TopRightCorner = '╮';
    private const char BottomLeftCorner = '╰';
    private const char BottomRightCorner = '╯';
    private const char BottomCross = '┴';
    private const char CellDivider = '┼';
    private const char CellBeginning = '├';
    private const char CellEnding = '┤';

    public static string ToStringTable<T>(
        this IEnumerable<T> values,
        string?[] columnHeaders,
        params Func<T, object>[] valueSelectors)
    {
        return ToStringTable(values.ToArray(), columnHeaders, valueSelectors);
    }

    private static string ToStringTable<T>(
        this IReadOnlyList<T> values,
        string?[] columnHeaders,
        params Func<T, object>[] valueSelectors)
    {
        if (columnHeaders.Length != valueSelectors.Length)
            throw new ArgumentException();

        var arrValues = new string?[values.Count + 1, valueSelectors.Length];

        // Fill headers
        for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            arrValues[0, colIndex] = columnHeaders[colIndex];

        // Fill table rows
        for (var rowIndex = 1; rowIndex < arrValues.GetLength(0); rowIndex++)
        for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            arrValues[rowIndex, colIndex] = valueSelectors[colIndex]
                .Invoke(values[rowIndex - 1]).ToString();

        return ToStringTable(arrValues);
    }

    private static string ToStringTable(string?[,] arrValues)
    {
        var maxColumnsWidth = GetMaxColumnsWidth(arrValues);
        var sb = new StringBuilder();
        // Top line

        sb.Append(TopLeftCorner);
        for (var colIndex = 0; colIndex < maxColumnsWidth.Length; colIndex++)
        {
            sb.Append(new string(Line, maxColumnsWidth[colIndex]));
            sb.Append(maxColumnsWidth.Length == colIndex + 1 ? TopRightCorner : TopCross);
        }

        sb.AppendLine();

        // Headers
        sb.Append(Pipe);
        for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
        {
            var cell = arrValues[0, colIndex]?.PadRight(maxColumnsWidth[colIndex]);
            sb.Append(cell);
            sb.Append(Pipe);
        }

        sb.AppendLine();

        // Header line
        sb.Append(CellBeginning);
        for (var colIndex = 0; colIndex < maxColumnsWidth.Length; colIndex++)
        {
            sb.Append(new string(Line, maxColumnsWidth[colIndex]));
            sb.Append(maxColumnsWidth.Length == colIndex + 1 ? CellEnding : CellDivider);
        }

        sb.AppendLine();

        // Rows
        for (var rowIndex = 1; rowIndex < arrValues.GetLength(0); rowIndex++)
        {
            sb.Append(Pipe);
            for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                var cell = arrValues[rowIndex, colIndex]?.PadRight(maxColumnsWidth[colIndex]);
                sb.Append(cell);
                sb.Append(Pipe);
            }

            sb.AppendLine();
        }

        // Bottom line
        sb.Append(BottomLeftCorner);
        for (var colIndex = 0; colIndex < maxColumnsWidth.Length; colIndex++)
        {
            sb.Append(new string('─', maxColumnsWidth[colIndex]));
            sb.Append(maxColumnsWidth.Length == colIndex + 1 ? BottomRightCorner : BottomCross);
        }

        sb.AppendLine();

        return sb.ToString();
    }

    private static int[] GetMaxColumnsWidth(string?[,] arrValues)
    {
        var maxColumnsWidth = new int[arrValues.GetLength(1)];
        for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
        for (var rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
        {
            var newLength = arrValues[rowIndex, colIndex]?.Length;
            var oldLength = maxColumnsWidth[colIndex];

            if (newLength > oldLength) maxColumnsWidth[colIndex] = newLength.Value;
        }

        return maxColumnsWidth;
    }
}