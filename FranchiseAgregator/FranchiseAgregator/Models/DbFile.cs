using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FranchiseAgregator.Models
{
    public class DbFile
    {
        [Key]
        public int FileId { get; set; }

        public string? FileUri { get; set; }

        public byte[]? FileContent { get; set; }

        public string? FileName { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}