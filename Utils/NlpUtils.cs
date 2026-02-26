using System.Text.RegularExpressions;

namespace IndoorWayfinder.Api.Utils;

public static class NlpUtils
{
    private static readonly Regex[] Patterns =
    {
        new(@"(?:tu|từ)\s+(?<a>.+?)\s+(?:den|đến|toi|tới)\s+(?<b>.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"^(?<a>.+?)\s*->\s*(?<b>.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"(?:den|đến|toi|tới)\s+(?<b>.+)\s+(?:tu|từ)\s+(?<a>.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    };

    private static readonly Regex OnlyBPattern =
        new(@"(?:den|đến|toi|tới)\s+(?<b>.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static (string? A, string? B) ExtractAAndB(string q)
    {
        var s = q.Trim();
        foreach (var pat in Patterns)
        {
            var m = pat.Match(s);
            if (m.Success)
            {
                var a = m.Groups["a"].Success ? m.Groups["a"].Value : null;
                var b = m.Groups["b"].Success ? m.Groups["b"].Value : null;
                return (a, b);
            }
        }

        var m2 = OnlyBPattern.Match(s);
        if (m2.Success)
        {
            return (null, m2.Groups["b"].Value);
        }

        return (null, null);
    }
}

