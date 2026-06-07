using FranchiseAgregator.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAggregator.Models
{
    public class ModerSt
    {
        [Key]
        public int ModerStId { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
