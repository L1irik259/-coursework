namespace RepairSystem.Core.Models;

public class Notification
{
    public int NotificationId { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public int? RequestId { get; set; }

    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
