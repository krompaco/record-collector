using System;
using System.Globalization;
using System.Linq;

namespace Krompaco.RecordCollector.Web.Extensions
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return input switch
            {
#pragma warning disable SA1122 // Use string.Empty for empty strings
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
#pragma warning restore SA1122 // Use string.Empty for empty strings
                _ => input.First().ToString(CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture) + input.Substring(1)
            };
        }
    }
}
