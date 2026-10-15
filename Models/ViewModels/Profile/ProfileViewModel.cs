namespace EITSTMS.Models.ViewModels.Profile
{
    public class ProfileViewModel
    {
        public string UserName { get; set; } = string.Empty; // 登录用户名

        public string EmployeeNumber { get; set; } = string.Empty; // 员工编号

        public string Name { get; set; } = string.Empty; // 姓名

        public string DepartmentName { get; set; } = string.Empty; // 所属部门

        public string Role { get; set; } = string.Empty; // 当前角色

        public bool IsActive { get; set; } // 账号是否启用

        public DateTime CreatedAt { get; set; } // 账号创建时间
    }
}