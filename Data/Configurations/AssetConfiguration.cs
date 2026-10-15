using EITSTMS.Models.Entities;
using EITSTMS.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EITSTMS.Data.Configurations
{
    public class AssetConfiguration : IEntityTypeConfiguration<Asset>
    {
        public void Configure(EntityTypeBuilder<Asset> builder)
        {
            builder.Property(a => a.AssetNumber)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(a => a.AssetName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(a => a.Manufacturer)
                .HasMaxLength(100);

            builder.Property(a => a.Model)
                .HasMaxLength(100);

            builder.Property(a => a.SerialNumber)
                .HasMaxLength(100);

            builder.Property(a => a.Status)
                .HasDefaultValue(AssetStatus.Available)   // 数据库默认值：如果未提供 Status，则使用 Available
                .HasSentinel((AssetStatus)(-1));          // EF Core 哨兵值：当 Status 为 -1 时，视为“未设置”，让数据库默认值生效

            builder.Property(a => a.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasIndex(a => a.AssetNumber)
                .IsUnique();

            builder.HasIndex(a => a.SerialNumber)
                .IsUnique()
                .HasFilter("[SerialNumber] IS NOT NULL");   // 只让 SerialNumber 不为空的记录参与唯一索引

            builder.HasIndex(a => a.Category);

            builder.HasIndex(a => a.Status);
        }
    }
}