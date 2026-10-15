using System.ComponentModel.DataAnnotations;

namespace EITSTMS.Models.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty; // 登录用户名

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // 登录密码

        public bool RememberMe { get; set; } // 是否记住登录状态
    }
}