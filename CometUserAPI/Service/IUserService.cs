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
        Task<APIResponse> UpdatePassword(string userName, string password, string otpText);
        Task<APIResponse> UpdateStatus(Updatestatus userStatus);
        Task<APIResponse> UpdateRole(UpdateRole updateRole);
        Task<List<UserModel>> GetAll();
        Task<UserModel> GetByCode(string code);
    }
}
