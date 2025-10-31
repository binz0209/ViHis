namespace VietHistory.Application.DTOs.Ingest;

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

