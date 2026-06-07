using RepairSystem.Core.Models;

namespace RepairSystem.Core.Interfaces;

public interface INotificationService
{
    Task NotifyAsync(int userId, string message, int? requestId = null);
    Task NotifyStatusChangedAsync(Request request);
    Task NotifyExecutorAssignedAsync(Request request);
    Task NotifyHelpRequestedAsync(Request request);
    Task NotifyDeadlineExtendedAsync(Request request);
    Task NotifyCompletedAsync(Request request);
    Task NotifyNewRequestAsync(Request request);
}
