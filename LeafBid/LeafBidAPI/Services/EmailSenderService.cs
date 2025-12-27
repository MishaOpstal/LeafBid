using System.Net;
using System.Net.Mail;
using LeafBidAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LeafBidAPI.Services
{
    public class EmailSenderService(IOptions<EmailSettings> emailSettings) : IEmailSender<User>
    {
        private readonly EmailSettings _emailSettings = emailSettings.Value;

        public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
        {
            return Execute(_emailSettings.Subject,
                $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>link</a>", email);
        }

        public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
        {
            return Execute(_emailSettings.Subject,
                $"Please reset your password by clicking this link: <a href='{resetLink}'>link</a>", email);
        }

        public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
        {
            return Execute(_emailSettings.Subject, $"Your password reset code is: {resetCode}", email);
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(subject, htmlMessage, email);
        }

        private async Task Execute(string subject, string message, string email)
        {
            try
            {
                string toEmail = string.IsNullOrEmpty(email) ? _emailSettings.ToEmail : email;
                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.UsernameEmail, "LeafBid")
                };
                mail.To.Add(new MailAddress(toEmail));
                mail.Subject = "LeafBid - " + subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.High;

                using SmtpClient smtp = new(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort);
                smtp.Credentials =
                    new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception occured whilst sending an email: " + ex.Message);
            }
        }
    }

    public class DummyEmailSender : IEmailSender<User>
    {
        public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
        {
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
        {
            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
        {
            return Task.CompletedTask;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }

    public class EmailSettings
    {
        public required string PrimaryDomain { get; init; }
        public required int PrimaryPort { get; init; }
        public required string UsernameEmail { get; init; }
        public required string UsernamePassword { get; init; }
        public required string FromEmail { get; init; }
        public required string ToEmail { get; init; }
        public required string Subject { get; init; }
    }
}