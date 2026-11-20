using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class AgentReviewConfiguration : IEntityTypeConfiguration<AgentReview>
    {
        public void Configure(EntityTypeBuilder<AgentReview> builder)
        {
            builder.ToTable("AgentReviews");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.AgentId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.ClienteId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Comment)
                .HasMaxLength(1000);

            builder.HasIndex(r => new { r.ClienteId, r.AgentId, r.PropertyId })
                .IsUnique();

            builder.HasOne(r => r.Property)
                .WithMany()
                .HasForeignKey(r => r.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => r.AgentId);
        }
    }
}
