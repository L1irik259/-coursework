namespace RepairSystem.Core.Models;

public class Comment
{
    public int CommentId { get; set; }

    public int RequestId { get; set; }
    public virtual Request Request { get; set; } = null!;

    public int AuthorId { get; set; }
    public virtual User Author { get; set; } = null!;

    public string Text { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
