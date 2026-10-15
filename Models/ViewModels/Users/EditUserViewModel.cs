using System.ComponentModel.DataAnnotations;

namespace EITSTMS.Models.ViewModels.Users
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty; // 用户 Id，用于确定编辑的是哪个用户

        [Required]
        public string UserName { get; set; } = string.Empty; // 登录用户名

        [Required]
        public string EmployeeNumber { get; set; } = string.Empty; // 员工编号

        [Required]
        public string Name { get; set; } = string.Empty; // 用户姓名

        [Required]
        public string Role { get; set; } = string.Empty; // 当前选择的角色

        public bool IsActive { get; set; } // 账号是否启用

        public List<string> AvailableRoles { get; set; } = new List<string>(); // 页面可选择的角色列表
    }
}