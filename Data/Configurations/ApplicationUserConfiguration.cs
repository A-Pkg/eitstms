using EITSTMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EITSTMS.Data.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.EmployeeNumber)
                .HasMaxLength(50)
                .IsRequired();      //告诉 EF Core，这个属性在数据库里不能为空

            builder.Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Property(u => u.IsSystemAdmin)
                .HasDefaultValue(false);       // 默认不是系统最高权限管理员

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");    // 数据库层默认使用 SQL Server 当前 UTC 时间

            builder.HasIndex(u => u.EmployeeNumber)
                .IsUnique();

            builder.HasOne(u => u.Department)                  // 一个 ApplicationUser 对应一个 Department
                .WithMany(d => d.Users)                        // 一个 Department 可以对应多个 ApplicationUser
                .HasForeignKey(u => u.DepartmentId)            // 使用 ApplicationUser.DepartmentId 作为外键
                .OnDelete(DeleteBehavior.Restrict);             // 限制，如果部门仍被用户引用，则禁止删除该部门
        }
    }
}