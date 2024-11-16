//CellCode.cs (QUESTION (i mean I don't need AAAA21)
namespace LabCalculator;
public static class CellCode
{
    public static string ToCode(int index)
    {
        index++;
        var result = "";
        if (index == 0) return ((char)64).ToString();
        while (index > 0)
        {
            var remainder = (index - 1) % 26;
            result = (char)(65 + remainder) + result;
            index = (index - remainder) / 26;
        }
        return result;
    }

    public static Tuple<int, int> FromCode(string code)
    {
        var column = 0;
        var row = 0;
        foreach (var chr in code)
        {
            if (char.IsLetter(chr))
            {
                column = column * 26 + (chr - 'A' + 1);
            }
            else if (char.IsDigit(chr))
            {
                row = row * 10 + (chr - '0');
            }
        }
        column--;
        return new Tuple<int, int>(row, column);
    }
}
