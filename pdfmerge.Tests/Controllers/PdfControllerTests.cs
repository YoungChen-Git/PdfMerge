using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PdfMergeApi.Controllers;
using PdfMergeApi.Models;
using PdfMergeApi.Services;

namespace PdfMergeApi.Tests.Controllers
{
    public class PdfControllerTests
    {
        private readonly Mock<IPdfService> _mockPdfService;
        private readonly PdfController _controller;

        public PdfControllerTests()
        {
            _mockPdfService = new Mock<IPdfService>();
            _controller = new PdfController(_mockPdfService.Object);
        }

        [Fact]
        public void MergePdfs_WithValidRequest_ReturnsFileResult()
        {
            // Arrange
            var request = new MergeRequest { DirectoryPath = "/valid/path" };
            var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };
            
            _mockPdfService.Setup(s => s.MergePdfsInDirectory(request.DirectoryPath))
                          .Returns(expectedBytes);

            // Act
            var result = _controller.MergePdfs(request);

            // Assert
            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;
            fileResult!.FileContents.Should().BeEquivalentTo(expectedBytes);
            fileResult.ContentType.Should().Be("application/pdf");
            fileResult.FileDownloadName.Should().StartWith("merged_");
            fileResult.FileDownloadName.Should().EndWith(".pdf");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void MergePdfs_WithInvalidDirectoryPath_ReturnsBadRequest(string invalidPath)
        {
            // Arrange
            var request = new MergeRequest { DirectoryPath = invalidPath };

            // Act
            var result = _controller.MergePdfs(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("DirectoryPath 欄位不可為空。");
        }

        [Fact]
        public void MergePdfs_WithNullDirectoryPath_ReturnsBadRequest()
        {
            // Arrange
            var request = new MergeRequest { DirectoryPath = null };

            // Act
            var result = _controller.MergePdfs(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult!.Value.Should().Be("DirectoryPath 欄位不可為空。");
        }

        [Fact]
        public void MergePdfs_WithDirectoryNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new MergeRequest { DirectoryPath = "/nonexistent/path" };
            var exception = new DirectoryNotFoundException("目錄不存在");
            
            _mockPdfService.Setup(s => s.MergePdfsInDirectory(request.DirectoryPath))
                          .Throws(exception);

            // Act
            var result = _controller.MergePdfs(request);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult!.Value.Should().Be(exception.Message);
        }

        [Fact]
        public void MergePdfs_WithGeneralException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new MergeRequest { DirectoryPath = "/some/path" };
            var exception = new InvalidOperationException("Something went wrong");
            
            _mockPdfService.Setup(s => s.MergePdfsInDirectory(request.DirectoryPath))
                          .Throws(exception);

            // Act
            var result = _controller.MergePdfs(request);

            // Assert
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(500);
            objectResult.Value.Should().Be($"處理過程中發生內部錯誤: {exception.Message}");
        }

        [Fact]
        public void MergePdfs_GeneratesUniqueFileNames()
        {
            // Arrange
            var request = new MergeRequest { DirectoryPath = "/valid/path" };
            var expectedBytes = new byte[] { 1, 2, 3 };
            
            _mockPdfService.Setup(s => s.MergePdfsInDirectory(request.DirectoryPath))
                          .Returns(expectedBytes);

            // Act
            var result1 = _controller.MergePdfs(request) as FileContentResult;
            Thread.Sleep(1000); // 確保時間戳不同
            var result2 = _controller.MergePdfs(request) as FileContentResult;

            // Assert
            result1!.FileDownloadName.Should().NotBe(result2!.FileDownloadName);
            result1.FileDownloadName.Should().MatchRegex(@"merged_\d{14}\.pdf");
            result2.FileDownloadName.Should().MatchRegex(@"merged_\d{14}\.pdf");
        }

        [Fact]
        public void MergePdfs_CallsPdfServiceWithCorrectParameter()
        {
            // Arrange
            var request = new MergeRequest { DirectoryPath = "/test/path" };
            var expectedBytes = new byte[] { 1, 2, 3 };
            
            _mockPdfService.Setup(s => s.MergePdfsInDirectory(It.IsAny<string>()))
                          .Returns(expectedBytes);

            // Act
            _controller.MergePdfs(request);

            // Assert
            _mockPdfService.Verify(s => s.MergePdfsInDirectory("/test/path"), Times.Once);
        }
    }
}
