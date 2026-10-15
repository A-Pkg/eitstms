using EITSTMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EITSTMS.Data.Configurations
{
    public class TicketSupplementConfiguration : IEntityTypeConfiguration<TicketSupplement>
    {
        public void Configure(EntityTypeBuilder<TicketSupplement> builder)
        {
            builder.HasKey(s => s.SupplementId);

            builder.Property(s => s.Content)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(s => s.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasIndex(s => new { s.TicketId, s.CreatedAt });

            builder.HasOne(s => s.Ticket)
                .WithMany(t => t.Supplements)
                .HasForeignKey(s => s.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}