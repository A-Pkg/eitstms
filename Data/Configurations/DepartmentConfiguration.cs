using EITSTMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EITSTMS.Data.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.Property(d => d.DepartmentName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(d => d.Description)
                .HasMaxLength(300);

            builder.Property(d => d.IsActive)
                .HasDefaultValue(true);

            builder.Property(d => d.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");    //SysUTCDateTime

            builder.HasIndex(d => d.DepartmentName)
                .IsUnique();
        }
    }
}