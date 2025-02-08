using AutoMapper;
using CometUserAPI.Entities;
using CometUserAPI.Helper;
using CometUserAPI.Model;
using CometUserAPI.Service;
using Microsoft.EntityFrameworkCore;

namespace CometUserAPI.Container
{
    public class UserService : IUserService
    {
        private readonly CometUserDBContext _dbContext;
        private readonly IMapper mapper;
        public UserService(CometUserDBContext context, IMapper mapper)
        {
            this._dbContext = context;
            this.mapper = mapper;
        }

        public async Task<APIResponse> ConfirmRegistration(int userId, string userName, string otpText)
        {
            APIResponse response = new APIResponse();
            bool otpResponse = await ValidateOTP(userName, otpText);

            if (!otpResponse)
            {
                response.Result = "fail";
                response.ErrorMessage = "Invalid OTP or Expired";
            } 
            else
            {
                var _tempData = await _dbContext.TblTempusers.FirstOrDefaultAsync(item => item.Id == userId);
                var _user = new TblUser()
                {
                    Username = userName,
                    Email = _tempData.Email,
                    Name = _tempData.Name,
                    Password = _tempData.Password,
                    Phone = _tempData.Phone,
                    Failattempt = 0,
                    Isactive = true,
                    Islocked = false,
                    Role = "user"
                };

                await this._dbContext.TblUsers.AddAsync(_user);
                await this._dbContext.SaveChangesAsync();
                await UpdatePWDManager(userName, _tempData.Password);
                response.Result = "Register successfully!";
            }

            return response;
        }

        public async Task<APIResponse> ResetPassword(string userName, string oldPassword, string newPassword)
        {
            APIResponse response = new APIResponse();
            var _user = await this._dbContext.TblUsers.FirstOrDefaultAsync(item => item.Username == userName && item.Password == oldPassword && item.Isactive == true);
            if (_user != null)
            {
                var _pwdHistory = await ValidatePwdHistory(userName, newPassword);
                if(_pwdHistory)
                {
                    response.Result = "Failed to validate old password that used in last 3 transaction";
                }
                else
                {
                    _user.Password = newPassword;
                    await this._dbContext.SaveChangesAsync();
                    await UpdatePWDManager(userName, newPassword);
                    response.Result = "Password changed";
                }
                
            }
            else
            {
                response.Result = "fail";
                response.ErrorMessage = "Failed to validate old password";
            }

            return response;
        }

        public async Task<APIResponse> UserRegistration(UserRegistration userRegistration)
        {
            APIResponse response = new APIResponse();
            int userId = 0;
            bool isValid = true;

            try
            {
                // duplicate user
                var _user = this._dbContext.TblUsers.Where(item => item.Username == userRegistration.UserName).ToList();
                if (_user.Count > 0)
                {
                    isValid = false;
                    response.Result = "fail";
                    response.ErrorMessage = "Duplicated user name";
                }

                // duplicate Email
                var _userEmail = this._dbContext.TblUsers.Where(item => item.Email == userRegistration.Email).ToList();
                if(_userEmail.Count > 0)
                {
                    isValid = false;
                    response.Result = "fail";
                    response.ErrorMessage = "Duplicated email";
                }

                if (userRegistration != null && isValid)
                {
                    var _tempUser = new TblTempuser()
                    {
                        Code = userRegistration.UserName,
                        Email = userRegistration.Email,
                        Name = userRegistration.Name,
                        Password = userRegistration.Password,
                        Phone = userRegistration.Phone
                    };
                    await this._dbContext.AddAsync(_tempUser);
                    await this._dbContext.SaveChangesAsync();
                    userId = _tempUser.Id;
                    string otpText = GenerateRandomNumber();
                    await UpdateOtp(userRegistration.UserName, otpText, "register");
                    await SendOtpMail(userRegistration.Email, otpText, userRegistration.Name);
                    response.Result = "Pass! Id:" + userId.ToString();
                }
            }
            catch (Exception ex)
            {
                response.Result = "fail";
            }
            return response;
        }

        public async Task<APIResponse> ForgetPassword(string userName)
        {
            APIResponse response = new APIResponse();
            var _user = await this._dbContext.TblUsers.FirstOrDefaultAsync(item => item.Username == userName && item.Islocked != true);
            if(_user != null)
            {
                string otptext = GenerateRandomNumber();
                await UpdateOtp(userName, otptext, "forgetPassword");
                await SendOtpMail(_user.Email, otptext, _user.Name);
                response.Result = "OTP sent";
            } else
            {
                response.Result = "Failed";
                response.ErrorMessage = "Invalid user";
            }
            
            return response;
        }

        public async Task<APIResponse> UpdatePassword(string userName, string password, string otpText)
        {
            APIResponse response = new APIResponse();

            bool otpValidation = await ValidateOTP(userName, otpText);
            if (otpValidation)
            {
                bool pwdHistory = await ValidatePwdHistory(userName, password);
                if (pwdHistory)
                {
                    response.Result = "fail";
                    response.ErrorMessage = "Failed to validate old password that used in last 3 transaction";
                }
                else
                {
                    var _user = await _dbContext.TblUsers.FirstOrDefaultAsync(item => item.Username == userName && item.Islocked == true);
                    if (_user != null)
                    {
                        _user.Password = password;
                        await UpdatePWDManager(userName, password);
                        await this._dbContext.SaveChangesAsync();
                        response.Result = "Password changed!";
                    }
                }
            } 
            else
            {
                response.Result = "Failed";
                response.ErrorMessage = "Invalid OTP";
            }
            return response;
        }

        public async Task<APIResponse> UpdateStatus(string userName, bool userStatus)
        {
            APIResponse response = new APIResponse();
            var _user = await this._dbContext.TblUsers.FirstOrDefaultAsync(item => item.Username == userName && item.Isactive == true);

            if (_user != null)
            {
                _user.Isactive = userStatus;
                await this._dbContext.SaveChangesAsync();
                response.Result = "Status changed";
            } else
            {
                response.Result = "Failed";
                response.ErrorMessage = "Invalid user";
            }
            return response;
        }

        public async Task<APIResponse> UpdateRole(string userName, string userRole)
        {
            APIResponse response = new APIResponse();
            var _user = await this._dbContext.TblUsers.FirstOrDefaultAsync(item => item.Username == userName && item.Isactive == true);

            if (_user != null)
            {
                _user.Role = userRole;
                await this._dbContext.SaveChangesAsync();
                response.Result = "User role changed";
            }
            else
            {
                response.Result = "Failed";
                response.ErrorMessage = "Invalid user";
            }
            return response;
        }

        private async Task UpdateOtp(string username, string otptext, string otptype)
        {
            var _opt = new TblOtpManager()
            {
                Username = username,
                Otptext = otptext,
                Createddate = DateTime.Now,
                Expiration = DateTime.Now.AddMinutes(30),
                Otptype = otptype
            };

            await this._dbContext.TblOtpManagers.AddAsync(_opt);
            await this._dbContext.SaveChangesAsync();
        }

        private async Task UpdatePWDManager(string username, string password)
        {
            var _opt = new TblPwdManger()
            {
                Username = username,
                Password = password,
                ModifyDate = DateTime.Now
            };

            await this._dbContext.TblPwdMangers.AddAsync(_opt);
            await this._dbContext.SaveChangesAsync();
        }

        private async Task<bool> ValidateOTP(string username, string OTPText)
        {
            bool response = false;
            var _data = await this._dbContext.TblOtpManagers.FirstOrDefaultAsync(item => item.Username == username
            && item.Otptext == OTPText && item.Expiration > DateTime.Now);
            if(_data != null)
            {
                response = true;
            }
            return response;
        }

        private string GenerateRandomNumber()
        {
            Random random = new Random();
            string randomNumber = random.Next(0, 100000).ToString("D6");
            return randomNumber;
        }

        private async Task SendOtpMail(string userEmail, string optText, string name)
        {

        }

        private async Task<bool> ValidatePwdHistory(string userName, string password)
        {
            bool response = false;
            var _pwd = await this._dbContext.TblPwdMangers.Where(item => item.Username == userName).
                OrderByDescending(p=>p.ModifyDate).Take(10).ToListAsync();
            if(_pwd.Count > 0) 
            { 
                var validate = _pwd.Where(o => o.Password == password).ToList();
                if (validate.Any())
                {
                    response = true;
                }
            }
            return response;
        }

        public async Task<List<UserModel>> GetAll()
        {
            List<UserModel> _response = new List<UserModel>();
            var _data = await this._dbContext.TblUsers.ToListAsync();
            if (_data != null)
            {
                _response = this.mapper.Map<List<TblUser>, List<UserModel>>(_data);
            }
            return _response;
        }

        public async Task<UserModel> GetByCode(string code)
        {
            UserModel _response = new UserModel();
            var _data = await this._dbContext.TblUsers.FindAsync(code);
            if (_data != null)
            {
                _response = this.mapper.Map<TblUser, UserModel>(_data);
            }
            return _response;
        }
    }

}
