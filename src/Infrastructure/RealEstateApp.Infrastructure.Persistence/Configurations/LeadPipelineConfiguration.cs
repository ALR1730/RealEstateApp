using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class LeadPipelineConfiguration : IEntityTypeConfiguration<LeadPipeline>
    {
        public void Configure(EntityTypeBuilder<LeadPipeline> builder)
        {
            builder.ToTable("LeadPipelines");
            builder.HasKey(lp => lp.Id);

            builder.Property(lp => lp.LeadName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(lp => lp.LeadEmail)
                .HasMaxLength(200);

            builder.Property(lp => lp.LeadPhone)
                .HasMaxLength(30);

            builder.Property(lp => lp.Notes)
                .HasMaxLength(2000);

            builder.Property(lp => lp.AgentId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(lp => lp.Stage)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue(PipelineStage.NewLead);

            builder.Property(lp => lp.Priority)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Normal");

            builder.Property(lp => lp.Source)
                .HasMaxLength(100);

            builder.Property(lp => lp.EstimatedBudget)
                .HasColumnType("decimal(18,2)");

            builder.Property(lp => lp.BudgetCurrency)
                .HasMaxLength(3);

            builder.HasOne(lp => lp.Property)
                .WithMany()
                .HasForeignKey(lp => lp.PropertyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(lp => lp.AgentId);
            builder.HasIndex(lp => lp.Stage);
        }
    }
}
