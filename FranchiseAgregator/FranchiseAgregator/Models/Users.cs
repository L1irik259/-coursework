using FranchiseAgregator.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastLogin { get; set; }

        public int RoleId { get; set; }
        public int UserStatusId { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual UserStatus UserStatus { get; set; } = null!;

        public virtual ICollection<Order> OrdersAsClient { get; set; } = new List<Order>();
        public virtual ICollection<Order> OrdersAsManager { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}