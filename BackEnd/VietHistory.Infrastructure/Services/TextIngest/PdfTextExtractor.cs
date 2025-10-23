using System;
using System.Collections.Generic;
using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using VietHistory.Application.DTOs.Ingest;
using PdfDocument = UglyToad.PdfPig.PdfDocument;

namespace VietHistory.Infrastructure.Services.TextIngest
{
    public interface IPdfTextExtractor
    {
        IReadOnlyList<PageText> ExtractPages(Stream pdfStream);
    }

    /// <summary>
    /// PdfPig extractor: đọc ổn định PDF lớn (hàng nghìn trang) có text-layer.
    /// </summary>
    public sealed class PdfTextExtractor : IPdfTextExtractor
    {
        public IReadOnlyList<PageText> ExtractPages(Stream pdfStream)
        {
            // PdfPig yêu cầu stream "seekable". Copy sang MemoryStream cho chắc chắn.
            var mem = new MemoryStream();
            pdfStream.CopyTo(mem);
            mem.Position = 0;

            var pages = new List<PageText>();

            // Bạn có thể truyền ParsingOptions nếu muốn, nhưng mặc định là đủ.
            using (var pdf = PdfDocument.Open(mem))
            {
                foreach (var page in pdf.GetPages())
                {
                    // page.Text: đã theo thứ tự đọc tương đối tốt
                    var text = page.Text ?? string.Empty;

                    // Chuẩn hoá CRLF -> LF để các bước sau nhất quán
                    text = text.Replace("\r\n", "\n");

                    pages.Add(new PageText
                    {
                        PageNumber = page.Number,
                        Raw = text
                    });
                }
            }

            return pages;
        }
    }
}
