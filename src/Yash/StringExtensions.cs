namespace Yash;

public static class StringExtensions
{
    public static string CapitalizeFirstLetter(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        char[] charArray = input.ToCharArray();
        if (char.IsLower(charArray[0]))
        {
            charArray[0] = char.ToUpper(charArray[0]);
        }

        return new string(charArray);
    }
}