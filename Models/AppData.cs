using System.Collections.Generic;

namespace SmartBudgetTracker.Models
{
    public class AppData
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<Expense> Expenses { get; set; } = new List<Expense>();
        public List<Budget> Budgets { get; set; } = new List<Budget>();
        public List<Bill> Bills { get; set; } = new List<Bill>();
        public List<Savings> Savings { get; set; } = new List<Savings>();
        public List<NotificationLog> NotificationLogs { get; set; } = new List<NotificationLog>();
    }
}
