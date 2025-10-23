using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Application.DTOs.Ingest;

namespace VietHistory.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly MongoContext _ctx;
    public AdminController(MongoContext ctx) { _ctx = ctx; }

    [HttpGet("mongo/ping")]
    public async Task<IActionResult> Ping()
    {
        try
        {
            var doc = await _ctx.Db.RunCommandAsync<MongoDB.Bson.BsonDocument>(
                new MongoDB.Bson.BsonDocument("ping", 1));

            double okRaw = 0;
            if (doc.TryGetValue("ok", out var okVal))
            {
                // "ok" có thể là Int32 hoặc Double tuỳ server
                okRaw = okVal.IsInt32 ? okVal.AsInt32 : okVal.ToDouble();
            }

            var names = await _ctx.Db.ListCollectionNames().ToListAsync();

            // ✅ chỉ trả POCO/scalar an toàn
            return Ok(new
            {
                ok = okRaw == 1.0,
                okRaw,
                database = _ctx.Db.DatabaseNamespace.DatabaseName,
                collections = names
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { ok = false, error = ex.Message });
        }
    }


    [HttpGet("mongo/info")]
    public async Task<IActionResult> Info()
    {
        var srcCol = _ctx.Db.GetCollection<SourceDoc>("sources");
        var chkCol = _ctx.Db.GetCollection<ChunkDoc>("chunks");
        var sc = await srcCol.CountDocumentsAsync(FilterDefinition<SourceDoc>.Empty);
        var cc = await chkCol.CountDocumentsAsync(FilterDefinition<ChunkDoc>.Empty);
        return Ok(new { database = _ctx.Db.DatabaseNamespace.DatabaseName, sources = sc, chunks = cc });
    }
}
