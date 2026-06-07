using FranchiseAgregator.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAggregator.Models
{
    public class Franchiser
    {
        [Key]
        public int FranchiserId { get; set; }

        public string Name { get; set; } = null!;
        public string INN { get; set; } = null!;
        public string? Website { get; set; }
        public string? Contacts { get; set; }

        public virtual ICollection<Franchise> Franchises { get; set; } = new List<Franchise>();
    }
}
