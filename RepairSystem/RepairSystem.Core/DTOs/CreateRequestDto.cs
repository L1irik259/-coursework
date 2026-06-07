namespace RepairSystem.Core.DTOs;

public class CreateRequestDto
{
    public int EquipmentId { get; set; }
    public string FaultDescription { get; set; } = null!;
    public string ProblemDescription { get; set; } = null!;
    public int ClientId { get; set; }
    public DateTime? Deadline { get; set; }
}
