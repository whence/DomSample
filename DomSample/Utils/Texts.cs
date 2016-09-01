using System;
using System.Collections.Generic;

namespace DomSample.Utils
{
    public static class Texts
    {
        public static string CapitaliseFirstChar(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return string.Empty + char.ToUpperInvariant(text[0]) + text.Substring(1, text.Length - 1);
        }

        public static string[] SplitCsv(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new string[0];

            var parts = text.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
            var list = new List<string>(parts.Length);
            foreach (var part in parts)
            {
                var item = part.Trim();
                if (!string.IsNullOrEmpty(item))
                {
                    list.Add(item);
                }
            }
            return list.ToArray();
        }

        public static bool? ParseBoolean(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            text = text.Trim();

            if ("yes".Equals(text, StringComparison.OrdinalIgnoreCase))
                return true;

            if ("no".Equals(text, StringComparison.OrdinalIgnoreCase))
                return false;

            bool value;
            if (bool.TryParse(text, out value))
                return value;

            return null;
        }

        public static string Trim(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Trim();
        }
    }
}
