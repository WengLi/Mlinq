using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mlinq.Common
{
    internal static class StringExtensions
    {
        private const string StartCharacterExp = @"[\p{L}\p{Nl}_]";
        private const string OtherCharacterExp = @"[\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Pc}\p{Cf}]";

        private const string NameExp = StartCharacterExp + OtherCharacterExp + "{0,}";

        private static readonly Regex _undottedNameValidator = new Regex(@"^" + NameExp + @"$", RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex _migrationIdPattern = new Regex(@"\d{15}_.+");
        private static readonly string[] _lineEndings = new[] { "\r\n", "\n" };

        public static bool EqualsIgnoreCase(this string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool EqualsOrdinal(this string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.Ordinal);
        }

        public static string RestrictTo(this string s, int size)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= size)
            {
                return s;
            }

            return s.Substring(0, size);
        }

        public static void EachLine(this string s, Action<string> action)
        {
            s.Split(_lineEndings, StringSplitOptions.None).Each(action);
        }

        public static bool IsValidUndottedName(this string name)
        {
            return !string.IsNullOrEmpty(name) && _undottedNameValidator.IsMatch(name);
        }
    }
}
