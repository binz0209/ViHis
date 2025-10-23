using System.Text.RegularExpressions;

namespace VietHistory.Infrastructure.Services.TextIngest
{
    public static class SentenceTokenizer
    {
        public static List<string> SplitSentences(string text, string[] abbreviations)
        {
            var protects = new Dictionary<string, string>();
            string Protect(string s) { var k = $"__ABB_{protects.Count}__"; protects[k] = s; return k; }

            foreach (var abb in abbreviations.Distinct())
            {
                var esc = Regex.Escape(abb);
                text = Regex.Replace(text, esc, m => Protect(m.Value));
            }

            var parts = Regex.Split(text, @"(?<=[\.\?\!…])\s+|\n{2,}")
                             .Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

            for (int i = 0; i < parts.Count; i++)
                foreach (var kv in protects) parts[i] = parts[i].Replace(kv.Key, kv.Value);

            return parts;
        }
    }

    public static class ChunkPack
    {
        // Ước lượng: 1 token ≈ 4 ký tự (tiếng Việt có dấu)
        public static int ApproxTokens(string s) => Math.Max(1, s.Length / 4);

        public static IEnumerable<(string text, int approxTokens)> PackByTokens(
            IEnumerable<string> sentences, int targetTokens, int overlapTokens)
        {
            var cur = new List<string>();
            int curTok = 0;

            foreach (var s in sentences)
            {
                var t = ApproxTokens(s);
                if (curTok + t > targetTokens && cur.Count > 0)
                {
                    yield return (string.Join(" ", cur), curTok);

                    var back = new List<string>();
                    int backTok = 0;
                    for (int i = cur.Count - 1; i >= 0 && backTok < overlapTokens; i--)
                    {
                        back.Insert(0, cur[i]);
                        backTok += ApproxTokens(cur[i]);
                    }
                    cur = back;
                    curTok = backTok;
                }
                cur.Add(s);
                curTok += t;
            }
            if (cur.Count > 0) yield return (string.Join(" ", cur), curTok);
        }
    }
}
