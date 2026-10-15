using EITSTMS.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EITSTMS.Data.Configurations
{
    public class TicketNumberCounterConfiguration : IEntityTypeConfiguration<TicketNumberCounter>
    {
        public void Configure(EntityTypeBuilder<TicketNumberCounter> builder)
        {
            builder.HasKey(c => c.CounterDate);

            builder.Property(c => c.CounterDate)
                .HasColumnType("date");

            builder.Property(c => c.CurrentNumber)
                .IsRequired();
        }
    }
}