using System;
using System.Collections.Generic;
using System.Linq;
using SmartBudgetTracker.Models;

namespace SmartBudgetMobile.Managers
{
    public class ExpenseManager
    {
        private List<Expense> _expenses;
        private int _nextId;

        public ExpenseManager()
        {
            _expenses = FileManager.LoadExpenses();
            _nextId = _expenses.Any() ? _expenses.Max(e => e.Id) + 1 : 1;
        }

        public List<Expense> GetExpensesByUser(string username)
        {
            return _expenses.Where(e => e.Username == username)
                           .OrderByDescending(e => e.Date)
                           .ToList();
        }

        public List<Expense> SearchExpenses(string username, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetExpensesByUser(username);

            searchTerm = searchTerm.ToLower();
            return _expenses.Where(e => e.Username == username &&
                (e.ExpenseName.ToLower().Contains(searchTerm) ||
                 e.Category.ToString().ToLower().Contains(searchTerm) ||
                 e.Notes?.ToLower().Contains(searchTerm) == true))
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        public List<Expense> FilterByDate(string username, DateTime start, DateTime end)
        {
            return _expenses.Where(e => e.Username == username && e.Date >= start && e.Date <= end)
                           .OrderByDescending(e => e.Date)
                           .ToList();
        }

        public List<Expense> FilterByCategory(string username, ExpenseCategory category)
        {
            return _expenses.Where(e => e.Username == username && e.Category == category)
                           .OrderByDescending(e => e.Date)
                           .ToList();
        }

        public List<Expense> GetTodayExpenses(string username)
        {
            var today = DateTime.Now.Date;
            return _expenses.Where(e => e.Username == username && e.Date.Date == today)
                           .OrderByDescending(e => e.Date)
                           .ToList();
        }

        public List<Expense> GetThisWeekExpenses(string username)
        {
            var weekStart = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
            return _expenses.Where(e => e.Username == username && e.Date >= weekStart)
                           .OrderByDescending(e => e.Date)
                           .ToList();
        }

        public List<Expense> GetThisMonthExpenses(string username)
        {
            var now = DateTime.Now;
            return _expenses.Where(e => e.Username == username &&
                e.Date.Year == now.Year && e.Date.Month == now.Month)
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        public List<Expense> GetThisYearExpenses(string username)
        {
            var now = DateTime.Now;
            return _expenses.Where(e => e.Username == username && e.Date.Year == now.Year)
                           .OrderByDescending(e => e.Date)
                           .ToList();
        }

        public (bool Success, string Message) AddExpense(Expense expense)
        {
            if (string.IsNullOrWhiteSpace(expense.ExpenseName))
                return (false, "Expense name cannot be empty.");

            if (expense.Amount <= 0)
                return (false, "Amount must be greater than zero.");

            var dateValid = ValidationManager.ValidateDate(expense.Date);
            if (!dateValid.IsValid)
                return (false, dateValid.ErrorMessage);

            expense.Id = _nextId++;
            expense.CreatedAt = DateTime.Now;
            _expenses.Add(expense);

            Save();
            return (true, "Expense added successfully.");
        }

        public (bool Success, string Message) UpdateExpense(Expense updatedExpense)
        {
            var existing = _expenses.FirstOrDefault(e => e.Id == updatedExpense.Id);
            if (existing == null)
                return (false, "Expense not found.");

            if (string.IsNullOrWhiteSpace(updatedExpense.ExpenseName))
                return (false, "Expense name cannot be empty.");

            if (updatedExpense.Amount <= 0)
                return (false, "Amount must be greater than zero.");

            existing.ExpenseName = updatedExpense.ExpenseName;
            existing.Category = updatedExpense.Category;
            existing.Amount = updatedExpense.Amount;
            existing.Quantity = updatedExpense.Quantity;
            existing.PricePerUnit = updatedExpense.PricePerUnit;
            existing.UnitOfMeasure = updatedExpense.UnitOfMeasure;
            existing.Date = updatedExpense.Date;
            existing.Notes = updatedExpense.Notes;

            Save();
            return (true, "Expense updated successfully.");
        }

        public (bool Success, string Message) DeleteExpense(int expenseId)
        {
            var expense = _expenses.FirstOrDefault(e => e.Id == expenseId);
            if (expense == null)
                return (false, "Expense not found.");

            _expenses.Remove(expense);
            Save();
            return (true, "Expense deleted successfully.");
        }

        public Expense GetExpenseById(int id)
        {
            return _expenses.FirstOrDefault(e => e.Id == id);
        }

        public decimal GetTotalExpenses(string username)
        {
            return _expenses.Where(e => e.Username == username).Sum(e => e.Amount);
        }

        public decimal GetDailyTotal(string username)
        {
            return GetTodayExpenses(username).Sum(e => e.Amount);
        }

        public decimal GetWeeklyTotal(string username)
        {
            return GetThisWeekExpenses(username).Sum(e => e.Amount);
        }

        public decimal GetMonthlyTotal(string username)
        {
            return GetThisMonthExpenses(username).Sum(e => e.Amount);
        }

        public decimal GetYearlyTotal(string username)
        {
            return GetThisYearExpenses(username).Sum(e => e.Amount);
        }

        public Dictionary<ExpenseCategory, decimal> GetCategoryBreakdown(string username)
        {
            return _expenses.Where(e => e.Username == username)
                          .GroupBy(e => e.Category)
                          .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }

        public KeyValuePair<ExpenseCategory, decimal> GetHighestCategory(string username)
        {
            var breakdown = GetCategoryBreakdown(username);
            return breakdown.Any() ? breakdown.OrderByDescending(x => x.Value).First()
                                   : new KeyValuePair<ExpenseCategory, decimal>(ExpenseCategory.Others, 0);
        }

        public Dictionary<string, decimal> GetMonthlyTrend(string username, int year)
        {
            var trend = new Dictionary<string, decimal>();
            for (int m = 1; m <= 12; m++)
            {
                decimal total = _expenses.Where(e => e.Username == username &&
                    e.Date.Year == year && e.Date.Month == m).Sum(e => e.Amount);
                trend.Add(new DateTime(year, m, 1).ToString("MMM"), total);
            }
            return trend;
        }

        public void Save()
        {
            FileManager.SaveExpenses(_expenses);
        }

        public void Reload()
        {
            _expenses = FileManager.LoadExpenses();
            _nextId = _expenses.Any() ? _expenses.Max(e => e.Id) + 1 : 1;
        }
    }
}
