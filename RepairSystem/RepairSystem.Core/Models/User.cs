namespace RepairSystem.Core.Models;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastLogin { get; set; }

    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    public int UserStatusId { get; set; }
    public virtual UserStatus UserStatus { get; set; } = null!;

    public virtual ICollection<Request> ClientRequests { get; set; } = new List<Request>();
    public virtual ICollection<Request> ExecutorRequests { get; set; } = new List<Request>();
    public virtual ICollection<Request> ManagerRequests { get; set; } = new List<Request>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<RequestConsultant> Consultations { get; set; } = new List<RequestConsultant>();
}
