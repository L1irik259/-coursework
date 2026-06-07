using FranchiseAgregator.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class PremTypes
    {
        [Key]
        public int PremTypesId { get; set; }

        public string Name { get; set; } = null!;
        public decimal? MinArea { get; set; }
        public string? Requirements { get; set; }

        public virtual ICollection<Franchise> Franchises { get; set; } = new List<Franchise>();
    }
}
