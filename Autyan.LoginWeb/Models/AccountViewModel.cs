using System.ComponentModel.DataAnnotations;

namespace Autyan.LoginWeb.Models
{
    public class UserRegisterViewModel
    {
        [Display(Name = "用户名")]
        [Required]
        public string UserName { get; set; }

        [Display(Name = "密码")]
        [Required]
        public string Password { get; set; }

        [Display(Name = "确认密码")]
        [Compare(nameof(Password))]
        [Required]
        public string ConfirmPassword { get; set; }
    }

    public class SignInViewModel
    {
        [Display(Name = "用户名")]
        public string UserName { get; set; }

        [Display(Name = "密码")]
        public string Password { get; set; }
    }
}