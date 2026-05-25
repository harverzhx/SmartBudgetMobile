using System;

namespace SmartBudgetMobile.Models
{
    public class ExpenseTemplate
    {
        public int Id { get; set; }
        public string TemplateName { get; set; }
        public ExpenseCategory Category { get; set; }
        public decimal PricePerUnit { get; set; }
        public string DefaultUnit { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
