using System;

namespace SmartBudgetMobile.Models
{
    public enum BudgetType
    {
        Daily,
        Weekly,
        Monthly,
        Yearly
    }

    public enum ExpenseCategory
    {
        Bills,
        BoardingHouse,
        Food,
        Transportation,
        School,
        Savings,
        Emergency,
        Others
    }

    public enum BillStatus
    {
        Paid,
        Unpaid,
        Overdue
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }
}
