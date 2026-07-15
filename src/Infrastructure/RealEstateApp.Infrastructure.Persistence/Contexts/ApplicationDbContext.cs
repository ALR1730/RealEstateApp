using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; } = null!;
        public DbSet<PropertyImage> PropertyImages { get; set; } = null!;
        public DbSet<PropertyType> PropertyTypes { get; set; } = null!;
        public DbSet<SaleType> SaleTypes { get; set; } = null!;
        public DbSet<Improvement> Improvements { get; set; } = null!;
        public DbSet<Offer> Offers { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<MortgageSimulation> MortgageSimulations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fluent API Configurations
            modelBuilder.Entity<Property>(entity =>
            {
                entity.ToTable("Properties");
                entity.HasKey(p => p.Id);

                // Configuración de relación uno a muchos con PropertyType
                entity.HasOne(p => p.PropertyType)
                    .WithMany(pt => pt.Properties)
                    .HasForeignKey(p => p.PropertyTypeId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configuración de relación uno a muchos con SaleType
                entity.HasOne(p => p.SaleType)
                    .WithMany(st => st.Properties)
                    .HasForeignKey(p => p.SaleTypeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PropertyImage>(entity =>
            {
                entity.ToTable("PropertyImages");
                entity.HasKey(pi => pi.Id);

                entity.HasOne(pi => pi.Property)
                    .WithMany(p => p.Images)
                    .HasForeignKey(pi => pi.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PropertyType>(entity =>
            {
                entity.ToTable("PropertyTypes");
                entity.HasKey(pt => pt.Id);
            });

            modelBuilder.Entity<SaleType>(entity =>
            {
                entity.ToTable("SaleTypes");
                entity.HasKey(st => st.Id);
            });

            modelBuilder.Entity<Improvement>(entity =>
            {
                entity.ToTable("Improvements");
                entity.HasKey(i => i.Id);
            });

            modelBuilder.Entity<Offer>(entity =>
            {
                entity.ToTable("Offers");
                entity.HasKey(o => o.Id);

                entity.HasOne(o => o.Property)
                    .WithMany(p => p.Offers)
                    .HasForeignKey(o => o.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.ToTable("Chats");
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.Property)
                    .WithMany(p => p.Chats)
                    .HasForeignKey(c => c.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
