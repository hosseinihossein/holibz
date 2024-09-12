using System;
using MailKit;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;

namespace holibz.Models
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string receiverUserName, string receiverEmail, string subject, string message);
    }

    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string receiverUserName, string receiverEmail, string subject, string message)
        {
            string senderEmail = "ahsgfdajhsgt@gmail.com";
            string senderGmailPassword = "wnmwahawpaexytmh";
            string senderName = "Admin";

            MimeMessage email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName, senderEmail));
            email.To.Add(new MailboxAddress(receiverUserName, receiverEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (SmtpClient smtp = new SmtpClient())
            {
                await smtp.ConnectAsync("smtp.gmail.com", 587, false);
                await smtp.AuthenticateAsync(senderEmail, senderGmailPassword);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }

        }
    }

    /*public class EmailValidationCodeDBModel(string userGuid, string validationCode)
    {
        public int Id { get; set; }
        public string UserGuid { get; set; } = userGuid;
        public string ValidationCode { get; set; } = validationCode;
        public DateTime Date { get; set; } = DateTime.Now;
    }
    public class EmailValidationCodeDBContext : DbContext
    {
        public EmailValidationCodeDBContext(DbContextOptions<EmailValidationCodeDBContext> options)
        : base(options) { }

        public DbSet<EmailValidationCodeDBModel> UserValidationCodes { get; set; } = null!;
    }*/
}