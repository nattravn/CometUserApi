using CometUserAPI.Model;
using CometUserAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CometUserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            this._userService = userService;
        }

        [HttpPost("userregistration")]
        public async Task<ActionResult> UserRegistration(UserRegistration userRegistration)
        {
            var data= await this._userService.UserRegistration(userRegistration);
            return Ok(data);
        }

        [HttpPost("confirmregistration")]
        public async Task<ActionResult> ConfirmRegistration(int userId, string userName, string otptext)
        {
            var data = await this._userService.ConfirmRegistration(userId, userName, otptext);
            return Ok(data);
        }

        [HttpPost("resetpassword")]
        public async Task<ActionResult> resetPassword(ResetPassword _data)
        {
            var data = await this._userService.ResetPassword(_data.userName, _data.oldPassword, _data.newPassword);
            return Ok(data);
        }

        [HttpGet("forgetpassword")]
        public async Task<ActionResult> forgetPassword(string userName)
        {
            var data = await this._userService.ForgetPassword(userName);
            return Ok(data);
        }

        [HttpPost("updatepassword")]
        public async Task<ActionResult> updatePassword(string userName, string password, string otpText)
        {
            var data = await this._userService.UpdatePassword(userName, password, otpText);
            return Ok(data);
        }

        [HttpPost("updatestatus")]
        public async Task<ActionResult> updateStatus(string userName, bool status)
        {
            var data = await this._userService.UpdateStatus(userName, status);
            return Ok(data);
        }

        [HttpPost("updaterole")]
        public async Task<ActionResult> updateRole(string userName, string role)
        {
            var data = await this._userService.UpdateRole(userName, role);
            return Ok(data);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var data = await this._userService.GetAll();
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }

        [HttpGet("Getbycode")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var data = await this._userService.GetByCode(code);
            if (data == null)
            {
                return NotFound();
            }
            return Ok(data);
        }
    }
}
