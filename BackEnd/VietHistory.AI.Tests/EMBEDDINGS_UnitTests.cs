using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using System.Text.Json;
using VietHistory.Infrastructure.Services;
using Xunit;

namespace VietHistory.AI.Tests
{
    [Trait("Feature", "EMBEDDINGS")]
    public class EMBEDDINGS_UnitTests
    {
        #region GenerateEmbeddingAsync Tests

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC15_GenerateEmbeddingAsync_ValidText_ReturnsVector()
        {
            // Arrange - Setup mock HTTP response
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""embedding"": {
                        ""values"": [0.1, 0.2, 0.3, 0.4, 0.5]
                    }
                }", Encoding.UTF8, "application/json")
            };

            var mockHttpHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHttpHandler) { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            var result = await embeddingService.GenerateEmbeddingAsync("Test text");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(0.1f);
            result[1].Should().Be(0.2f);
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P0")]
        public async Task TC16_GenerateEmbeddingAsync_EmptyText_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            Func<Task> act = async () => await embeddingService.GenerateEmbeddingAsync("");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P0")]
        public async Task TC17_GenerateEmbeddingAsync_NullText_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            Func<Task> act = async () => await embeddingService.GenerateEmbeddingAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P1")]
        public async Task TC18_GenerateEmbeddingAsync_WhitespaceText_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            Func<Task> act = async () => await embeddingService.GenerateEmbeddingAsync("   ");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P1")]
        public async Task TC19_GenerateEmbeddingAsync_InvalidResponseFormat_ThrowsInvalidOperationException()
        {
            // Arrange - Setup mock HTTP response with invalid format
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""invalid"": ""response""
                }", Encoding.UTF8, "application/json")
            };

            var mockHttpHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHttpHandler) { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            Func<Task> act = async () => await embeddingService.GenerateEmbeddingAsync("Test text");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Failed to parse embedding*");
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P1")]
        public async Task TC20_GenerateEmbeddingAsync_HttpError_ThrowsInvalidOperationException()
        {
            // Arrange - Setup mock HTTP response with error
            var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(@"{
                    ""error"": ""Invalid API key""
                }", Encoding.UTF8, "application/json")
            };

            var mockHttpHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHttpHandler) { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            Func<Task> act = async () => await embeddingService.GenerateEmbeddingAsync("Test text");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Failed to generate embedding*");
        }

        #endregion

        #region GenerateQueryEmbeddingAsync Tests

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P1")]
        public async Task TC21_GenerateQueryEmbeddingAsync_ValidText_ReturnsVector()
        {
            // Arrange - Setup mock HTTP response
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""embedding"": {
                        ""values"": [0.1, 0.2, 0.3, 0.4, 0.5]
                    }
                }", Encoding.UTF8, "application/json")
            };

            var mockHttpHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHttpHandler) { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            var result = await embeddingService.GenerateQueryEmbeddingAsync("Test query");

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(0.1f);
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P0")]
        public async Task TC22_GenerateQueryEmbeddingAsync_EmptyText_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            Func<Task> act = async () => await embeddingService.GenerateQueryEmbeddingAsync("");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P0")]
        public async Task TC23_GenerateQueryEmbeddingAsync_NullText_ThrowsArgumentException()
        {
            // Arrange
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            Func<Task> act = async () => await embeddingService.GenerateQueryEmbeddingAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*cannot be null or empty*");
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P1")]
        public async Task TC24_GenerateQueryEmbeddingAsync_InvalidResponseFormat_ThrowsInvalidOperationException()
        {
            // Arrange - Setup mock HTTP response with invalid format
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{
                    ""invalid"": ""response""
                }", Encoding.UTF8, "application/json")
            };

            var mockHttpHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHttpHandler) { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            Func<Task> act = async () => await embeddingService.GenerateQueryEmbeddingAsync("Test query");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Failed to parse embedding*");
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P1")]
        public async Task TC25_GenerateQueryEmbeddingAsync_HttpError_ThrowsInvalidOperationException()
        {
            // Arrange - Setup mock HTTP response with error
            var mockResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(@"{
                    ""error"": ""Invalid API key""
                }", Encoding.UTF8, "application/json")
            };

            var mockHttpHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHttpHandler) { Timeout = TimeSpan.FromSeconds(60) };
            var embeddingService = new EmbeddingService(httpClient, "test-api-key", "text-embedding-004");

            // Act
            Func<Task> act = async () => await embeddingService.GenerateQueryEmbeddingAsync("Test query");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Failed to generate query embedding*");
        }

        #endregion
    }

    // Helper class for mocking HttpClient
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}

