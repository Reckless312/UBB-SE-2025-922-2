namespace DrinkDb_Auth.Service.AdminDashboard.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;
    using System.Threading.Tasks;
    using DrinkDb_Auth.Service.AdminDashboard.Interfaces;
    using MailKit.Net.Smtp;
    using Microsoft.Extensions.Configuration;
    using MimeKit;

    public class SmtpEmailSender : IEmailSender
    {
        private const string SmtpServerHost = "smtp.gmail.com";
        private const int SmtpServerPort = 587;
        private const bool UseSsl = false;
        private const bool DisconnectQuit = true;

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
                await client.ConnectAsync(SmtpServerHost, SmtpServerPort, UseSsl);
                await client.AuthenticateAsync(smtpEmail, smtpPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(DisconnectQuit);
            }
            catch
            {
            }
        }
    }
}