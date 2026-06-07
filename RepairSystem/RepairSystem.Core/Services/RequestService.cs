using Microsoft.EntityFrameworkCore;
using RepairSystem.Core.Data;
using RepairSystem.Core.DTOs;
using RepairSystem.Core.Interfaces;
using RepairSystem.Core.Models;

namespace RepairSystem.Core.Services;

public class RequestService : IRequestService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;
    private readonly IManagerAssignmentService _managerAssignment;

    public RequestService(AppDbContext db, INotificationService notifications, IManagerAssignmentService managerAssignment)
    {
        _db = db;
        _notifications = notifications;
        _managerAssignment = managerAssignment;
    }

    public async Task<Request> CreateAsync(CreateRequestDto dto)
    {
        var year = DateTime.Now.Year;
        var count = await _db.Requests.CountAsync(r => r.CreatedAt.Year == year);
        var managerId = await _managerAssignment.PickManagerAsync();

        var request = new Request
        {
            Number = $"REQ-{year}-{(count + 1):D4}",
            CreatedAt = DateTime.Now,
            Deadline = dto.Deadline,
            EquipmentId = dto.EquipmentId,
            FaultDescription = dto.FaultDescription,
            ProblemDescription = dto.ProblemDescription,
            ClientId = dto.ClientId,
            ManagerId = managerId,
            StatusId = 1
        };

        _db.Requests.Add(request);
        await _db.SaveChangesAsync();

        await _db.Entry(request).Reference(r => r.Status).LoadAsync();
        await _notifications.NotifyNewRequestAsync(request);

        return request;
    }

    public async Task AssignExecutorAsync(int requestId, int executorId)
    {
        var request = await _db.Requests.Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.RequestId == requestId)
            ?? throw new InvalidOperationException("Заявка не найдена");

        request.ExecutorId = executorId;
        request.StatusId = 2;
        await _db.SaveChangesAsync();

        await _db.Entry(request).Reference(r => r.Status).LoadAsync();
        await _notifications.NotifyExecutorAssignedAsync(request);
        await _notifications.NotifyStatusChangedAsync(request);
    }

    public async Task ChangeStatusAsync(int requestId, int statusId, int actorId)
    {
        var request = await _db.Requests.Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.RequestId == requestId)
            ?? throw new InvalidOperationException("Заявка не найдена");

        request.StatusId = statusId;
        if (statusId == 3)
            request.CompletedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        await _db.Entry(request).Reference(r => r.Status).LoadAsync();
        await _notifications.NotifyStatusChangedAsync(request);

        if (statusId == 3)
            await _notifications.NotifyCompletedAsync(request);
    }

    public async Task RequestHelpAsync(int requestId, int executorId)
    {
        var request = await _db.Requests.FindAsync(requestId)
            ?? throw new InvalidOperationException("Заявка не найдена");

        request.IsHelpRequested = true;
        request.HelpRequestedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        await _notifications.NotifyHelpRequestedAsync(request);
    }

    public async Task AttractSpecialistAsync(int requestId, int specialistId, int managerId)
    {
        _db.RequestConsultants.Add(new RequestConsultant
        {
            RequestId = requestId,
            SpecialistId = specialistId,
            AddedByManagerId = managerId,
            AddedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();

        var request = await _db.Requests.FindAsync(requestId);
        if (request != null)
            await _notifications.NotifyAsync(specialistId,
                $"Вас привлекли как консультанта к заявке №{request.Number}",
                requestId);
    }

    public async Task ExtendDeadlineAsync(int requestId, DateTime newDeadline, string agreementNote)
    {
        var request = await _db.Requests.FindAsync(requestId)
            ?? throw new InvalidOperationException("Заявка не найдена");

        request.DeadlineExtended = true;
        request.ExtendedDeadline = newDeadline;
        request.ExtensionAgreementNote = agreementNote;
        request.Deadline = newDeadline;
        await _db.SaveChangesAsync();

        await _notifications.NotifyDeadlineExtendedAsync(request);
    }
}
