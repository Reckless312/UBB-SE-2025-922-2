// <copyright file="EmailJob.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App1.Models;
using App1.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Quartz;

/// <summary>
/// email job.
/// </summary>
public class EmailJob : IJob
{
    private readonly IUserService userService;
    private readonly IReviewService reviewService;
    private readonly IConfiguration config;
    private readonly ITemplateProvider templateProvider;
    private readonly IEmailSender emailSender;

    public EmailJob(
        IUserService userService,
        IReviewService reviewService,
        IConfiguration configuration)
    {
        this.userService = userService;
        this.reviewService = reviewService;
        this.config = configuration;
        this.templateProvider = new FileTemplateProvider();
        this.emailSender = new SmtpEmailSender();
    }

    public EmailJob(
        IUserService userService,
        IReviewService reviewService,
        IConfiguration configuration,
        IEmailSender emailSender,
        ITemplateProvider templateProvider)
    {
        this.userService = userService;
        this.reviewService = reviewService;
        this.config = configuration;
        this.templateProvider = templateProvider;
        this.emailSender = emailSender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            string? smtpEmail = this.config["SMTP_MODERATOR_EMAIL"];
            string? smtpPassword = this.config["SMTP_MODERATOR_PASSWORD"];

            if (string.IsNullOrEmpty(smtpEmail) || string.IsNullOrEmpty(smtpPassword))
            {
                throw new Exception("SMTP credentials not configured in appsettings.json");
            }

            AdminReportData reportData = this.GatherReportData();

            string emailHtml = this.GenerateEmailContent(reportData);
            string emailText = this.GeneratePlainTextContent(reportData);

            foreach (User admin in reportData.AdminUsers)
            {
                MimeMessage message = new MimeMessage();
                message.From.Add(new MailboxAddress("System Admin", smtpEmail));
                message.To.Add(new MailboxAddress(admin.FullName, admin.EmailAddress));
                message.Subject = $"Admin Report - {reportData.ReportDate:yyyy-MM-dd}";

                var body = new BodyBuilder { HtmlBody = emailHtml, TextBody = emailText };

                message.Body = body.ToMessageBody();

                await this.emailSender.SendEmailAsync(message, smtpEmail, smtpPassword);
                System.Diagnostics.Debug.WriteLine($"Sent report to {admin.EmailAddress}");
            }

            System.Diagnostics.Debug.WriteLine("Email job completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Email Job Failed: {ex}");
        }
    }

    private AdminReportData GatherReportData()
    {
        DateTime reportDate = DateTime.Now;
        DateTime yesterday = reportDate.AddDays(-1);
        List<User> adminUsers = this.userService.GetAdminUsers();
        int activeUsersCount = this.userService.GetAdminUsers().Count() + this.userService.GetRegularUsers().Count();
        int bannedUsersCount = this.userService.GetBannedUsers().Count();
        int numberOfNewReviews = this.reviewService.GetReviewsSince(yesterday).Count;
        double averageRating = this.reviewService.GetAverageRatingForVisibleReviews();
        List<Review> recentReviews = this.reviewService.GetReviewsForReport();

        return new AdminReportData(reportDate, adminUsers, activeUsersCount, bannedUsersCount, numberOfNewReviews, averageRating, recentReviews);
    }

    private string GenerateEmailContent(AdminReportData data)
    {
        string emailTemplate = this.templateProvider.GetEmailTemplate();
        emailTemplate = emailTemplate.Replace("{{ReportDate}}", data.ReportDate.ToString("yyyy-MM-dd"))
                                     .Replace("{{ActiveUsersCount}}", data.ActiveUsersCount.ToString())
                                     .Replace("{{BannedUsersCount}}", data.BannedUsersCount.ToString())
                                     .Replace("{{NewReviewsCount}}", data.NewReviewsCount.ToString())
                                     .Replace("{{AverageRating}}", data.AverageRating.ToString("0.0"))
                                     .Replace("{{RecentReviewsHtml}}", this.GenerateRecentReviewsHtml(data.RecentReviews))
                                     .Replace("{{GeneratedAt}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        return emailTemplate;
    }

    private string GenerateRecentReviewsHtml(List<Review> reviews)
    {
        if (!reviews.Any())
        {
            return "<p>No recent reviews</p>";
        }

        StringBuilder html = new StringBuilder("<table border='1' cellpadding='5' style='border-collapse: collapse; width: 100%;'> <tr><th>User</th><th>Rating</th><th>Date</th></tr>");
        foreach (var review in reviews)
        {
            string row = this.templateProvider.GetReviewRowTemplate();
            string userName = this.userService.GetUserById(review.UserId).FullName;
            row = row.Replace("{{userName}}", userName)
                     .Replace("{{rating}}", review.Rating.ToString())
                     .Replace("{{creationDate}}", review.CreatedDate.ToString("yyyy-MM-dd"));
            html.Append(row);
        }

        html.Append("</table>");
        return html.ToString();
    }

    private string GeneratePlainTextContent(AdminReportData data)
    {
        string textTemplate = this.templateProvider.GetPlainTextTemplate();
        textTemplate = textTemplate.Replace("{{ReportDate}}", data.ReportDate.ToString("yyyy-MM-dd"))
                                   .Replace("{{ActiveUsersCount}}", data.ActiveUsersCount.ToString())
                                   .Replace("{{BannedUsersCount}}", data.BannedUsersCount.ToString())
                                   .Replace("{{AverageRating}}", data.AverageRating.ToString("0.0"))
                                   .Replace("{{GeneratedAt}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        return textTemplate;
    }
}
