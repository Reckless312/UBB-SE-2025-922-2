using DataAccess.Service.AdminDashboard.Interfaces;

namespace DataAccess.Service.AdminDashboard.Components
{
    public class FileTemplateProvider : ITemplateProvider
    {
        private static readonly string EmailPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "EmailContentTemplate.html");
        private static readonly string PlainTextPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "PlainTextContentTemplate.txt");
        private static readonly string ReviewPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "RecentReviewForReportTemplate.html");

        public string GetEmailTemplate()
        {
            return File.ReadAllText(FileTemplateProvider.EmailPath);
        }

        public string GetPlainTextTemplate()
        {
            return File.ReadAllText(FileTemplateProvider.PlainTextPath);
        }

        public string GetReviewRowTemplate()
        {
            return File.ReadAllText(FileTemplateProvider.ReviewPath);
        }
    }
}
