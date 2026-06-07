using Microsoft.EntityFrameworkCore;
using RepairSystem.Core.Models;

namespace RepairSystem.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext() { }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserStatus> UserStatuses { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Equipment> Equipment { get; set; } = null!;
    public DbSet<RequestStatus> RequestStatuses { get; set; } = null!;
    public DbSet<Request> Requests { get; set; } = null!;
    public DbSet<RequestConsultant> RequestConsultants { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(
                "Server=WINDOWS-S4Q07EB\\SQLEXPRESS;Database=RepairSystemDB;Trusted_Connection=True;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SystemSetting>().HasKey(s => s.Key);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Client)
            .WithMany(u => u.ClientRequests)
            .HasForeignKey(r => r.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Executor)
            .WithMany(u => u.ExecutorRequests)
            .HasForeignKey(r => r.ExecutorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Request>()
            .HasOne(r => r.Manager)
            .WithMany(u => u.ManagerRequests)
            .HasForeignKey(r => r.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestConsultant>()
            .HasOne(rc => rc.Specialist)
            .WithMany(u => u.Consultations)
            .HasForeignKey(rc => rc.SpecialistId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequestConsultant>()
            .HasOne(rc => rc.AddedByManager)
            .WithMany()
            .HasForeignKey(rc => rc.AddedByManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, Name = "Клиент" },
            new Role { RoleId = 2, Name = "Исполнитель" },
            new Role { RoleId = 3, Name = "Менеджер" });

        modelBuilder.Entity<UserStatus>().HasData(
            new UserStatus { UserStatusId = 1, Name = "Активен" },
            new UserStatus { UserStatusId = 2, Name = "Заблокирован" });

        modelBuilder.Entity<RequestStatus>().HasData(
            new RequestStatus { RequestStatusId = 1, Name = "В ожидании" },
            new RequestStatus { RequestStatusId = 2, Name = "В работе" },
            new RequestStatus { RequestStatusId = 3, Name = "Выполнено" });

        modelBuilder.Entity<SystemSetting>().HasData(
            new SystemSetting { Key = "QrCodeFormUrl", Value = "https://forms.google.com/your-form-link-here" });
    }
}
