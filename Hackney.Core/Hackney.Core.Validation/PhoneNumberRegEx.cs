using System;

namespace Hackney.Core.Validation
{
    public static class PhoneNumberRegEx
    {
        public readonly static string UkPhoneNumberRegEx
            = @"^(((\+44\s?\d{4}|\(?0\d{4}\)?)\s?\d{3}\s?\d{3})|((\+44\s?\d{3}|\(?0\d{3}\)?)\s?\d{3}\s?\d{4})|((\+44\s?\d{2}|\(?0\d{2}\)?)\s?\d{4}\s?\d{4}))(\s?\#(\d{4}|\d{3}))?$";

        public readonly static string IntPhoneNumberRegEx = @"^[+]?([0-9]*[\.\s\-\(\)]|[0-9]+){6,24}$";

        /// <summary>
        /// Returns the appropriate phone number reg ex for the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The correct reg ex</returns>
        /// <exception cref="System.ArgumentException">When the supplied value is unknown.</exception>
        public static string GetRegEx(PhoneNumberType type)
        {
            switch (type)
            {
                case PhoneNumberType.UK: return UkPhoneNumberRegEx;
                case PhoneNumberType.International: return IntPhoneNumberRegEx;
                default:
                    throw new ArgumentException($"Unknown phone number type value: {type}", nameof(type));
            }
        }

    }
}
