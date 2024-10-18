using KVATUM_EMAIL_SERVICE.Core.Entities;

namespace KVATUM_EMAIL_SERVICE.Core.IService
{
    public interface IEmailService
    {
        Task<ServiceResponse<string>> SendMessage(string email, string subject, string message);
        Task<ServiceResponse<string>> SendMessage(string fromEmail, string toEmail, string subject, string message);
    }
}