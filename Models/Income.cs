using System;

namespace SmartBudgetMobile.Models
{
    public class Income
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
