namespace RepairSystem.Core.Models;

public class UserStatus
{
    public int UserStatusId { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
