using System.Globalization;

public static class Helper
{
    private static NumberFormatInfo numberFormat = new CultureInfo("en-US", false).NumberFormat;

    //extension method
    public static string NiceFloat(this float f, int numOfDecimal = 2)
    {
        numberFormat.NumberDecimalDigits = numOfDecimal;
        return f.ToString("N", numberFormat);
    }
}
