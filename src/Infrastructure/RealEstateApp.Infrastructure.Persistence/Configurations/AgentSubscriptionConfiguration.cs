using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class AgentSubscriptionConfiguration : IEntityTypeConfiguration<AgentSubscription>
    {
        public void Configure(EntityTypeBuilder<AgentSubscription> builder)
        {
            builder.ToTable("AgentSubscriptions");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.AgentId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasIndex(s => s.AgentId);

            builder.HasOne(s => s.SubscriptionPlan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(s => s.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
