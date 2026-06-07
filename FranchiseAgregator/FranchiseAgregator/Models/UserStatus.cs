using FranchiseAgregator.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class UserStatus
    {
        [Key]
        public int UserStatusId { get; set; }

        public string Name { get; set; } = null!;

        public virtual ICollection<Users> Users { get; set; } = new List<Users>();
    }
}