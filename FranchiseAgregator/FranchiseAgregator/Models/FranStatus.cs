using FranchiseAgregator.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class FranStatus
    {
        [Key]
        public int FranStatusId { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<Franchise> Franchises { get; set; } = new List<Franchise>();
    }
}
