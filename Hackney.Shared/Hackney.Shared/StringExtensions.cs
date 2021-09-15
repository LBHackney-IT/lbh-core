using System;

namespace Hackney.Shared
{
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
            if (str.Length == 1) return str.ToLowerInvariant();

            // else if (str.Length > 1)
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}
