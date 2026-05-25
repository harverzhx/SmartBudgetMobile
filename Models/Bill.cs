using System;

namespace SmartBudgetMobile.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public string BillName { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public BillStatus Status { get; set; }
        public string Username { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
