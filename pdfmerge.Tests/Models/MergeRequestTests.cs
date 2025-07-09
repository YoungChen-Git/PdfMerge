using FluentAssertions;
using PdfMergeApi.Models;

namespace PdfMergeApi.Tests.Models
{
    public class MergeRequestTests
    {
        [Fact]
        public void MergeRequest_CanBeInstantiated()
        {
            // Act
            var request = new MergeRequest();

            // Assert
            request.Should().NotBeNull();
            request.DirectoryPath.Should().BeNull();
        }

        [Fact]
        public void MergeRequest_CanSetDirectoryPath()
        {
            // Arrange
            var expectedPath = "/test/path";

            // Act
            var request = new MergeRequest { DirectoryPath = expectedPath };

            // Assert
            request.DirectoryPath.Should().Be(expectedPath);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("/valid/path")]
        [InlineData("C:\\Windows\\Path")]
        public void MergeRequest_AcceptsVariousDirectoryPathValues(string path)
        {
            // Act
            var request = new MergeRequest { DirectoryPath = path };

            // Assert
            request.DirectoryPath.Should().Be(path);
        }

        [Fact]
        public void MergeRequest_AcceptsNullDirectoryPath()
        {
            // Act
            var request = new MergeRequest { DirectoryPath = null };

            // Assert
            request.DirectoryPath.Should().BeNull();
        }
    }
}
