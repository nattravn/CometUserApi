using CometUserAPI.Helper;
using CometUserAPI.Model;

namespace CometUserAPI.Service
{
    public interface IUserService
    {
        Task<APIResponse> UserRegistration(UserRegistration userRegistration);
        Task<APIResponse> ConfirmRegistration(int userId, string userName, string otptext);
        Task<APIResponse> ResetPassword(string userName, string oldPassword, string newPassword);
        Task<APIResponse> ForgetPassword(string userName);
        Task<APIResponse> UpdatePassword(string userName, string password, string otptext);
        Task<APIResponse> UpdateStatus(string userName, bool userStatus);
        Task<APIResponse> UpdateRole(string userName, string userRole);
        Task<List<UserModel>> GetAll();
        Task<UserModel> GetByCode(string code);
    }
}
