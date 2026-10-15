using System.ComponentModel.DataAnnotations;

namespace EITSTMS.Models.ViewModels.Users
{
    public class CreateUserViewModel
    {
        [Required]
        public string UserName { get; set; } = string.Empty; // 登录用户名

        [Required]
        public string EmployeeNumber { get; set; } = string.Empty; // 员工编号

        [Required]
        public string Name { get; set; } = string.Empty; // 用户姓名

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty; // 新用户初始密码

        [Required]
        public string Role { get; set; } = string.Empty; // 创建时选择的角色

        public List<string> AvailableRoles { get; set; } = new List<string>(); // 页面可选择的角色列表
    }
}