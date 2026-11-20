using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class PropertyDocumentConfiguration : IEntityTypeConfiguration<PropertyDocument>
    {
        public void Configure(EntityTypeBuilder<PropertyDocument> builder)
        {
            builder.ToTable("PropertyDocuments");
            builder.HasKey(d => d.Id);

            builder.Property(d => d.DocumentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.FileUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.ContentType)
                .HasMaxLength(100);

            builder.Property(d => d.UploadedBy)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasOne(d => d.Property)
                .WithMany()
                .HasForeignKey(d => d.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(d => d.PropertyId);
        }
    }
}
