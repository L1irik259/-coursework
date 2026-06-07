namespace RepairSystem.Core.Models;

public class Equipment
{
    public int EquipmentId { get; set; }
    public string Name { get; set; } = null!;
    public string SerialNumber { get; set; } = null!;
    public string? Description { get; set; }
    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
