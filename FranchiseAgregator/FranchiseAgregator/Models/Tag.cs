using FranchiseAgregator.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        public string Name { get; set; } = null!;
        public string? Color { get; set; }

        public virtual ICollection<FranchiseTag> FranchiseTags { get; set; } = new List<FranchiseTag>();
    }
}