using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.RegularExpressions;

namespace Conveyancing_Extractor.Services
{
    /// <summary>
    /// Minimal regex-based HTML utility. No third-party libraries.
    /// Methods are targeted at known, stable patterns in machine-generated HTML
    /// rather than attempting to parse arbitrary markup.
    /// </summary>
    public static partial class HtmlParser
    {
        [GeneratedRegex(@"<[^>]+>")]
        private static partial Regex StripTagsRegex();

        [GeneratedRegex(@"\s+")]
        private static partial Regex CollapseWhitespaceRegex();

        /// <summary>
        /// Returns decoded plain text from an HTML fragment by stripping all tags.
        /// </summary>
        public static string InnerText(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            var stripped = StripTagsRegex().Replace(html, " ");
            return WebUtility.HtmlDecode(CollapseWhitespaceRegex().Replace(stripped, " ").Trim());
        }

        

        /// <summary>
        /// Returns the first regex capture (group 1, falling back to group 2 for alternation
        /// patterns), or empty string when there is no match.
        /// </summary>
        public static string Match(string html,
            [StringSyntax(StringSyntaxAttribute.Regex)] string pattern,
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline)
        {
            var m = Regex.Match(html, pattern, options);
            if (!m.Success) return string.Empty;
            var g1 = m.Groups[1].Value.Trim();
            return g1.Length > 0 ? g1 : m.Groups[2].Value.Trim();
        }

        
        /// <summary>
        /// Counts non-overlapping occurrences of a literal string (case-sensitive).
        /// </summary>
        public static int Count(string html, string literal)
        {
            if (string.IsNullOrEmpty(html)) return 0;
            int count = 0, pos = 0;
            while ((pos = html.IndexOf(literal, pos, StringComparison.Ordinal)) != -1)
            {
                count++;
                pos += literal.Length;
            }
            return count;
        }
    }
}
