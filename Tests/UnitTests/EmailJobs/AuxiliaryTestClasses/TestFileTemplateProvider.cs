namespace UnitTests.EmailJobs.AuxiliaryTestClasses
{
    using App1.Services;

    public class TestFileTemplateProvider : ITemplateProvider
    {
        private readonly string baseDirectory;
        private readonly IFileSystem fileSystem;

        public TestFileTemplateProvider(string baseDirectory, IFileSystem fileSystem = null)
        {
            this.baseDirectory = baseDirectory;
            this.fileSystem = fileSystem ?? new DefaultFileSystem();
        }

        public string GetEmailTemplate()
        {
            string path = Path.Combine(this.baseDirectory, "Templates", "EmailContentTemplate.html");
            return this.fileSystem.ReadAllText(path);
        }

        public string GetPlainTextTemplate()
        {
            string path = Path.Combine(this.baseDirectory, "Templates", "PlainTextContentTemplate.txt");
            return this.fileSystem.ReadAllText(path);
        }

        public string GetReviewRowTemplate()
        {
            string path = Path.Combine(this.baseDirectory, "Templates", "RecentReviewForReportTemplate.html");
            return this.fileSystem.ReadAllText(path);
        }
    }
}