// <copyright file="FileTemplateProvider.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace DrinkDb_Auth.Service.AdminDashboard.Components
{
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class FileTemplateProvider : ITemplateProvider
    {
        private static readonly string EmailPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "EmailContentTemplate.html");
        private static readonly string PlainTextPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "PlainTextContentTemplate.txt");
        private static readonly string ReviewPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "RecentReviewForReportTemplate.html");

        public string GetEmailTemplate()
        {
            return File.ReadAllText(EmailPath);
        }

        public string GetPlainTextTemplate()
        {
            return File.ReadAllText(PlainTextPath);
        }

        public string GetReviewRowTemplate()
        {
            return File.ReadAllText(ReviewPath);
        }
    }
}
