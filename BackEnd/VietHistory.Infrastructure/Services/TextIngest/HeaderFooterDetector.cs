using System.Text.RegularExpressions;
using VietHistory.Application.DTOs.Ingest;
using VietHistory.Domain.Entities;

namespace VietHistory.Infrastructure.Services.TextIngest
{
    public static class HeaderFooterDetector
    {
        public static (HashSet<string> headers, HashSet<string> footers) Detect(
            IReadOnlyList<PageText> pages, int headLines = 2, int footLines = 2, double freqThreshold = 0.7)
        {
            var total = pages.Count;
            var headerCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var footerCounts = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var p in pages)
            {
                var lines = SplitLines(p.Raw);
                foreach (var h in lines.Take(Math.Min(headLines, lines.Count)))
                {
                    var k = NormalizeLine(h);
                    if (k.Length > 0) headerCounts[k] = headerCounts.GetValueOrDefault(k) + 1;
                }
                foreach (var f in lines.Skip(Math.Max(0, lines.Count - footLines)))
                {
                    var k = NormalizeLine(f);
                    if (k.Length > 0) footerCounts[k] = footerCounts.GetValueOrDefault(k) + 1;
                }
            }

            var headerSet = headerCounts
                .Where(kv => kv.Value >= Math.Ceiling(freqThreshold * total))
                .Select(kv => kv.Key).ToHashSet(StringComparer.Ordinal);

            var footerSet = footerCounts
                .Where(kv => kv.Value >= Math.Ceiling(freqThreshold * total))
                .Select(kv => kv.Key).ToHashSet(StringComparer.Ordinal);

            return (headerSet, footerSet);
        }

        public static IReadOnlyList<PageText> RemoveHeadersFooters(
            IReadOnlyList<PageText> pages, HashSet<string> headers, HashSet<string> footers)
        {
            var list = new List<PageText>(pages.Count);
            foreach (var p in pages)
            {
                var lines = SplitLines(p.Raw).ToList();

                while (lines.Count > 0 && headers.Contains(NormalizeLine(lines[0])))
                    lines.RemoveAt(0);

                while (lines.Count > 0 && footers.Contains(NormalizeLine(lines[^1])))
                    lines.RemoveAt(lines.Count - 1);

                var text = string.Join("\n", lines);
                // Gắn tag [Trang X] để trích dẫn sau này
                text = $"[Trang {p.PageNumber}]\n{text}";
                list.Add(new PageText { PageNumber = p.PageNumber, Raw = text });
            }
            return list;
        }

        private static IReadOnlyList<string> SplitLines(string t)
            => t.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        private static string NormalizeLine(string s)
        {
            var t = s.Trim();
            if (int.TryParse(t, out _)) return "<PAGENUM>";
            t = Regex.Replace(t, @"\s+", " ").ToLowerInvariant();
            return t;
        }
    }
}
