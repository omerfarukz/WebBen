using System.Text;

namespace WebBen.Core.Extensions;

/// <summary>
///     How To: Best way to draw table in console app (C#)
///     https://stackoverflow.com/questions/856845/how-to-best-way-to-draw-table-in-console-app-c
/// </summary>
internal static class TableParser
{
    public static string ToStringTable<T>(
        this IEnumerable<T> values,
        string?[] columnHeaders,
        params Func<T, object>[] valueSelectors)
    {
        return ToStringTable(values.ToArray(), columnHeaders, valueSelectors);
    }

    private static string ToStringTable<T>(
        this T[] values,
        string?[] columnHeaders,
        params Func<T, object>[] valueSelectors)
    {
        if (columnHeaders.Length != valueSelectors.Length)
            throw new ArgumentException();

        var arrValues = new string?[values.Length + 1, valueSelectors.Length];

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
        sb.Append('╭');
        for (var colIndex = 0; colIndex < maxColumnsWidth.Length; colIndex++)
        {
            sb.Append(new string('─', maxColumnsWidth[colIndex]));
            sb.Append(maxColumnsWidth.Length == colIndex + 1 ? '╮' : '┬');
        }
        sb.AppendLine();

        // Headers
        for (var rowIndex = 0; rowIndex < 1; rowIndex++)
        {
            sb.Append('│');
            for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                var cell = arrValues[rowIndex, colIndex]?.PadRight(maxColumnsWidth[colIndex]);
                sb.Append(cell);
                sb.Append('│');
            }
        }
        sb.AppendLine();
    
        // Header bottom line
        sb.Append('├');
        for (var colIndex = 0; colIndex < maxColumnsWidth.Length; colIndex++)
        {
            sb.Append(new string('─', maxColumnsWidth[colIndex]));
            sb.Append(maxColumnsWidth.Length == colIndex + 1 ? '│' : '┼');
        }
        sb.AppendLine();
    
        // Rows
        for (var rowIndex = 1; rowIndex < arrValues.GetLength(0); rowIndex++)
        {
            sb.Append('│');
            for (var colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                var cell = arrValues[rowIndex, colIndex]?.PadRight(maxColumnsWidth[colIndex]);
                sb.Append(cell);
                sb.Append('│');
            }
            sb.AppendLine();
        }

        // Bottom line
        sb.Append('╰');
        for (var colIndex = 0; colIndex < maxColumnsWidth.Length; colIndex++)
        {
            sb.Append(new string('─', maxColumnsWidth[colIndex]));
            sb.Append(maxColumnsWidth.Length == colIndex + 1 ? '╯' : '┴');
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