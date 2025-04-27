namespace App1.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using MimeKit;

    public interface IEmailSender
    {
        Task SendEmailAsync(MimeMessage message, string smtpEmail, string smtpPassword);
    }
}
