using System;

namespace SmartBudgetMobile.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string ExpenseName { get; set; }
        public ExpenseCategory Category { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public string UnitOfMeasure { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
