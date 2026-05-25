using System;

namespace SmartBudgetMobile.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public BudgetType BudgetType { get; set; }
        public decimal Amount { get; set; }
        public decimal Spent { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
