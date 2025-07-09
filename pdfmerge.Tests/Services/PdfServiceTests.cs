using FluentAssertions;
using PdfMergeApi.Services;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfMergeApi.Tests.Services
{
    public class PdfServiceTests : IDisposable
    {
        private readonly string _testDirectory;
        private readonly PdfService _pdfService;

        public PdfServiceTests()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            _pdfService = new PdfService();
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public void MergePdfsInDirectory_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "nonexistent");

            // Act & Assert
            var exception = Assert.Throws<DirectoryNotFoundException>(() =>
                _pdfService.MergePdfsInDirectory(nonExistentPath));

            exception.Message.Should().Contain("指定的目錄不存在");
        }

        [Fact]
        public void MergePdfsInDirectory_WithEmptyDirectory_ThrowsFileNotFoundException()
        {
            // Arrange
            var emptyDirectory = Path.Combine(_testDirectory, "empty");
            Directory.CreateDirectory(emptyDirectory);

            // Act & Assert
            var exception = Assert.Throws<FileNotFoundException>(() =>
                _pdfService.MergePdfsInDirectory(emptyDirectory));

            exception.Message.Should().Contain("找不到任何 PDF 檔案");
        }

        [Fact]
        public void MergePdfsInDirectory_WithSinglePdf_ReturnsValidPdfBytes()
        {
            // Arrange
            var testPdfPath = CreateTestPdf("test1.pdf");

            // Act
            var result = _pdfService.MergePdfsInDirectory(_testDirectory);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
            
            // 驗證結果是有效的 PDF
            using var stream = new MemoryStream(result);
            var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            document.PageCount.Should().Be(1);
        }

        [Fact]
        public void MergePdfsInDirectory_WithMultiplePdfs_MergesAllPages()
        {
            // Arrange
            CreateTestPdf("test1.pdf");
            CreateTestPdf("test2.pdf");
            CreateTestPdf("test3.pdf");

            // Act
            var result = _pdfService.MergePdfsInDirectory(_testDirectory);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);

            // 驗證合併後的 PDF 包含所有頁面
            using var stream = new MemoryStream(result);
            var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            document.PageCount.Should().Be(3); // 3 個 PDF，每個 1 頁
        }

        [Fact]
        public void MergePdfsInDirectory_WithSubdirectories_FindsPdfsRecursively()
        {
            // Arrange
            var subDirectory = Path.Combine(_testDirectory, "subdir");
            Directory.CreateDirectory(subDirectory);

            CreateTestPdf("root.pdf");
            CreateTestPdf(Path.Combine("subdir", "sub.pdf"));

            // Act
            var result = _pdfService.MergePdfsInDirectory(_testDirectory);

            // Assert
            result.Should().NotBeNull();

            using var stream = new MemoryStream(result);
            var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            document.PageCount.Should().Be(2); // 2 個 PDF，每個 1 頁
        }

        [Fact]
        public void MergePdfsInDirectory_WithMixedFiles_OnlyProcessesPdfFiles()
        {
            // Arrange
            CreateTestPdf("valid.pdf");
            
            // 創建非 PDF 檔案
            var txtFile = Path.Combine(_testDirectory, "notpdf.txt");
            File.WriteAllText(txtFile, "This is not a PDF");

            var jpgFile = Path.Combine(_testDirectory, "image.jpg");
            File.WriteAllBytes(jpgFile, new byte[] { 0xFF, 0xD8, 0xFF }); // JPEG header

            // Act
            var result = _pdfService.MergePdfsInDirectory(_testDirectory);

            // Assert
            result.Should().NotBeNull();

            using var stream = new MemoryStream(result);
            var document = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
            document.PageCount.Should().Be(1); // 只有 1 個有效的 PDF
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\n")]
        public void MergePdfsInDirectory_WithWhitespaceOrEmptyPath_ThrowsDirectoryNotFoundException(string path)
        {
            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() =>
                _pdfService.MergePdfsInDirectory(path));
        }

        private string CreateTestPdf(string fileName)
        {
            var fullPath = Path.Combine(_testDirectory, fileName);
            var directory = Path.GetDirectoryName(fullPath);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            // 創建一個簡單的 PDF 文件
            using var document = new PdfDocument();
            var page = document.AddPage();
            
            using var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page);
            
            // 使用簡單的線條而不是文字，避免字型問題
            var width = page.Width.Point;
            var height = page.Height.Point;
            gfx.DrawLine(PdfSharp.Drawing.XPens.Black, 0, 0, width, height);
            gfx.DrawLine(PdfSharp.Drawing.XPens.Black, width, 0, 0, height);
            
            // 畫一個圓圈
            var r = width / 10;
            gfx.DrawEllipse(new PdfSharp.Drawing.XPen(PdfSharp.Drawing.XColors.Blue, 2), 
                new PdfSharp.Drawing.XRect(width / 2 - r, height / 2 - r, 2 * r, 2 * r));

            document.Save(fullPath);
            return fullPath;
        }
    }
}
