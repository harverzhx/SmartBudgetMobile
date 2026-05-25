using System;
using System.Collections.Generic;
using System.Linq;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Managers
{
    public class BudgetManager
    {
        private List<Budget> _budgets;
        private int _nextId;

        public BudgetManager()
        {
            _budgets = FileManager.LoadBudgets();
            _nextId = _budgets.Any() ? _budgets.Max(b => b.Id) + 1 : 1;
        }

        public List<Budget> GetAllBudgets(string username)
        {
            return _budgets.Where(b => b.Username == username && b.IsActive).ToList();
        }

        public Budget GetBudgetByType(string username, BudgetType type)
        {
            return _budgets.FirstOrDefault(b => b.Username == username && b.BudgetType == type && b.IsActive);
        }

        public (bool Success, string Message) SetBudget(string username, BudgetType budgetType, decimal amount)
        {
            if (amount <= 0)
                return (false, "Budget amount must be greater than zero.");

            var existing = GetBudgetByType(username, budgetType);
            if (existing != null)
            {
                existing.Amount = amount;
                existing.UpdatedAt = DateTime.Now;
                existing.IsActive = true;
            }
            else
            {
                var budget = new Budget
                {
                    Id = _nextId++,
                    Username = username,
                    BudgetType = budgetType,
                    Amount = amount,
                    Spent = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsActive = true
                };
                _budgets.Add(budget);
            }

            Save();
            return (true, $"{budgetType} budget of {amount:C} has been set.");
        }

        public (bool Success, string Message) UpdateBudget(string username, BudgetType budgetType, decimal amount)
        {
            return SetBudget(username, budgetType, amount);
        }

        public (bool Success, string Message) ResetBudget(string username, BudgetType budgetType)
        {
            var budget = GetBudgetByType(username, budgetType);
            if (budget == null)
                return (false, $"No {budgetType} budget found.");

            budget.Amount = 0;
            budget.Spent = 0;
            budget.UpdatedAt = DateTime.Now;
            budget.IsActive = false;
            Save();
            return (true, $"{budgetType} budget has been reset.");
        }

        public void UpdateSpent(string username, BudgetType budgetType, decimal amount, bool isAdding)
        {
            var budget = GetBudgetByType(username, budgetType);
            if (budget == null) return;

            if (isAdding)
                budget.Spent += amount;
            else
                budget.Spent = Math.Max(0, budget.Spent - amount);

            budget.UpdatedAt = DateTime.Now;
            Save();
        }

        public void RecalculateSpent(string username, ExpenseManager expenseManager)
        {
            var expenses = expenseManager.GetExpensesByUser(username);
            var budgets = GetAllBudgets(username);

            foreach (var budget in budgets)
            {
                decimal total = 0;
                var now = DateTime.Now;

                switch (budget.BudgetType)
                {
                    case BudgetType.Daily:
                        total = expenses.Where(e => e.Date.Date == now.Date).Sum(e => e.Amount);
                        break;
                    case BudgetType.Weekly:
                        var weekStart = now.AddDays(-(int)now.DayOfWeek);
                        total = expenses.Where(e => e.Date >= weekStart && e.Date <= now).Sum(e => e.Amount);
                        break;
                    case BudgetType.Monthly:
                        total = expenses.Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month).Sum(e => e.Amount);
                        break;
                    case BudgetType.Yearly:
                        total = expenses.Where(e => e.Date.Year == now.Year).Sum(e => e.Amount);
                        break;
                }

                budget.Spent = total;
                budget.UpdatedAt = DateTime.Now;
            }

            Save();
        }

        public decimal GetTotalBudget(string username)
        {
            return GetAllBudgets(username).Sum(b => b.Amount);
        }

        public decimal GetTotalSpent(string username)
        {
            return GetAllBudgets(username).Sum(b => b.Spent);
        }

        public decimal GetRemainingBalance(string username)
        {
            return GetTotalBudget(username) - GetTotalSpent(username);
        }

        public bool IsBudgetExceeded(string username)
        {
            return GetAllBudgets(username).Any(b => b.Amount > 0 && b.Spent > b.Amount);
        }

        public void Save()
        {
            FileManager.SaveBudgets(_budgets);
        }

        public void Reload()
        {
            _budgets = FileManager.LoadBudgets();
            _nextId = _budgets.Any() ? _budgets.Max(b => b.Id) + 1 : 1;
        }
    }
}
