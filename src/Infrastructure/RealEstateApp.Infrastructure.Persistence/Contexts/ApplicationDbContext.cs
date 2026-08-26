using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Constants;
using RealEstateApp.Core.Domain.Entities;
using System.Security.Claims;

namespace RealEstateApp.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor? httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<PropertyImage> PropertyImages { get; set; } = null!;
        public DbSet<PropertyType> PropertyTypes { get; set; } = null!;
        public DbSet<SaleType> SaleTypes { get; set; } = null!;
        public DbSet<Improvement> Improvements { get; set; } = null!;
        public DbSet<PropertyImprovement> PropertyImprovements { get; set; } = null!;
        public DbSet<Offer> Offers { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<MortgageSimulation> MortgageSimulations { get; set; } = null!;
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public DbSet<UserActivity> UserActivities { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<PropertyAppointment> PropertyAppointments { get; set; } = null!;
        public DbSet<Province> Provinces { get; set; } = null!;
        public DbSet<Municipality> Municipalities { get; set; } = null!;
        public DbSet<AgentVerification> AgentVerifications { get; set; } = null!;
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
        public DbSet<AgentSubscription> AgentSubscriptions { get; set; } = null!;
        public DbSet<SavedSearch> SavedSearches { get; set; } = null!;
        public DbSet<PropertyPriceHistory> PropertyPriceHistories { get; set; } = null!;
        public DbSet<PropertyValuation> PropertyValuations { get; set; } = null!;
        public DbSet<LeadPipeline> LeadPipelines { get; set; } = null!;
        public DbSet<BuyAbilityEvaluation> BuyAbilityEvaluations { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Renombrar tablas de Identity para mayor claridad
            modelBuilder.Entity<IdentityUser>(entity =>
            {
                entity.ToTable("Users");
            });
            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("Roles");
            });
            modelBuilder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });
            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            #region Property

            modelBuilder.Entity<Property>(entity =>
            {
                entity.ToTable("Properties");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(p => p.Code)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.HasIndex(p => p.Code)
                    .IsUnique();

                entity.Property(p => p.Price)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue(CurrencyConstants.DOP);

                entity.Property(p => p.PriceInDOP)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.SizeInMeters)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.MontoSeparacion)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.Description)
                    .HasMaxLength(2000);

                entity.Property(p => p.MatterportModelId)
                    .HasMaxLength(50);

                entity.Property(p => p.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue(PropertyStatus.Available);

                entity.Property(p => p.AgentId)
                    .IsRequired()
                    .HasMaxLength(450); // Coincide con la longitud del Id de IdentityUser

                entity.Property(p => p.Sector)
                    .HasMaxLength(100);

                entity.Property(p => p.FullAddress)
                    .HasMaxLength(300);

                entity.Property(p => p.IsFeatured)
                    .HasDefaultValue(false);

                // Relación uno a muchos con PropertyType
                entity.HasOne(p => p.PropertyType)
                    .WithMany(pt => pt.Properties)
                    .HasForeignKey(p => p.PropertyTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación uno a muchos con SaleType
                entity.HasOne(p => p.SaleType)
                    .WithMany(st => st.Properties)
                    .HasForeignKey(p => p.SaleTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación con Provincia
                entity.HasOne(p => p.Province)
                    .WithMany(prov => prov.Properties)
                    .HasForeignKey(p => p.ProvinceId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Relación con Municipio
                entity.HasOne(p => p.Municipality)
                    .WithMany(mun => mun.Properties)
                    .HasForeignKey(p => p.MunicipalityId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            #endregion

            #region Province

            modelBuilder.Entity<Province>(entity =>
            {
                entity.ToTable("Provinces");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.IsoCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.HasIndex(p => p.IsoCode)
                    .IsUnique();
            });

            #endregion

            #region Municipality

            modelBuilder.Entity<Municipality>(entity =>
            {
                entity.ToTable("Municipalities");
                entity.HasKey(m => m.Id);

                entity.Property(m => m.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(m => m.Province)
                    .WithMany(p => p.Municipalities)
                    .HasForeignKey(m => m.ProvinceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region PropertyImage

            modelBuilder.Entity<PropertyImage>(entity =>
            {
                entity.ToTable("PropertyImages");
                entity.HasKey(pi => pi.Id);

                entity.Property(pi => pi.ImageUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(pi => pi.Property)
                    .WithMany(p => p.Images)
                    .HasForeignKey(pi => pi.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region PropertyType

            modelBuilder.Entity<PropertyType>(entity =>
            {
                entity.ToTable("PropertyTypes");
                entity.HasKey(pt => pt.Id);

                entity.Property(pt => pt.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(pt => pt.Description)
                    .HasMaxLength(500);
            });

            #endregion

            #region SaleType

            modelBuilder.Entity<SaleType>(entity =>
            {
                entity.ToTable("SaleTypes");
                entity.HasKey(st => st.Id);

                entity.Property(st => st.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(st => st.Description)
                    .HasMaxLength(500);
            });

            #endregion

            #region Improvement

            modelBuilder.Entity<Improvement>(entity =>
            {
                entity.ToTable("Improvements");
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(i => i.Description)
                    .HasMaxLength(500);
            });

            #endregion

            #region PropertyImprovement (Many-to-Many Join Table)

            modelBuilder.Entity<PropertyImprovement>(entity =>
            {
                entity.ToTable("PropertyImprovements");

                // Clave compuesta
                entity.HasKey(pi => new { pi.PropertyId, pi.ImprovementId });

                entity.HasOne(pi => pi.Property)
                    .WithMany(p => p.PropertyImprovements)
                    .HasForeignKey(pi => pi.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pi => pi.Improvement)
                    .WithMany(i => i.PropertyImprovements)
                    .HasForeignKey(pi => pi.ImprovementId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region Offer

            modelBuilder.Entity<Offer>(entity =>
            {
                entity.ToTable("Offers");
                entity.HasKey(o => o.Id);

                entity.Property(o => o.MontoOfertado)
                    .HasColumnType("decimal(18,2)");

                entity.Property(o => o.CounterOfferAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(o => o.CounterOfferMessage)
                    .HasMaxLength(1000);

                entity.Property(o => o.ClienteId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(o => o.Property)
                    .WithMany(p => p.Offers)
                    .HasForeignKey(o => o.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region Chat

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("Chats");
                entity.HasKey(c => c.Id);

                entity.Property(c => c.ClienteId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(c => c.AgenteId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(c => c.SenderId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(c => c.MessageContent)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(c => c.PropertyId)
                    .IsRequired(false);

                entity.HasOne(c => c.Property)
                    .WithMany(p => p.Chats)
                    .HasForeignKey(c => c.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region Favorite

            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.ToTable("Favorites");
                entity.HasKey(f => f.Id);

                entity.Property(f => f.ClienteId)
                    .IsRequired()
                    .HasMaxLength(450);

                // Índice único para evitar duplicados (un cliente no puede marcar la misma propiedad dos veces)
                entity.HasIndex(f => new { f.ClienteId, f.PropertyId })
                    .IsUnique();

                entity.HasOne(f => f.Property)
                    .WithMany(p => p.Favorites)
                    .HasForeignKey(f => f.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region MortgageSimulation

            modelBuilder.Entity<MortgageSimulation>(entity =>
            {
                entity.ToTable("MortgageSimulations");
                entity.HasKey(ms => ms.Id);

                entity.Property(ms => ms.MontoInicialAportado)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ms => ms.TasaInteresAnual)
                    .HasColumnType("decimal(5,2)");

                entity.Property(ms => ms.ClienteId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(ms => ms.Property)
                    .WithMany(p => p.MortgageSimulations)
                    .HasForeignKey(ms => ms.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region PropertyAppointment

            modelBuilder.Entity<PropertyAppointment>(entity =>
            {
                entity.ToTable("PropertyAppointments");
                entity.HasKey(pa => pa.Id);

                entity.Property(pa => pa.ClienteId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(pa => pa.AgentId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(pa => pa.Notes)
                    .HasMaxLength(1000);

                entity.Property(pa => pa.AgentNotes)
                    .HasMaxLength(1000);

                entity.HasOne(pa => pa.Property)
                    .WithMany(p => p.Appointments)
                    .HasForeignKey(pa => pa.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region AgentVerification

            modelBuilder.Entity<AgentVerification>(entity =>
            {
                entity.ToTable("AgentVerifications");
                entity.HasKey(v => v.Id);

                entity.Property(v => v.AgentId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasIndex(v => v.AgentId)
                    .IsUnique();

                entity.Property(v => v.Cedula)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(v => v.CedulaFrontImageUrl)
                    .HasMaxLength(500);

                entity.Property(v => v.CedulaBackImageUrl)
                    .HasMaxLength(500);

                entity.Property(v => v.Status)
                    .IsRequired()
                    .HasMaxLength(30)
                    .HasDefaultValue(VerificationStatus.Pending);

                entity.Property(v => v.RejectionReason)
                    .HasMaxLength(1000);

                entity.Property(v => v.ReviewedByAdminId)
                    .HasMaxLength(450);
            });

            #endregion

            #region SubscriptionPlan

            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.ToTable("SubscriptionPlans");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.Description)
                    .HasMaxLength(500);

                entity.Property(p => p.MonthlyPrice)
                    .HasColumnType("decimal(18,2)");
            });

            #endregion

            #region AgentSubscription

            modelBuilder.Entity<AgentSubscription>(entity =>
            {
                entity.ToTable("AgentSubscriptions");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.AgentId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasIndex(s => s.AgentId);

                entity.HasOne(s => s.SubscriptionPlan)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(s => s.SubscriptionPlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            #endregion

            #region SavedSearch

            modelBuilder.Entity<SavedSearch>(entity =>
            {
                entity.ToTable("SavedSearches");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(s => s.Sector)
                    .HasMaxLength(100);

                entity.Property(s => s.MinPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(s => s.MaxPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(s => s.MinSizeInMeters)
                    .HasColumnType("decimal(18,2)");

                entity.Property(s => s.MaxSizeInMeters)
                    .HasColumnType("decimal(18,2)");

                entity.HasIndex(s => s.UserId);
            });

            #endregion

            #region PropertyPriceHistory

            modelBuilder.Entity<PropertyPriceHistory>(entity =>
            {
                entity.ToTable("PropertyPriceHistories");
                entity.HasKey(ph => ph.Id);

                entity.Property(ph => ph.OldPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ph => ph.NewPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ph => ph.PercentageChange)
                    .HasColumnType("decimal(8,2)");

                entity.Property(ph => ph.Currency)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(ph => ph.ChangeReason)
                    .HasMaxLength(250);

                entity.Property(ph => ph.ChangedByUserId)
                    .HasMaxLength(450);

                entity.HasOne(ph => ph.Property)
                    .WithMany(p => p.PriceHistories)
                    .HasForeignKey(ph => ph.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ph => ph.PropertyId);
                entity.HasIndex(ph => ph.ChangeDate);
            });

            #endregion

            #region PropertyValuation

            modelBuilder.Entity<PropertyValuation>(entity =>
            {
                entity.ToTable("PropertyValuations");
                entity.HasKey(pv => pv.Id);

                entity.Property(pv => pv.EstimatedPricePerSqm)
                    .HasColumnType("decimal(18,2)");

                entity.Property(pv => pv.EstimatedTotalPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(pv => pv.MinPricePerSqm)
                    .HasColumnType("decimal(18,2)");

                entity.Property(pv => pv.MaxPricePerSqm)
                    .HasColumnType("decimal(18,2)");

                entity.Property(pv => pv.StandardDeviation)
                    .HasColumnType("decimal(18,2)");

                entity.Property(pv => pv.SearchRadiusKm)
                    .HasColumnType("decimal(8,2)");

                entity.Property(pv => pv.Currency)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(pv => pv.Notes)
                    .HasMaxLength(1000);

                entity.HasOne(pv => pv.Property)
                    .WithMany()
                    .HasForeignKey(pv => pv.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(pv => pv.PropertyId);
            });

            #endregion

            #region LeadPipeline

            modelBuilder.Entity<LeadPipeline>(entity =>
            {
                entity.ToTable("LeadPipelines");
                entity.HasKey(lp => lp.Id);

                entity.Property(lp => lp.LeadName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(lp => lp.LeadEmail)
                    .HasMaxLength(200);

                entity.Property(lp => lp.LeadPhone)
                    .HasMaxLength(30);

                entity.Property(lp => lp.Notes)
                    .HasMaxLength(2000);

                entity.Property(lp => lp.AgentId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(lp => lp.Stage)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue(PipelineStage.NewLead);

                entity.Property(lp => lp.Priority)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Normal");

                entity.Property(lp => lp.Source)
                    .HasMaxLength(100);

                entity.Property(lp => lp.EstimatedBudget)
                    .HasColumnType("decimal(18,2)");

                entity.Property(lp => lp.BudgetCurrency)
                    .HasMaxLength(3);

                entity.HasOne(lp => lp.Property)
                    .WithMany()
                    .HasForeignKey(lp => lp.PropertyId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(lp => lp.AgentId);
                entity.HasIndex(lp => lp.Stage);
            });

            #endregion

            #region BuyAbilityEvaluation

            modelBuilder.Entity<BuyAbilityEvaluation>(entity =>
            {
                entity.ToTable("BuyAbilityEvaluations");
                entity.HasKey(ba => ba.Id);

                entity.Property(ba => ba.ClientId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.Property(ba => ba.MonthlyGrossIncome)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ba => ba.MonthlyNetIncome)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ba => ba.MonthlyDebtPayments)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ba => ba.AvailableDownPayment)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ba => ba.MaxMortgageAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ba => ba.MaxPropertyPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ba => ba.MaxMonthlyPayment)
                    .HasColumnType("decimal(18,2)");

                entity.Property(ba => ba.DebtToIncomeRatio)
                    .HasColumnType("decimal(8,2)");

                entity.Property(ba => ba.EstimatedAnnualRate)
                    .HasColumnType("decimal(8,2)");

                entity.Property(ba => ba.Currency)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(ba => ba.CreditScoreRating)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(ba => ba.EvaluationResult)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(ba => ba.Observations)
                    .HasMaxLength(2000);

                entity.HasIndex(ba => ba.ClientId);
            });

            #endregion
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var currentUser = _httpContextAccessor?.HttpContext?.User?.Identity?.Name
                              ?? _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? "System";

            foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Created = DateTime.UtcNow;
                        entry.Entity.CreatedBy = currentUser;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModified = DateTime.UtcNow;
                        entry.Entity.LastModifiedBy = currentUser;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
