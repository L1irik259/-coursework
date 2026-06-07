using FranchiseAgregator.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAggregator.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<Franchise> Franchises { get; set; } = new List<Franchise>();
    }
}
