using System;

namespace SmartBudgetTracker.Models
{
    public class Savings
    {
        public int Id { get; set; }
        public string GoalName { get; set; }
        public decimal GoalAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? TargetDate { get; set; }
        public bool IsCompleted { get; set; }

        public decimal RemainingAmount => GoalAmount - CurrentAmount;
        public decimal ProgressPercent => GoalAmount > 0 ? Math.Round((CurrentAmount / GoalAmount) * 100, 2) : 0;
    }
}
