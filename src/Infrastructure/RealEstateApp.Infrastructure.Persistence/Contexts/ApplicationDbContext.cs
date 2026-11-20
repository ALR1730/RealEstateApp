using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Domain.Common;
using RealEstateApp.Core.Domain.Entities;

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
        public DbSet<AgentReview> AgentReviews { get; set; } = null!;
        public DbSet<Commission> Commissions { get; set; } = null!;
        public DbSet<PropertyDocument> PropertyDocuments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Renombrar tablas de Identity para mayor claridad
            modelBuilder.Entity<IdentityUser>(entity => entity.ToTable("Users"));
            modelBuilder.Entity<IdentityRole>(entity => entity.ToTable("Roles"));
            modelBuilder.Entity<IdentityUserRole<string>>(entity => entity.ToTable("UserRoles"));
            modelBuilder.Entity<IdentityUserLogin<string>>(entity => entity.ToTable("UserLogins"));

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ApplyAuditAndConcurrencyTokens();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            ApplyAuditAndConcurrencyTokens();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ApplyAuditAndConcurrencyTokens()
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

            foreach (var entry in ChangeTracker.Entries<IHasConcurrencyToken>())
            {
                if (entry.State == EntityState.Modified)
                {
                    var rowVersionProperty = entry.Property(nameof(IHasConcurrencyToken.RowVersion));
                    var originalRowVersion = rowVersionProperty.OriginalValue;

                    entry.Entity.IncrementRowVersion();

                    rowVersionProperty.OriginalValue = originalRowVersion;
                    rowVersionProperty.CurrentValue = entry.Entity.RowVersion;
                    rowVersionProperty.IsModified = true;
                }
            }
        }
    }
}
