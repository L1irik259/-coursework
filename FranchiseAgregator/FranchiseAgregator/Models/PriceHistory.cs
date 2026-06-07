using System;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class PriceHistory
    {
        [Key]
        public int PriceHistoryId { get; set; }

        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime ChangeAd { get; set; }
        public string Reason { get; set; } = null!;

        public int FranchiseId { get; set; }

        public virtual Franchise Franchise { get; set; } = null!;
    }
}
