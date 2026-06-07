using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class FeedbackStatus
    {
        [Key]
        public int FeedbackStatusId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = null!;

        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}