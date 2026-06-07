using FranchiseAgregator.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }

        public DateTime AddedAt { get; set; }

        public int FranchiseId { get; set; }
        public int UserId { get; set; }

        public virtual Franchise Franchise { get; set; } = null!;
        public virtual Users User { get; set; } = null!;
    }
}