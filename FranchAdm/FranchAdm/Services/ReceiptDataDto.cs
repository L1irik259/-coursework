using System;

namespace FranchAdm.Services
{
    public class ReceiptDataDto
    {
        public int     OrderId           { get; set; }
        public string  OrderNumber       { get; set; }
        public DateTime CreatedAt        { get; set; }

        public string  ClientName        { get; set; }
        public string  ClientEmail       { get; set; }

        public string  FranchiseName     { get; set; }
        public string  FranchiserName    { get; set; }
        public string  CategoryName      { get; set; }

        public decimal FinalPrice        { get; set; }
        public decimal PledgeAmount      { get; set; }
        public decimal RoyaltyPercent    { get; set; }
        public decimal? DiscountPercent  { get; set; }

        public string  RegionName        { get; set; }
        public string  ContactMethodName { get; set; }
    }
}
