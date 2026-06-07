using System.ComponentModel.DataAnnotations;

namespace FranchiseAggregator.Models
{
    public class FranchiseTag
    {
        [Key]
        public int FranchiseTagId { get; set; }

        public int FranchiseId { get; set; }
        public int TagId { get; set; }

        public virtual Franchise Franchise { get; set; } = null!;
        public virtual Tag Tag { get; set; } = null!;
    }
}