namespace DataAccess.Service.AdminDashboard.Components
{
    using System;
    using System.Threading.Tasks;
    using Service.AdminDashboard.Interfaces;
    using MailKit.Net.Smtp;
    using MimeKit;

    public class SmtpEmailSender : IEmailSender
    {
        public async Task SendEmailAsync(MimeMessage message, string smtpEmail, string smtpPassword)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (string.IsNullOrEmpty(smtpEmail))
            {
                throw new ArgumentNullException(nameof(smtpEmail));
            }

            if (string.IsNullOrEmpty(smtpPassword))
            {
                throw new ArgumentNullException(nameof(smtpPassword));
            }

            try
            {
                using SmtpClient client = new SmtpClient();
                await client.ConnectAsync("smtp.gmail.com", 587, false);
                await client.AuthenticateAsync(smtpEmail, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch
            {
            }
        }
    }
}