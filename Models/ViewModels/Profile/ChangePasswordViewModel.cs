using System.ComponentModel.DataAnnotations;

namespace EITSTMS.Models.ViewModels.Profile
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty; // 当前密码

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty; // 新密码

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = string.Empty; // 确认新密码
    }
}