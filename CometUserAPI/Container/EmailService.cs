using CometUserAPI.Model;
using CometUserAPI.Service;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using MimeKit;


namespace CometUserAPI.Container
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings emailSettings;
        public EmailService(IOptions<EmailSettings> options) { 
            this.emailSettings = options.Value;
        }
        public async Task SendEmail(MailRequest mailRequest)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(emailSettings.Email);
            email.To.Add(MailboxAddress.Parse(mailRequest.Email));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = mailRequest.EmailBody;
            email.Body=builder.ToMessageBody();

            using var smptp = new SmtpClient();

            smptp.CheckCertificateRevocation = false;
            smptp.Connect(emailSettings.Host, emailSettings.Port,SecureSocketOptions.StartTls);
            smptp.Authenticate(emailSettings.Email, emailSettings.Password);
            await smptp.SendAsync(email);
            smptp.Disconnect(true);
        }
    }
}
