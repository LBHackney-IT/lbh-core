namespace Hackney.Core.DynamoDb.EntityUpdater;

public static class StringExtensions
{
    /// <summary>
    /// Converts the string to camel case (i.e. the first character is lowercase)
    /// </summary>
    /// <param name="str">The string to change</param>
    /// <returns>A copied of the string with the first chacater in lowercase. A null or empty string returns what was supplied.
    /// </returns>
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
