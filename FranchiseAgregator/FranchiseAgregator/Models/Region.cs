using FranchiseAgregator.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class Region
    {
        [Key]
        public int RegionId { get; set; }

        public string Name { get; set; } = null!;
        public string? RegionCode { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}