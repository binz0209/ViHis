using System.Text.RegularExpressions;
using VietHistory.Application.DTOs.Ingest;

namespace VietHistory.Infrastructure.Services.TextIngest
{
    public interface IFallbackAIngestor
    {
        (IReadOnlyList<Chunk> Chunks, int TotalPages) Run(Stream pdfStream, ParserProfile? profile = null);
    }

    public sealed class FallbackAIngestor : IFallbackAIngestor
    {
        private readonly IPdfTextExtractor _extractor;

        public FallbackAIngestor(IPdfTextExtractor extractor)
        {
            _extractor = extractor;
        }

        public (IReadOnlyList<Chunk> Chunks, int TotalPages) Run(Stream pdfStream, ParserProfile? profile = null)
        {
            var pf = profile ?? new ParserProfile();

            // 1) Extract
            var pages = _extractor.ExtractPages(pdfStream)
                .Select(p => new PageText { PageNumber = p.PageNumber, Raw = TextNormalizer.CleanRaw(p.Raw) })
                .OrderBy(p => p.PageNumber)
                .ToList();

            // 2) Header/Footer
            // 1️⃣ Gộp các trang cực ngắn (ví dụ chỉ có 1–2 dòng)
            for (int i = 0; i < pages.Count - 1;)
            {
                if (pages[i].Raw.Length < 400)
                {
                    pages[i + 1] = new PageText
                    {
                        PageNumber = pages[i].PageNumber,
                        Raw = pages[i].Raw + "\n" + pages[i + 1].Raw
                    };
                    pages.RemoveAt(i);
                }
                else i++;
            }

            // 2️⃣ Header/footer detect chặt hơn (80%)
            var (headers, footers) = HeaderFooterDetector.Detect(pages, 2, 2, 0.8);

            // 3️⃣ Loại bỏ header/footer & làm sạch [Trang X] dư
            var cleaned = HeaderFooterDetector.RemoveHeadersFooters(pages, headers, footers)
                .Select(p => new PageText
                {
                    PageNumber = p.PageNumber,
                    Raw = Regex.Replace(p.Raw, @"(\[Trang\s+\d+\]\s*){2,}", "[Trang $1]", RegexOptions.IgnoreCase)
                })
                .ToList();

            // 3) Sentences + giữ [Trang X]
            var sentencesWithPage = new List<(string sentence, int page)>();
            foreach (var p in cleaned)
            {
                var lines = p.Raw.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                var tag = lines.Count > 0 && lines[0].StartsWith("[Trang ") ? lines[0] : $"[Trang {p.PageNumber}]";
                var body = string.Join("\n", lines.Where((_, i) => i > 0));

                var sents = SentenceTokenizer.SplitSentences(body, pf.Abbreviations);
                sentencesWithPage.Add((tag, p.PageNumber));
                foreach (var s in sents) sentencesWithPage.Add((s, p.PageNumber));
            }

            // 4) Pack theo token + overlap
            var chunks = new List<Chunk>();
            var buffer = new List<(string sentence, int page)>();
            int bufTok = 0;
            int idx = 0;

            foreach (var item in sentencesWithPage)
            {
                var t = ChunkPack.ApproxTokens(item.sentence);
                if (bufTok + t > pf.MinChunkTokens && buffer.Count > 0)
                {
                    chunks.Add(BuildChunk(idx++, buffer));
                    var back = new List<(string sentence, int page)>();
                    int backTok = 0;
                    for (int i = buffer.Count - 1; i >= 0 && backTok < pf.OverlapTokens; i--)
                    {
                        back.Insert(0, buffer[i]);
                        backTok += ChunkPack.ApproxTokens(buffer[i].sentence);
                    }
                    buffer = back;
                    bufTok = backTok;
                }
                buffer.Add(item);
                bufTok += t;
            }
            if (buffer.Count > 0) chunks.Add(BuildChunk(idx++, buffer));

            return (chunks, pages.Count);
        }

        private static Chunk BuildChunk(int idx, List<(string sentence, int page)> buffer)
        {
            var pages = buffer.Select(x => x.page).ToList();
            var pFrom = pages.Min();
            var pTo = pages.Max();
            var text = string.Join(" ", buffer.Select(x => x.sentence));

            if (!text.StartsWith("[Trang "))
                text = $"[Trang {pFrom}] " + text;

            return new Chunk
            {
                ChunkIndex = idx,
                Content = text.Trim(),
                PageFrom = pFrom,
                PageTo = pTo,
                ApproxTokens = ChunkPack.ApproxTokens(text)
            };
        }
    }
}
