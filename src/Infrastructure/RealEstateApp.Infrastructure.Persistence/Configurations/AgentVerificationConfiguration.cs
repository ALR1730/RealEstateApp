using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class AgentVerificationConfiguration : IEntityTypeConfiguration<AgentVerification>
    {
        public void Configure(EntityTypeBuilder<AgentVerification> builder)
        {
            builder.ToTable("AgentVerifications");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.AgentId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasIndex(v => v.AgentId)
                .IsUnique();

            builder.Property(v => v.Cedula)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(v => v.CedulaFrontImageUrl)
                .HasMaxLength(500);

            builder.Property(v => v.CedulaBackImageUrl)
                .HasMaxLength(500);

            builder.Property(v => v.Status)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue(VerificationStatus.Pending);

            builder.Property(v => v.RejectionReason)
                .HasMaxLength(1000);

            builder.Property(v => v.ReviewedByAdminId)
                .HasMaxLength(450);
        }
    }
}
