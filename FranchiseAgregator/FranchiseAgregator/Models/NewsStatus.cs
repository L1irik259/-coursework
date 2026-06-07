using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAggregator.Models
{
    public class NewsStatus
    {
        [Key]
        public int NewsStatusId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = null!; // Например: "Черновик", "Опубликовано", "Архив"

        [MaxLength(200)]
        public string? Description { get; set; }

        public virtual ICollection<News> News { get; set; } = new List<News>();
    }
}