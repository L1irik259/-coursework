using FranchiseAgregator.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class OrderStatus
    {
        [Key]
        public int OrderStatusId { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}