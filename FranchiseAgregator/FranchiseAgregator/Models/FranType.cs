using FranchiseAgregator.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAggregator.Models
{
    public class FranType
    {
        [Key]
        public int FranTypeId { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<Franchise> Franchises { get; set; } = new List<Franchise>();
    }
}
