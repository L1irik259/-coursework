using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FranchiseAgregator.Models;

namespace FranchiseAgregator.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = null!;

        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        public string Message { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔹 ИСПРАВЛЕНО: string? → int? (чтобы соответствовало Users.UserId)
        public int? UserId { get; set; }

        public int FeedbackStatusId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual Users? User { get; set; }

        [ForeignKey(nameof(FeedbackStatusId))]
        public virtual FeedbackStatus? Status { get; set; }
    }
}