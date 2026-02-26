namespace IndoorWayfinder.Api.Utils
{
    using System.Text;
    using System.Text.RegularExpressions;

    public static class TextUtils
    {
        public static string NormalizeName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string lower = input.ToLowerInvariant();

            // Remove Vietnamese diacritics
            var normalized = lower.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            var result = sb.ToString().Normalize(NormalizationForm.FormC);

            // Remove special chars
            result = Regex.Replace(result, @"[^a-z0-9\s]", "");
            result = Regex.Replace(result, @"\s+", " ").Trim();

            return result;
        }
    }
}
