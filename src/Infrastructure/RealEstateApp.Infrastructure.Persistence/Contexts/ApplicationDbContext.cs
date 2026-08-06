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

                entity.Property(p => p.SizeInMeters)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.MontoSeparacion)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.Description)
                    .HasMaxLength(2000);

                entity.Property(p => p.Status)
                    .HasMaxLength(20)
                    .HasDefaultValue(PropertyStatus.Available);

                entity.Property(p => p.AgentId)
                    .IsRequired()
                    .HasMaxLength(450); // Coincide con la longitud del Id de IdentityUser

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
