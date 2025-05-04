namespace App1.Tests.Services
{
    using System;
    using System.IO;
    using Moq;
    using UnitTests.EmailJobs.AuxiliaryTestClasses;
    using Xunit;

    public class FileTemplateProviderTests : IDisposable
    {
        private readonly string tempDirectory;
        private readonly string templatesDirectory;
        private readonly string emailTemplatePath;
        private readonly string plainTextTemplatePath;
        private readonly string reviewTemplatePath;

        public FileTemplateProviderTests()
        {
            // Set up test directory structure
            this.tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            this.templatesDirectory = Path.Combine(this.tempDirectory, "Templates");
            Directory.CreateDirectory(this.templatesDirectory);

            // Create template files with test content
            this.emailTemplatePath = Path.Combine(this.templatesDirectory, "EmailContentTemplate.html");
            this.plainTextTemplatePath = Path.Combine(this.templatesDirectory, "PlainTextContentTemplate.txt");
            this.reviewTemplatePath = Path.Combine(this.templatesDirectory, "RecentReviewForReportTemplate.html");

            File.WriteAllText(this.emailTemplatePath, "<html><body>Email Template</body></html>");
            File.WriteAllText(this.plainTextTemplatePath, "Plain Text Template");
            File.WriteAllText(this.reviewTemplatePath, "<div>Review Template</div>");

            // Mock or override the base directory for tests
            Environment.SetEnvironmentVariable("BASEDIR", this.tempDirectory);
        }

        [Fact]
        public void GetEmailTemplate_WhenFileExists_ReturnsFileContent()
        {
            // Arrange
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory);

            // Act
            string result = provider.GetEmailTemplate();

            // Assert
            Assert.Equal("<html><body>Email Template</body></html>", result);
        }

        [Fact]
        public void GetPlainTextTemplate_WhenFileExists_ReturnsFileContent()
        {
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory);
            string result = provider.GetPlainTextTemplate();
            Assert.Equal("Plain Text Template", result);
        }

        [Fact]
        public void GetReviewRowTemplate_WhenFileExists_ReturnsFileContent()
        {
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory);
            string result = provider.GetReviewRowTemplate();
            Assert.Equal("<div>Review Template</div>", result);
        }

        [Fact]
        public void GetEmailTemplate_WhenFileDoesNotExist_ThrowsFileNotFoundException()
        {
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory);
            File.Delete(this.emailTemplatePath);
            Assert.Throws<FileNotFoundException>(() => provider.GetEmailTemplate());
        }

        [Fact]
        public void GetPlainTextTemplate_WhenFileDoesNotExist_ThrowsFileNotFoundException()
        {
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory);
            File.Delete(this.plainTextTemplatePath);

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => provider.GetPlainTextTemplate());
        }

        [Fact]
        public void GetReviewRowTemplate_WhenFileDoesNotExist_ThrowsFileNotFoundException()
        {
            // Arrange
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory);
            File.Delete(this.reviewTemplatePath);

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => provider.GetReviewRowTemplate());
        }

        [Fact]
        public void GetEmailTemplate_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            // Arrange
            Directory.Delete(this.templatesDirectory, true);
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory);

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => provider.GetEmailTemplate());
        }

        [Fact]
        public void GetPlainTextTemplate_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            Directory.Delete(this.templatesDirectory, true);
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory);
            Assert.Throws<DirectoryNotFoundException>(() => provider.GetPlainTextTemplate());
        }

        [Fact]
        public void GetReviewRowTemplate_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
        {
            Directory.Delete(this.templatesDirectory, true);
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory);
            Assert.Throws<DirectoryNotFoundException>(() => provider.GetReviewRowTemplate());
        }

        //[Fact]
        //public void GetEmailTemplate_WhenAccessDenied_ThrowsUnauthorizedAccessException()
        //{
        //    Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();
        //    mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Throws(new UnauthorizedAccessException("Access denied"));
        //    TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory, mockFileSystem.Object);
        //    Assert.Throws<UnauthorizedAccessException>(() => provider.GetEmailTemplate());
        //}

        [Fact]
        public void GetPlainTextTemplate_WhenAccessDenied_ThrowsUnauthorizedAccessException()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Throws(new UnauthorizedAccessException("Access denied"));
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory, mockFileSystem.Object);
            Assert.Throws<UnauthorizedAccessException>(() => provider.GetPlainTextTemplate());
        }

        [Fact]
        public void GetReviewRowTemplate_WhenAccessDenied_ThrowsUnauthorizedAccessException()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Throws(new UnauthorizedAccessException("Access denied"));
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory, mockFileSystem.Object);
            Assert.Throws<UnauthorizedAccessException>(() => provider.GetReviewRowTemplate());
        }

        [Fact]
        public void GetEmailTemplate_WhenIOExceptionOccurs_ThrowsIOException()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Throws(new IOException("IO error occurred"));
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory, mockFileSystem.Object);
            Assert.Throws<IOException>(() => provider.GetEmailTemplate());
        }

        [Fact]
        public void GetPlainTextTemplate_WhenIOExceptionOccurs_ThrowsIOException()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Throws(new IOException("IO error occurred"));
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory, mockFileSystem.Object);
            Assert.Throws<IOException>(() => provider.GetPlainTextTemplate());
        }

        [Fact]
        public void GetReviewRowTemplate_WhenIOExceptionOccurs_ThrowsIOException()
        {
            Mock<IFileSystem> mockFileSystem = new Mock<IFileSystem>();
            mockFileSystem.Setup(fs => fs.ReadAllText(It.IsAny<string>())).Throws(new IOException("IO error occurred"));
            TestFileTemplateProvider provider = new TestFileTemplateProvider(this.tempDirectory, mockFileSystem.Object);
            Assert.Throws<IOException>(() => provider.GetReviewRowTemplate());
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(this.tempDirectory, true);
            }
            catch
            {
            }

            Environment.SetEnvironmentVariable("BASEDIR", null);
        }
    }
}