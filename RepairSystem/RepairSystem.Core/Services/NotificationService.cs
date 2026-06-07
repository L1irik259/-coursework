using RepairSystem.Core.Data;
using RepairSystem.Core.Interfaces;
using RepairSystem.Core.Models;

namespace RepairSystem.Core.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db) => _db = db;

    public async Task NotifyAsync(int userId, string message, int? requestId = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            RequestId = requestId,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();
    }

    public async Task NotifyStatusChangedAsync(Request request) =>
        await NotifyAsync(request.ClientId,
            $"Статус заявки №{request.Number} изменён на «{request.Status?.Name}»",
            request.RequestId);

    public async Task NotifyExecutorAssignedAsync(Request request)
    {
        if (request.ExecutorId.HasValue)
            await NotifyAsync(request.ExecutorId.Value,
                $"Вам назначена заявка №{request.Number}",
                request.RequestId);
    }

    public async Task NotifyHelpRequestedAsync(Request request)
    {
        if (request.ManagerId.HasValue)
            await NotifyAsync(request.ManagerId.Value,
                $"Исполнитель запросил помощь по заявке №{request.Number}",
                request.RequestId);
    }

    public async Task NotifyDeadlineExtendedAsync(Request request)
    {
        var deadline = request.ExtendedDeadline?.ToString("dd.MM.yyyy") ?? "—";
        await NotifyAsync(request.ClientId,
            $"Срок выполнения заявки №{request.Number} продлён до {deadline}",
            request.RequestId);
    }

    public async Task NotifyCompletedAsync(Request request) =>
        await NotifyAsync(request.ClientId,
            $"Заявка №{request.Number} выполнена. Оцените качество работы.",
            request.RequestId);

    public async Task NotifyNewRequestAsync(Request request)
    {
        if (request.ManagerId.HasValue)
            await NotifyAsync(request.ManagerId.Value,
                $"Новая заявка №{request.Number} ожидает обработки",
                request.RequestId);
    }
}
