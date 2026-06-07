namespace RepairSystem.Core.Models;

public class RequestConsultant
{
    public int Id { get; set; }

    public int RequestId { get; set; }
    public virtual Request Request { get; set; } = null!;

    public int SpecialistId { get; set; }
    public virtual User Specialist { get; set; } = null!;

    public int AddedByManagerId { get; set; }
    public virtual User AddedByManager { get; set; } = null!;

    public DateTime AddedAt { get; set; }
}
