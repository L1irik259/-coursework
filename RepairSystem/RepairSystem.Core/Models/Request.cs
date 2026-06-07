namespace RepairSystem.Core.Models;

public class Request
{
    public int RequestId { get; set; }
    public string Number { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int EquipmentId { get; set; }
    public virtual Equipment Equipment { get; set; } = null!;

    public string FaultDescription { get; set; } = null!;
    public string ProblemDescription { get; set; } = null!;

    public int ClientId { get; set; }
    public virtual User Client { get; set; } = null!;

    public int? ExecutorId { get; set; }
    public virtual User? Executor { get; set; }

    public int? ManagerId { get; set; }
    public virtual User? Manager { get; set; }

    public int StatusId { get; set; }
    public virtual RequestStatus Status { get; set; } = null!;

    public bool IsHelpRequested { get; set; }
    public DateTime? HelpRequestedAt { get; set; }

    public bool DeadlineExtended { get; set; }
    public DateTime? ExtendedDeadline { get; set; }
    public string? ExtensionAgreementNote { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<RequestConsultant> Consultants { get; set; } = new List<RequestConsultant>();
}
