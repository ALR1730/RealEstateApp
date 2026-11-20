using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class ChatConfiguration : IEntityTypeConfiguration<Chat>
    {
        public void Configure(EntityTypeBuilder<Chat> builder)
        {
            builder.ToTable("Chats");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.ClienteId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(c => c.AgenteId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(c => c.SenderId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(c => c.MessageContent)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(c => c.PropertyId)
                .IsRequired(false);

            builder.HasOne(c => c.Property)
                .WithMany(p => p.Chats)
                .HasForeignKey(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
