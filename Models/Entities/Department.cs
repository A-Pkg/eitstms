namespace EITSTMS.Models.Entities
{
    public class Department
    {
        public int DepartmentId { get; set; }                        // 部门主键

        public string DepartmentName { get; set; } = string.Empty;   // 部门名称

        public string? Description { get; set; }                     // 部门说明，可为空

        public bool IsActive { get; set; } = true;                   // 部门是否启用

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   // 部门创建时间，统一保存 UTC

        // 导航属性：该部门下的所有用户
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
