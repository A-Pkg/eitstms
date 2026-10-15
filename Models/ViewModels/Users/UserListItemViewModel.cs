namespace EITSTMS.Models.ViewModels.Users
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty; // 用户 Id

        public string? UserName { get; set; } // 登录用户名

        public string EmployeeNumber { get; set; } = string.Empty; // 员工编号

        public string Name { get; set; } = string.Empty; // 用户姓名

        public bool IsActive { get; set; } // 账号是否启用

        public IList<string> Roles { get; set; } = new List<string>(); // 用户拥有的角色列表

        public bool CanEdit { get; set; } // 当前登录管理员是否可以编辑该用户
    }
}