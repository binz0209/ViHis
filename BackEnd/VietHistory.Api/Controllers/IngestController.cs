using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using VietHistory.Api.Controllers.Forms;
using VietHistory.Application.DTOs.Ingest;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Infrastructure.Services.TextIngest;

namespace VietHistory.Api.Controllers;

[ApiController]
[Route("api/v1/ingest")]
public sealed class IngestController : ControllerBase
{
    private readonly IFallbackAIngestor _ingestor;
    private readonly ISourceRepository _sourceRepo;
    private readonly IChunkRepository _chunkRepo;

    public IngestController(
        IFallbackAIngestor ingestor,
        ISourceRepository sourceRepo,
        IChunkRepository chunkRepo)
    {
        _ingestor = ingestor;
        _sourceRepo = sourceRepo;
        _chunkRepo = chunkRepo;
    }

    /// <summary>📄 Upload PDF để preview 10 chunk đầu tiên (chưa lưu DB)</summary>
    [HttpPost("preview")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(IngestPreviewResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<IngestPreviewResult>> Preview([FromForm] IngestUploadForm form)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest("Missing PDF.");

        using var ms = new MemoryStream();
        await form.File.CopyToAsync(ms);
        ms.Position = 0;

        // ⚙️ Gọi RunAsync (chưa cần sourceId vì chỉ preview)
        var (chunks, totalPages) = await _ingestor.RunAsync(ms, "preview");

        return Ok(new IngestPreviewResult
        {
            FileName = form.File.FileName,
            TotalPages = totalPages,
            TotalChunks = chunks.Count,
            Chunks = chunks.Take(10).ToList()
        });
    }

    /// <summary>📘 Ingest thật & lưu Mongo (sources, chunks có embedding)</summary>
    [HttpPost("pdf")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> IngestAndSave([FromForm] IngestUploadForm form, CancellationToken ct)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest("Missing PDF.");

        using var ms = new MemoryStream();
        await form.File.CopyToAsync(ms);
        ms.Position = 0;

        // 1️⃣ Tạo Source
        var source = new SourceDoc
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Title = string.IsNullOrWhiteSpace(form.Title)
                ? Path.GetFileNameWithoutExtension(form.File.FileName)
                : form.Title!,
            Author = form.Author,
            Year = form.Year,
            FileName = form.File.FileName,
        };
        var sourceId = await _sourceRepo.InsertAsync(source, ct);

        // 2️⃣ Gọi Ingest (sẽ tự tạo chunk + embedding + lưu Mongo)
        var (chunks, totalPages) = await _ingestor.RunAsync(ms, sourceId);

        // 3️⃣ Update số trang vào Source (nếu muốn)
        var update = Builders<SourceDoc>.Update.Set(s => s.Pages, totalPages);
        await _sourceRepo.Collection.UpdateOneAsync(s => s.Id == sourceId, update, cancellationToken: ct);

        return Ok(new
        {
            message = "✅ Ingested & saved successfully",
            sourceId,
            title = source.Title,
            totalPages,
            totalChunks = chunks.Count
        });
    }

    /// <summary>📖 Lấy toàn bộ các chunk đã ingest (hoặc theo sourceId)</summary>
    [HttpGet("chunks")]
    [ProducesResponseType(typeof(List<ChunkDoc>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllChunks([FromQuery] string? sourceId = null, [FromQuery] int skip = 0, [FromQuery] int take = 100)
    {
        var filter = string.IsNullOrWhiteSpace(sourceId)
            ? Builders<ChunkDoc>.Filter.Empty
            : Builders<ChunkDoc>.Filter.Eq(c => c.SourceId, sourceId);

        var chunks = await _chunkRepo.Collection
            .Find(filter)
            .Skip(skip)
            .Limit(take)
            .SortBy(c => c.SourceId)
            .ThenBy(c => c.ChunkIndex)
            .ToListAsync();

        return Ok(new { count = chunks.Count, chunks });
    }

    /// <summary>📗 Lấy danh sách source đã ingest (để chọn lọc xem chunk)</summary>
    [HttpGet("sources")]
    [ProducesResponseType(typeof(List<SourceDoc>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSources([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        var sources = await _sourceRepo.Collection
            .Find(Builders<SourceDoc>.Filter.Empty)
            .Skip(skip)
            .Limit(take)
            .SortByDescending(s => s.Year)
            .ThenBy(s => s.Title)
            .ToListAsync();

        return Ok(new { count = sources.Count, sources });
    }

    /// <summary>🔍 Lấy chi tiết 1 source + các chunk của nó</summary>
    [HttpGet("source/{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSourceWithChunks(string id)
    {
        var src = await _sourceRepo.Collection.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (src == null)
            return NotFound($"Không tìm thấy source với id={id}");

        var chunks = await _chunkRepo.Collection
            .Find(c => c.SourceId == id)
            .SortBy(c => c.ChunkIndex)
            .ToListAsync();

        return Ok(new
        {
            source = src,
            chunkCount = chunks.Count,
            chunks
        });
    }
}
