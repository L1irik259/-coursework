using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FranchiseAggregator.Models
{
    public class News
    {
        [Key]
        public int NewsId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Text { get; set; } = null!;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public DateTime PublishDate { get; set; } = DateTime.Now;

        // 🔹 Внешний ключ на справочник (замена IsActivate)
        public int NewsStatusId { get; set; }

        [ForeignKey(nameof(NewsStatusId))]
        public virtual NewsStatus? Status { get; set; } 
    }
}