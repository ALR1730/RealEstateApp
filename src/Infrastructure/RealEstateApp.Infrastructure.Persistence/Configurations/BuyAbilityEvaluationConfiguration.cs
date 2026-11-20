using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Configurations
{
    public sealed class BuyAbilityEvaluationConfiguration : IEntityTypeConfiguration<BuyAbilityEvaluation>
    {
        public void Configure(EntityTypeBuilder<BuyAbilityEvaluation> builder)
        {
            builder.ToTable("BuyAbilityEvaluations");
            builder.HasKey(ba => ba.Id);

            builder.Property(ba => ba.ClientId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(ba => ba.MonthlyGrossIncome)
                .HasColumnType("decimal(18,2)");

            builder.Property(ba => ba.MonthlyNetIncome)
                .HasColumnType("decimal(18,2)");

            builder.Property(ba => ba.MonthlyDebtPayments)
                .HasColumnType("decimal(18,2)");

            builder.Property(ba => ba.AvailableDownPayment)
                .HasColumnType("decimal(18,2)");

            builder.Property(ba => ba.MaxMortgageAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(ba => ba.MaxPropertyPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(ba => ba.MaxMonthlyPayment)
                .HasColumnType("decimal(18,2)");

            builder.Property(ba => ba.DebtToIncomeRatio)
                .HasColumnType("decimal(8,2)");

            builder.Property(ba => ba.EstimatedAnnualRate)
                .HasColumnType("decimal(8,2)");

            builder.Property(ba => ba.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(ba => ba.CreditScoreRating)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(ba => ba.EvaluationResult)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(ba => ba.Observations)
                .HasMaxLength(2000);

            builder.HasIndex(ba => ba.ClientId);
        }
    }
}
