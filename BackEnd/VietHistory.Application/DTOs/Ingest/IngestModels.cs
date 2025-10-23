using System;

namespace VietHistory.Application.DTOs.Ingest
{
    public sealed class ParserProfile
    {
        public int MinChunkTokens { get; init; } = 900;   // ~800–1000
        public int OverlapTokens { get; init; } = 120;    // ~100–150
        public double HeaderFooterFreqThreshold { get; init; } = 0.7; // 70%
        public string[] Abbreviations { get; init; } = new[]
        {
            "Tp.", "ThS.", "TS.", "PGS.", "GS.", "Mr.", "Ms.", "Dr.", "tr."
        };
    }

    public sealed class PageText
    {
        public int PageNumber { get; init; }
        public string Raw { get; init; } = string.Empty;
    }

    public sealed class Chunk
    {
        public int ChunkIndex { get; init; }
        public string Content { get; init; } = string.Empty;
        public int PageFrom { get; init; }
        public int PageTo { get; init; }
        public int ApproxTokens { get; init; }
    }

    public sealed class IngestPreviewResult
    {
        public string FileName { get; init; } = string.Empty;
        public int TotalPages { get; init; }
        public int TotalChunks { get; init; }
        public IReadOnlyList<Chunk> Chunks { get; init; } = Array.Empty<Chunk>();
    }
}
