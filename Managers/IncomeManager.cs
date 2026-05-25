using System;
using System.Collections.Generic;
using System.Linq;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Managers
{
    public class IncomeManager
    {
        private List<Income> _incomes;
        private int _nextId;

        public IncomeManager()
        {
            _incomes = FileManager.LoadIncomes();
            _nextId = _incomes.Any() ? _incomes.Max(i => i.Id) + 1 : 1;
        }

        public List<Income> GetIncomesByUser(string username)
        {
            return _incomes.Where(i => i.Username == username)
                           .OrderByDescending(i => i.Date)
                           .ToList();
        }

        public decimal GetTotalIncome(string username)
        {
            return _incomes.Where(i => i.Username == username).Sum(i => i.Amount);
        }

        public decimal GetBalance(string username, decimal totalExpenses)
        {
            return GetTotalIncome(username) - totalExpenses;
        }

        public (bool Success, string Message) AddIncome(Income income)
        {
            if (income.Amount <= 0)
                return (false, "Amount must be greater than zero.");

            income.Id = _nextId++;
            income.CreatedAt = DateTime.Now;
            _incomes.Add(income);

            Save();
            return (true, "Income added successfully.");
        }

        public (bool Success, string Message) DeleteIncome(int incomeId)
        {
            var income = _incomes.FirstOrDefault(i => i.Id == incomeId);
            if (income == null)
                return (false, "Income not found.");

            _incomes.Remove(income);
            Save();
            return (true, "Income deleted successfully.");
        }

        public void Save()
        {
            FileManager.SaveIncomes(_incomes);
        }

        public void Reload()
        {
            _incomes = FileManager.LoadIncomes();
            _nextId = _incomes.Any() ? _incomes.Max(i => i.Id) + 1 : 1;
        }
    }
}
