using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RepairSystem.Core.Data;
using RepairSystem.Core.Interfaces;
using RepairSystem.Core.Services;

namespace RepairSystem.Admin;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                "Server=WINDOWS-S4Q07EB\\SQLEXPRESS;Database=RepairSystemDB;Trusted_Connection=True;TrustServerCertificate=True;"));

        services.AddScoped<IQrCodeService, QrCodeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IManagerAssignmentService, ManagerAssignmentService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IStatisticsService, StatisticsService>();

        Services = services.BuildServiceProvider();
    }
}
