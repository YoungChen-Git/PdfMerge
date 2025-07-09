using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PdfMergeApi.Models;
using PdfMergeApi.Services;
using PdfSharp.Pdf;
using System.Net;
using System.Text;
using System.Text.Json;

namespace PdfMergeApi.Tests.Integration
{
    public class PdfControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _testDirectory;

        public PdfControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
        }

        public void Dispose()
        {
            _client.Dispose();
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public async Task MergePdfs_WithValidDirectory_ReturnsValidPdf()
        {
            // Arrange
            CreateTestPdf("test1.pdf");
            CreateTestPdf("test2.pdf");

            var request = new MergeRequest { DirectoryPath = _testDirectory };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/pdf/merge", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
            
            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            pdfBytes.Length.Should().BeGreaterThan(0);

            // 驗證返回的是有效的 PDF
            using var stream = new MemoryStream(pdfBytes);
            var document = PdfSharp.Pdf.IO.PdfReader.Open(stream, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
            document.PageCount.Should().Be(2);
        }

        [Fact]
        public async Task MergePdfs_WithEmptyDirectoryPath_ReturnsBadRequest()
        {
            // Arrange
            var request = new MergeRequest { DirectoryPath = "" };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/pdf/merge", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("DirectoryPath 欄位不可為空");
        }

        [Fact]
        public async Task MergePdfs_WithNonExistentDirectory_ReturnsNotFound()
        {
            // Arrange
            var request = new MergeRequest { DirectoryPath = "/nonexistent/directory" };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/pdf/merge", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task HealthCheck_ReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("healthy");
        }

        private void CreateTestPdf(string fileName)
        {
            var fullPath = Path.Combine(_testDirectory, fileName);

            using var document = new PdfDocument();
            var page = document.AddPage();
            
            using var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
            
            // 使用簡單的圖形而不是文字，避免字型問題
            var width = page.Width.Point;
            var height = page.Height.Point;
            gfx.DrawLine(PdfSharp.Drawing.XPens.Red, 0, 0, width, height);
            gfx.DrawLine(PdfSharp.Drawing.XPens.Red, width, 0, 0, height);

            // 畫一個圓圈
            var r = width / 5;
            gfx.DrawEllipse(new PdfSharp.Drawing.XPen(PdfSharp.Drawing.XColors.Red, 1.5), 
                PdfSharp.Drawing.XBrushes.White, 
                new PdfSharp.Drawing.XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            document.Save(fullPath);
        }
    }
}
