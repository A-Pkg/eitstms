using EITSTMS.Models.Entities;
using EITSTMS.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EITSTMS.Data.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.Property(t => t.TicketNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(t => t.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(t => t.Description)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(t => t.InformationRequest)
                .HasMaxLength(1000);

            builder.Property(t => t.Resolution)
                .HasMaxLength(1000);

            builder.Property(t => t.Status)
                .HasDefaultValue(TicketStatus.Open);

            builder.Property(t => t.Priority)
                .HasDefaultValue(TicketPriority.Normal)   // 数据库默认值：如果未提供 Priority，则使用 Normal
                .HasSentinel((TicketPriority)(-1));       // EF Core 哨兵值：当 Priority 为 -1 时，视为“未设置”，让数据库默认值生效

            builder.Property(t => t.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasIndex(t => t.TicketNumber)
                .IsUnique();

            builder.HasIndex(t => t.Status);

            builder.HasIndex(t => t.Priority);

            builder.HasIndex(t => t.CreatedById);

            builder.HasIndex(t => new { t.AssignedToId, t.Status });

            builder.HasIndex(t => t.CategoryId);

            builder.HasIndex(t => t.CreatedAt);

            builder.HasIndex(t => t.AutoCloseAt)
                .HasFilter("[AutoCloseAt] IS NOT NULL");

            builder.HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.AssignedTo)
                .WithMany()
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.RelatedAsset)
                .WithMany(a => a.Tickets)
                .HasForeignKey(t => t.RelatedAssetId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}