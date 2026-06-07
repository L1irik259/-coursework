using FranchiseAgregator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAggregator.Models
{
    public class Franchise
    {
        [Key]
        public int FranchiseId { get; set; }

        public string Article { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal PledgeAmount { get; set; }
        public decimal InvestmentAmount { get; set; }
        public int PaybackPeriod { get; set; }
        public decimal RoyaltyPercent { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
        public string Description { get; set; } = null!;
        public string? MinPhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public int CategoryId { get; set; }
        public int FranTypeId { get; set; }
        public int PremTypesId { get; set; }
        public int FranStatusId { get; set; }
        public int FranchiserId { get; set; }

        public virtual Category Category { get; set; } = null!;
        public virtual FranType FranType { get; set; } = null!;
        public virtual PremTypes PremTypes { get; set; } = null!;
        public virtual FranStatus FranStatus { get; set; } = null!;
        public virtual Franchiser Franchiser { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<FranchiseTag> FranchiseTags { get; set; } = new List<FranchiseTag>();
        public virtual ICollection<PriceHistory> PriceHistories { get; set; } = new List<PriceHistory>();
    }
}