using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FranchiseAgregator.Models;

namespace FranchiseAgregator.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public string OrderNumber { get; set; } = null!;
        public string Comments { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        // Ссылка на франшизу
        public int FranchiseId { get; set; }
        public virtual Franchise Franchise { get; set; } = null!;

        // Ссылка на пользователя-клиента
        public int UserId { get; set; }
        public virtual Users Client { get; set; } = null!;

        public int OrderStatusId { get; set; }
        public virtual OrderStatus OrderStatus { get; set; } = null!;

        public int RegionId { get; set; }
        public virtual Region Region { get; set; } = null!;

        public int ContactMethodId { get; set; }
        public virtual ContactMethod ContactMethod { get; set; } = null!;

        // Менеджер теперь определяется через Role в Users
        // Физическая колонка ManagerId удалена

        public int? FileId { get; set; }
        public virtual DbFile? File { get; set; }
    }
}