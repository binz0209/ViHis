using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using VietHistory.Domain.Entities;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Infrastructure.Services;

namespace VietHistory.Api.Controllers;

/// <summary>
/// Controller để generate embeddings cho existing chunks
/// Dùng để migrate data sang vector search
/// </summary>
[ApiController]
[Route("api/v1/admin/embeddings")]
public class EmbeddingsController : ControllerBase
{
    private readonly IMongoContext _ctx;
    private readonly EmbeddingService? _embeddingService;
    private readonly ILogger<EmbeddingsController> _logger;

    public EmbeddingsController(
        IMongoContext ctx, 
        EmbeddingService? embeddingService,
        ILogger<EmbeddingsController> logger)
    {
        _ctx = ctx;
        _embeddingService = embeddingService;
        _logger = logger;
    }

    /// <summary>
    /// Generate embeddings cho tất cả chunks chưa có embedding
    /// </summary>
    [HttpPost("generate-all")]
    public async Task<ActionResult> GenerateAllEmbeddings([FromQuery] int? limit = null, CancellationToken ct = default)
    {
        if (_embeddingService == null)
        {
            return BadRequest(new { error = "EmbeddingService is not configured. Please set Gemini:ApiKey in appsettings.json" });
        }

        try
        {
            // Lấy tất cả chunks chưa có embedding
            var filter = Builders<ChunkDoc>.Filter.Eq(chunk => chunk.Embedding, null);
            var totalCount = await _ctx.Chunks.CountDocumentsAsync(filter, cancellationToken: ct);
            
            var query = _ctx.Chunks.Find(filter);
            if (limit.HasValue)
            {
                query = query.Limit(limit.Value);
            }
            
            var chunks = await query.ToListAsync(ct);
            
            _logger.LogInformation($"Found {chunks.Count} chunks without embeddings (total: {totalCount})");

            if (chunks.Count == 0)
            {
                return Ok(new { message = "All chunks already have embeddings", processed = 0, total = totalCount });
            }

            int processed = 0;
            int errors = 0;
            var startTime = DateTime.UtcNow;

            foreach (var chunk in chunks)
            {
                try
                {
                    // Generate embedding
                    _logger.LogDebug($"Generating embedding for chunk {chunk.Id}...");
                    var embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content, ct);

                    // Update chunk
                    var update = Builders<ChunkDoc>.Update.Set(c => c.Embedding, embedding);
                    await _ctx.Chunks.UpdateOneAsync(
                        Builders<ChunkDoc>.Filter.Eq(c => c.Id, chunk.Id),
                        update,
                        cancellationToken: ct
                    );

                    processed++;

                    if (processed % 10 == 0)
                    {
                        _logger.LogInformation($"Processed {processed}/{chunks.Count} chunks...");
                    }

                    // Rate limiting: delay 100ms between requests
                    await Task.Delay(100, ct);
                }
                catch (Exception ex)
                {
                    errors++;
                    _logger.LogError(ex, $"Error processing chunk {chunk.Id}: {ex.Message}");
                }
            }

            var duration = DateTime.UtcNow - startTime;

            return Ok(new
            {
                message = "Embeddings generated successfully",
                processed,
                errors,
                total = totalCount,
                duration = $"{duration.TotalSeconds:F2}s",
                averageTime = $"{duration.TotalSeconds / processed:F2}s per chunk"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Kiểm tra trạng thái embeddings
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult> GetEmbeddingStatus(CancellationToken ct = default)
    {
        try
        {
            var total = await _ctx.Chunks.CountDocumentsAsync(_ => true, cancellationToken: ct);
            var withEmbedding = await _ctx.Chunks.CountDocumentsAsync(c => c.Embedding != null, cancellationToken: ct);
            var withoutEmbedding = total - withEmbedding;

            return Ok(new
            {
                total,
                withEmbedding,
                withoutEmbedding,
                percentage = total > 0 ? (withEmbedding * 100.0 / total).ToString("F2") : "0.00"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting embedding status");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Xóa tất cả embeddings (dùng khi cần regenerate)
    /// </summary>
    [HttpDelete("all")]
    public async Task<ActionResult> DeleteAllEmbeddings(CancellationToken ct = default)
    {
        try
        {
            var update = Builders<ChunkDoc>.Update.Unset(c => c.Embedding);
            var result = await _ctx.Chunks.UpdateManyAsync(_ => true, update, cancellationToken: ct);

            _logger.LogInformation($"Deleted {result.ModifiedCount} embeddings");

            return Ok(new
            {
                message = "Embeddings deleted successfully",
                deleted = result.ModifiedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting embeddings");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}




