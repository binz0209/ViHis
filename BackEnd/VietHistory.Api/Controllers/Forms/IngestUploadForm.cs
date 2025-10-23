using Microsoft.AspNetCore.Http;

namespace VietHistory.Api.Controllers.Forms;

public sealed class IngestUploadForm
{
    public IFormFile? File { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public int? Year { get; set; }
}
