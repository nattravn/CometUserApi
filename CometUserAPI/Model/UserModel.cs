using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CometUserAPI.Model
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool? Isactive { get; set; }
        public string? Statusname { get; set; }
        public string Role { get; set; }
    }

    public class UserRegister
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
    }

    public class Confirmpassword
    {
        public int userid { get; set; }
        public string username { get; set; }
        public string otptext { get; set; }
    }

    public class ResetPassword
    {
        public string userName { get; set; }
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
    }

    public class Updatepassword
    {
        public string username { get; set; }
        public string password { get; set; }
        public string otptext { get; set; }
    }

    public class Updatestatus
    {
        public string username { get; set; }
        public bool status { get; set; }
    }

    public class UpdateRole
    {
        public string username { get; set; }
        public string role { get; set; }
    }
}

