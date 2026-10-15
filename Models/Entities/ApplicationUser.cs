using Microsoft.AspNetCore.Identity;

namespace EITSTMS.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string EmployeeNumber { get; set; } = string.Empty;   // 员工编号，例如 EMP001

        public string Name { get; set; } = string.Empty;             // 员工姓名

        public int? DepartmentId { get; set; }                       // 所属部门外键，允许为空

        public bool IsActive { get; set; } = true;                   // 业务层面账号是否启用

        public bool IsSystemAdmin { get; set; } = false;            // 是否为系统初始最高权限管理员

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   // 用户创建时间，统一保存 UTC

        public Department? Department { get; set; }                  // 导航属性：所属部门完整对象
    }
}
