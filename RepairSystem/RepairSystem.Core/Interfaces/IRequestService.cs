using RepairSystem.Core.DTOs;
using RepairSystem.Core.Models;

namespace RepairSystem.Core.Interfaces;

public interface IRequestService
{
    Task<Request> CreateAsync(CreateRequestDto dto);
    Task AssignExecutorAsync(int requestId, int executorId);
    Task ChangeStatusAsync(int requestId, int statusId, int actorId);
    Task RequestHelpAsync(int requestId, int executorId);
    Task AttractSpecialistAsync(int requestId, int specialistId, int managerId);
    Task ExtendDeadlineAsync(int requestId, DateTime newDeadline, string agreementNote);
}
