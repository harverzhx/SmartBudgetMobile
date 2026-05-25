using System;
using System.Collections.Generic;
using System.Linq;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Managers
{
    public class NotificationManager
    {
        private List<NotificationLog> _logs;
        private int _nextId;

        public NotificationManager()
        {
            _logs = FileManager.LoadLogs();
            _nextId = _logs.Any() ? _logs.Max(l => l.Id) + 1 : 1;
        }

        public List<NotificationLog> GetAllLogs(string username)
        {
            return _logs.Where(l => l.Username == username)
                       .OrderByDescending(l => l.CreatedAt)
                       .ToList();
        }

        public List<NotificationLog> GetUnreadLogs(string username)
        {
            return _logs.Where(l => l.Username == username && !l.IsRead)
                       .OrderByDescending(l => l.CreatedAt)
                       .ToList();
        }

        public int GetUnreadCount(string username)
        {
            return _logs.Count(l => l.Username == username && !l.IsRead);
        }

        public void AddNotification(string username, string message, NotificationType type)
        {
            var log = new NotificationLog
            {
                Id = _nextId++,
                Username = username,
                Message = message,
                Type = type,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _logs.Add(log);
            Save();
        }

        public void MarkAsRead(int notificationId)
        {
            var log = _logs.FirstOrDefault(l => l.Id == notificationId);
            if (log != null)
            {
                log.IsRead = true;
                Save();
            }
        }

        public void MarkAllAsRead(string username)
        {
            foreach (var log in _logs.Where(l => l.Username == username && !l.IsRead))
            {
                log.IsRead = true;
            }
            Save();
        }

        public void ClearAll(string username)
        {
            _logs.RemoveAll(l => l.Username == username);
            Save();
        }

        public void CheckBudgetWarnings(BudgetManager budgetManager, string username)
        {
            var budgets = budgetManager.GetAllBudgets(username);
            foreach (var budget in budgets)
            {
                if (budget.Amount <= 0) continue;

                decimal usagePercent = (budget.Spent / budget.Amount) * 100;

                if (usagePercent >= 100)
                {
                    AddNotification(username,
                        $"Warning: Your {budget.BudgetType} budget of {budget.Amount:C} has been fully used!",
                        NotificationType.Warning);
                }
                else if (usagePercent >= 80)
                {
                    AddNotification(username,
                        $"Alert: You have used {usagePercent:F0}% of your {budget.BudgetType} budget.",
                        NotificationType.Warning);
                }
            }
        }

        public void CheckBillDueDates(BillManager billManager, string username)
        {
            var bills = billManager.GetAllBills(username);
            foreach (var bill in bills)
            {
                if (bill.Status == BillStatus.Paid) continue;

                int daysUntilDue = (bill.DueDate - DateTime.Now).Days;

                if (daysUntilDue < 0)
                {
                    if (bill.Status != BillStatus.Overdue)
                    {
                        bill.Status = BillStatus.Overdue;
                        AddNotification(username,
                            $"Overdue: {bill.BillName} payment of {bill.Amount:C} was due {Math.Abs(daysUntilDue)} day(s) ago!",
                            NotificationType.Error);
                    }
                }
                else if (daysUntilDue == 0)
                {
                    AddNotification(username,
                        $"Reminder: {bill.BillName} payment of {bill.Amount:C} is due today!",
                        NotificationType.Warning);
                }
                else if (daysUntilDue <= 3)
                {
                    AddNotification(username,
                        $"Reminder: {bill.BillName} payment of {bill.Amount:C} is due in {daysUntilDue} day(s).",
                        NotificationType.Info);
                }
            }
            billManager.Save();
        }

        private void Save()
        {
            FileManager.SaveLogs(_logs);
        }
    }
}
