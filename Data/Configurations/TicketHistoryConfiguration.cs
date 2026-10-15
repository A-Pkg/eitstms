using EITSTMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EITSTMS.Data.Configurations
{
    public class TicketHistoryConfiguration : IEntityTypeConfiguration<TicketHistory>
    {
        public void Configure(EntityTypeBuilder<TicketHistory> builder)
        {
            builder.HasKey(h => h.HistoryId);

            builder.Property(h => h.Description)
                .HasMaxLength(1000);

            builder.Property(h => h.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasIndex(h => new { h.TicketId, h.CreatedAt });

            builder.HasIndex(h => h.ActionType);

            builder.HasOne(h => h.Ticket)
                .WithMany(t => t.Histories)
                .HasForeignKey(h => h.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.PerformedBy)
                .WithMany()
                .HasForeignKey(h => h.PerformedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}