using Microsoft.AspNetCore.Http;

namespace VietHistory.Api.Controllers.Forms
{
    public sealed class IngestPreviewForm
    {
        // Tên property "file" => trùng key form-data trên Swagger
        public IFormFile? File { get; set; }
    }
}
