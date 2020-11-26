using Code.Hub.Shared.Configurations.Email;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;

namespace Code.Hub.App.Configurations.Email
{
    public class CustomEmailSender : IEmailSender
    {
        public CustomEmailSender(IOptions<CustomEmailSenderSettings> optionsAccessor, IHostEnvironment env)
        {
            Options = optionsAccessor.Value;
        }

        public CustomEmailSenderSettings Options { get; } //set only via Secret Manager


        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var mimeMessage = new MimeMessage();

            mimeMessage.From.Add(new MailboxAddress(Options.SenderName, Options.Sender));

            mimeMessage.To.Add(new MailboxAddress(email, email));

            mimeMessage.Subject = subject;

            mimeMessage.Body = new TextPart("html")
            {
                Text = message
            };

            // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
            using var client = new SmtpClient { ServerCertificateValidationCallback = (s, c, h, e) => true };

            await client.ConnectAsync(Options.MailServer);

            // Note: only needed if the SMTP server requires authentication
            await client.AuthenticateAsync(Options.Sender, Options.Password);

            await client.SendAsync(mimeMessage);

            await client.DisconnectAsync(true);
        }
    }
}
