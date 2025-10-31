namespace VietHistory.Application.DTOs.Ingest;

public sealed class Chunk
{
    public int ChunkIndex { get; init; }
    public string Content { get; init; } = string.Empty;
    public int PageFrom { get; init; }
    public int PageTo { get; init; }
    public int ApproxTokens { get; init; }
}

