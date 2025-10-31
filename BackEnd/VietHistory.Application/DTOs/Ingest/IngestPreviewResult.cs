using System;

namespace VietHistory.Application.DTOs.Ingest;

public sealed class IngestPreviewResult
{
    public string FileName { get; init; } = string.Empty;
    public int TotalPages { get; init; }
    public int TotalChunks { get; init; }
    public IReadOnlyList<Chunk> Chunks { get; init; } = Array.Empty<Chunk>();
}

