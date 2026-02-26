using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace IndoorWayfinder.Api.Utils;

public static class NormUtils
{
    private static readonly Regex NonAllowedChars =
        new(@"[^a-z0-9\s\-\_/]", RegexOptions.Compiled);

    private static readonly Regex MultiSpace =
        new(@"\s+", RegexOptions.Compiled);

    public static string NormalizeName(string s)
    {
        s = s.Trim().ToLowerInvariant();
        s = RemoveDiacritics(s);
        s = NonAllowedChars.Replace(s, " ");
        s = MultiSpace.Replace(s, " ");

        s = s.Replace("toa ", "toa ")
             .Replace("toà ", "toa ")
             .Replace("nha ", "toa ")
             .Replace("khoi ", "khu ");

        return s.Trim();
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}

