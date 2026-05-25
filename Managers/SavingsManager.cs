using System;
using System.Collections.Generic;
using System.Linq;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Managers
{
    public class SavingsManager
    {
        private List<Savings> _savings;
        private int _nextId;

        public SavingsManager()
        {
            _savings = FileManager.LoadSavings();
            _nextId = _savings.Any() ? _savings.Max(s => s.Id) + 1 : 1;
        }

        public List<Savings> GetAllSavings(string username)
        {
            return _savings.Where(s => s.Username == username).ToList();
        }

        public List<Savings> GetActiveGoals(string username)
        {
            return _savings.Where(s => s.Username == username && !s.IsCompleted).ToList();
        }

        public List<Savings> GetCompletedGoals(string username)
        {
            return _savings.Where(s => s.Username == username && s.IsCompleted).ToList();
        }

        public (bool Success, string Message) AddGoal(Savings savings)
        {
            if (string.IsNullOrWhiteSpace(savings.GoalName))
                return (false, "Goal name cannot be empty.");

            if (savings.GoalAmount <= 0)
                return (false, "Goal amount must be greater than zero.");

            if (savings.CurrentAmount < 0)
                return (false, "Current amount cannot be negative.");

            savings.Id = _nextId++;
            savings.CreatedAt = DateTime.Now;
            savings.IsCompleted = false;
            _savings.Add(savings);

            Save();
            return (true, "Savings goal added successfully.");
        }

        public (bool Success, string Message) UpdateGoal(Savings updated)
        {
            var existing = _savings.FirstOrDefault(s => s.Id == updated.Id);
            if (existing == null)
                return (false, "Goal not found.");

            if (string.IsNullOrWhiteSpace(updated.GoalName))
                return (false, "Goal name cannot be empty.");

            if (updated.GoalAmount <= 0)
                return (false, "Goal amount must be greater than zero.");

            if (updated.CurrentAmount < 0)
                return (false, "Current amount cannot be negative.");

            existing.GoalName = updated.GoalName;
            existing.GoalAmount = updated.GoalAmount;
            existing.CurrentAmount = updated.CurrentAmount;
            existing.TargetDate = updated.TargetDate;
            existing.IsCompleted = existing.CurrentAmount >= existing.GoalAmount;

            Save();
            return (true, "Savings goal updated successfully.");
        }

        public (bool Success, string Message) DeleteGoal(int goalId)
        {
            var goal = _savings.FirstOrDefault(s => s.Id == goalId);
            if (goal == null)
                return (false, "Goal not found.");

            _savings.Remove(goal);
            Save();
            return (true, "Savings goal deleted successfully.");
        }

        public (bool Success, string Message) AddToGoal(int goalId, decimal amount)
        {
            var goal = _savings.FirstOrDefault(s => s.Id == goalId);
            if (goal == null)
                return (false, "Goal not found.");

            if (amount <= 0)
                return (false, "Amount must be greater than zero.");

            goal.CurrentAmount += amount;

            if (goal.CurrentAmount >= goal.GoalAmount)
            {
                goal.IsCompleted = true;
                Save();
                return (true, $"Congratulations! You have reached your savings goal '{goal.GoalName}'!");
            }

            Save();
            return (true, $"{amount:C} added to '{goal.GoalName}'. Progress: {goal.ProgressPercent:F0}%");
        }

        public decimal GetTotalSavings(string username)
        {
            return _savings.Where(s => s.Username == username && !s.IsCompleted)
                          .Sum(s => s.CurrentAmount);
        }

        public decimal GetTotalGoals(string username)
        {
            return _savings.Where(s => s.Username == username && !s.IsCompleted)
                          .Sum(s => s.GoalAmount);
        }

        public Savings GetGoalById(int id)
        {
            return _savings.FirstOrDefault(s => s.Id == id);
        }

        public void Save()
        {
            FileManager.SaveSavings(_savings);
        }

        public void Reload()
        {
            _savings = FileManager.LoadSavings();
            _nextId = _savings.Any() ? _savings.Max(s => s.Id) + 1 : 1;
        }
    }
}
