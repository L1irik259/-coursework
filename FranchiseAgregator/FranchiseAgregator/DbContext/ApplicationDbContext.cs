using FranchiseAgregator.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace FranchiseAggregator.Models 
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserStatus> UserStatuses { get; set; } = null!;
        public DbSet<OrderStatus> OrderStatuses { get; set; } = null!;
        public DbSet<Region> Regions { get; set; } = null!;
        public DbSet<ContactMethod> ContactMethods { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<FranType> FranTypes { get; set; } = null!;
        public DbSet<PremTypes> PremTypeses { get; set; } = null!;
        public DbSet<FranStatus> FranStatuses { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<ModerSt> ModerSts { get; set; } = null!;

        public DbSet<Users> Users { get; set; } = null!;
        public DbSet<Franchiser> Franchisers { get; set; } = null!;
        public DbSet<Franchise> Franchises { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<NewsStatus> NewsStatuses { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public DbSet<FranchiseTag> FranchiseTags { get; set; } = null!;
        public DbSet<PriceHistory> PriceHistories { get; set; } = null!;
        public DbSet<DbFile> Files { get; set; } = null!;
        public DbSet<FranchiseAggregator.Models.Feedback> Feedbacks { get; set; }

        public DbSet<FeedbackStatus> FeedbackStatuses { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=WINDOWS-S4Q07EB\\SQLEXPRESS;Database=FranchiseDB;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

    
            modelBuilder.Entity<Franchise>(entity =>
            {
                entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
                entity.Property(e => e.FinalPrice).HasPrecision(18, 2);
                entity.Property(e => e.InvestmentAmount).HasPrecision(18, 2);
                entity.Property(e => e.PledgeAmount).HasPrecision(18, 2);
                entity.Property(e => e.RoyaltyPercent).HasPrecision(5, 2);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<PremTypes>(entity =>
            {
                entity.Property(e => e.MinArea).HasPrecision(6, 2);
            });

            modelBuilder.Entity<PriceHistory>(entity =>
            {
                entity.Property(e => e.NewPrice).HasPrecision(18, 2);
                entity.Property(e => e.OldPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Client) 
                .WithMany(u => u.OrdersAsClient) 
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

        }
    }
}