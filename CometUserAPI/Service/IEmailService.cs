using CometUserAPI.Model;

namespace CometUserAPI.Service
{
    public interface IEmailService
    {
        Task SendEmail(MailRequest mailRequest);
    }
}
