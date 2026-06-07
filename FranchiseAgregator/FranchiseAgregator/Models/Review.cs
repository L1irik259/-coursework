using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public int Rating { get; set; }
        public string Text { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public int FranchiseId { get; set; }
        public int UserId { get; set; }
        public int ModerStId { get; set; }

        public virtual Franchise Franchise { get; set; } = null!;
        public virtual Users User { get; set; } = null!;
        public virtual ModerSt ModerSt { get; set; } = null!;
    }
}