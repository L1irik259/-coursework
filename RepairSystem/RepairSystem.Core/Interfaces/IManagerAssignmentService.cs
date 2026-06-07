namespace RepairSystem.Core.Interfaces;

public interface IManagerAssignmentService
{
    Task<int?> PickManagerAsync();
}
