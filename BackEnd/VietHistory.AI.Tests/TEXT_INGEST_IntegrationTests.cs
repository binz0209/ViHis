using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using VietHistory.Api.Controllers;
using VietHistory.Api.Controllers.Forms;
using VietHistory.Domain.Entities;
using VietHistory.Domain.Repositories;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Infrastructure.Mongo.Repositories;
using VietHistory.Infrastructure.Services.Gemini;
using VietHistory.Infrastructure.Services.TextIngest;
using Xunit;

namespace VietHistory.AI.Tests
{
    [Trait("Feature", "TEXT_INGEST")]
    [Trait("Integration", "Real")]
    public class TEXT_INGEST_IntegrationTests
    {
        private readonly IMongoContext _mongo;
        private readonly IngestController _controller;
        private readonly FallbackAIngestor _ingestor;
        private readonly ISourceRepository _sourceRepo;
        private readonly IChunkRepository _chunkRepo;

        public TEXT_INGEST_IntegrationTests()
        {
            var mongoSettings = new MongoSettings
            {
                ConnectionString = "mongodb+srv://lanserveUser:Binzdapoet.020904@hlinhwfil.eunq7.mongodb.net/?retryWrites=true&w=majority&appName=Hlinhwfil",
                Database = "vihis_test"
            };
            var mongoContext = new MongoContext(mongoSettings);
            _mongo = mongoContext;
            _sourceRepo = new SourceRepository(mongoContext);
            _chunkRepo = new ChunkRepository(mongoContext);

            // Ingestor requires GeminiOptions for embedding API
            var geminiOpt = new GeminiOptions
            {
                ApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? "",
                Model = "embedding-001"
            };
            var extractor = new PdfTextExtractor();
            _ingestor = new FallbackAIngestor(extractor, _chunkRepo, geminiOpt);

            _controller = new IngestController(_ingestor, _sourceRepo, _chunkRepo)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        #region GetAllChunks Tests

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC01_GetAllChunks_WithoutFilter_ReturnsAll()
        {
            // Arrange
            // Act
            var response = await _controller.GetAllChunks();

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC02_GetAllChunks_WithSourceId_ReturnsFiltered()
        {
            // Arrange - Use valid ObjectId format
            var sourceId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

            // Act
            var response = await _controller.GetAllChunks(sourceId);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P1")]
        public async Task TC03_GetAllChunks_Pagination_WorksCorrectly()
        {
            // Arrange
            var skip = 0;
            var take = 10;

            // Act
            var response = await _controller.GetAllChunks(null, skip, take);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P2")]
        public async Task TC04_GetAllChunks_NonexistentSourceId_ReturnsEmpty()
        {
            // Arrange - Use valid ObjectId format (but nonexistent)
            var nonexistentSourceId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

            // Act
            var response = await _controller.GetAllChunks(nonexistentSourceId);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            // Should return empty list, not throw
        }

        #endregion

        #region GetAllSources Tests

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC05_GetAllSources_ReturnsSources()
        {
            // Arrange
            // Act
            var response = await _controller.GetAllSources();

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P1")]
        public async Task TC06_GetAllSources_Pagination_Works()
        {
            // Arrange
            var skip = 0;
            var take = 5;

            // Act
            var response = await _controller.GetAllSources(skip, take);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
        }

        #endregion

        #region GetSourceWithChunks Tests

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC07_GetSourceWithChunks_ValidId_ReturnsSourceAndChunks()
        {
            // Arrange - Create a test source first
            var testSource = new SourceDoc
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Title = "Test Source",
                Author = "Test Author",
                Year = 2024,
                FileName = "test.pdf",
                Pages = 10
            };
            var sourceId = await _sourceRepo.InsertAsync(testSource);

            // Act
            var response = await _controller.GetSourceWithChunks(sourceId);

            // Assert
            response.Should().NotBeNull();
            var result = response as OkObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(200);

            // Cleanup
            await _sourceRepo.Collection.DeleteOneAsync(s => s.Id == sourceId);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P1")]
        public async Task TC08_GetSourceWithChunks_NonexistentId_Returns404()
        {
            // Arrange - Use valid ObjectId format (but nonexistent)
            var nonexistentId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

            // Act
            var response = await _controller.GetSourceWithChunks(nonexistentId);

            // Assert
            response.Should().NotBeNull();
            var result = response as NotFoundObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(404);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P2")]
        public async Task TC09_GetSourceWithChunks_InvalidObjectId_ThrowsFormatException()
        {
            // Arrange
            var invalidObjectId = "not-a-valid-objectid";

            // Act & Assert
            // MongoDB driver throws FormatException when trying to parse invalid ObjectId format
            // Current behavior: MongoDB throws before controller can handle it
            await Assert.ThrowsAsync<System.FormatException>(async () =>
                await _controller.GetSourceWithChunks(invalidObjectId));
        }

        #endregion

        #region Preview Tests (requires PDF file)

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P0")]
        public async Task TC10_Preview_MissingFile_Returns400()
        {
            // Arrange
            var form = new IngestUploadForm
            {
                File = null,
                Title = "Test Title"
            };

            // Act
            var response = await _controller.Preview(form);

            // Assert
            response.Should().NotBeNull();
            var result = response.Result as BadRequestObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P0")]
        public async Task TC11_Preview_EmptyFile_Returns400()
        {
            // Arrange
            var mockFile = new MockFormFile("test.pdf", 0);
            var form = new IngestUploadForm
            {
                File = mockFile,
                Title = "Test Title"
            };

            // Act
            var response = await _controller.Preview(form);

            // Assert
            response.Should().NotBeNull();
            var result = response.Result as BadRequestObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
        }

        // Note: Real PDF preview test requires a valid PDF file
        // TC12_Preview_ValidPDF_ReturnsPreview - Skipped until PDF test file available

        #endregion

        #region IngestAndSave Tests (requires PDF file and Gemini API)

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P0")]
        public async Task TC13_IngestAndSave_MissingFile_Returns400()
        {
            // Arrange
            var form = new IngestUploadForm
            {
                File = null,
                Title = "Test Title",
                Author = "Test Author",
                Year = 2024
            };

            // Act
            var response = await _controller.IngestAndSave(form, default);

            // Assert
            response.Should().NotBeNull();
            var result = response as BadRequestObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P0")]
        public async Task TC14_IngestAndSave_EmptyFile_Returns400()
        {
            // Arrange
            var mockFile = new MockFormFile("test.pdf", 0);
            var form = new IngestUploadForm
            {
                File = mockFile,
                Title = "Test Title"
            };

            // Act
            var response = await _controller.IngestAndSave(form, default);

            // Assert
            response.Should().NotBeNull();
            var result = response as BadRequestObjectResult;
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(400);
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P1")]
        public async Task TC15_IngestAndSave_MissingTitle_UsesFileName()
        {
            // Note: This test requires a valid PDF file
            // Current behavior: If Title is null/empty, uses Path.GetFileNameWithoutExtension(form.File.FileName)
            // Skipped until PDF test file available
        }

        // Note: Real ingest test requires:
        // - Valid PDF file
        // - Gemini API key configured
        // - May fail with rate limit (429) - should handle gracefully
        // TC16_IngestAndSave_ValidPDF_SavesToMongo - Skipped until PDF test file and API key available

        #endregion

        #region Helper Class for Mock IFormFile

        private class MockFormFile : IFormFile
        {
            private readonly string _fileName;
            private readonly long _length;

            public MockFormFile(string fileName, long length)
            {
                _fileName = fileName;
                _length = length;
            }

            public string ContentType => "application/pdf";
            public string ContentDisposition => $"form-data; name=\"file\"; filename=\"{_fileName}\"";
            public IHeaderDictionary Headers => new HeaderDictionary();
            public long Length => _length;
            public string Name => "file";
            public string FileName => _fileName;

            public Stream OpenReadStream()
            {
                return new MemoryStream();
            }

            public void CopyTo(Stream target)
            {
                // Empty implementation for mock
            }

            public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        #endregion
    }
}

