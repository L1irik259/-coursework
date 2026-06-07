using Microsoft.EntityFrameworkCore;
using RepairSystem.Core.Data;
using RepairSystem.Core.Interfaces;

namespace RepairSystem.Core.Services;

public class ManagerAssignmentService : IManagerAssignmentService
{
    private readonly AppDbContext _db;

    public ManagerAssignmentService(AppDbContext db) => _db = db;

    public async Task<int?> PickManagerAsync()
    {
        var manager = await _db.Users
            .Where(u => u.Role.Name == "Менеджер" && u.UserStatus.Name == "Активен")
            .OrderBy(u => u.ManagerRequests.Count(r => r.Status.Name != "Выполнено"))
            .FirstOrDefaultAsync();

        return manager?.UserId;
    }
}
