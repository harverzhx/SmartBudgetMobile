using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Managers
{
    public class ReportManager
    {
        private readonly ExpenseManager _expenseManager;
        private readonly BudgetManager _budgetManager;
        private readonly BillManager _billManager;
        private readonly SavingsManager _savingsManager;
        private readonly string _username;

        public ReportManager(ExpenseManager expenseManager, BudgetManager budgetManager,
            BillManager billManager, SavingsManager savingsManager, string username)
        {
            _expenseManager = expenseManager;
            _budgetManager = budgetManager;
            _billManager = billManager;
            _savingsManager = savingsManager;
            _username = username;
        }

        public string GenerateDailyReport()
        {
            var sb = new StringBuilder();
            var now = DateTime.Now;
            var expenses = _expenseManager.GetTodayExpenses(_username);

            sb.AppendLine("================================================");
            sb.AppendLine("        SMART BUDGET TRACKER - DAILY REPORT");
            sb.AppendLine($"        Date: {now:MMMM dd, yyyy}");
            sb.AppendLine("================================================");
            sb.AppendLine();
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            decimal dailyBudget = GetBudgetForType(BudgetType.Daily);
            decimal dailySpent = _expenseManager.GetDailyTotal(_username);

            sb.AppendLine("--- BUDGET SUMMARY ---");
            sb.AppendLine($"Daily Budget: {dailyBudget:C}");
            sb.AppendLine($"Total Spent Today: {dailySpent:C}");
            sb.AppendLine($"Remaining: {(dailyBudget - dailySpent):C}");
            sb.AppendLine();

            sb.AppendLine("--- EXPENSES ---");
            if (expenses.Count == 0)
            {
                sb.AppendLine("No expenses recorded today.");
            }
            else
            {
                sb.AppendLine($"{"Name",-25} {"Category",-18} {"Amount",-12} {"Time",-10}");
                sb.AppendLine(new string('-', 65));
                foreach (var exp in expenses.OrderBy(e => e.Date))
                {
                    sb.AppendLine($"{exp.ExpenseName,-25} {exp.Category,-18} {exp.Amount,-12:C} {exp.Date:hh:mm tt}");
                }
            }
            sb.AppendLine();

            AppendWarnings(sb, dailyBudget, dailySpent);
            AppendFooter(sb);

            return sb.ToString();
        }

        public string GenerateWeeklyReport()
        {
            var sb = new StringBuilder();
            var now = DateTime.Now;
            var weekStart = now.AddDays(-(int)now.DayOfWeek);
            var expenses = _expenseManager.GetThisWeekExpenses(_username);

            sb.AppendLine("================================================");
            sb.AppendLine("       SMART BUDGET TRACKER - WEEKLY REPORT");
            sb.AppendLine($"        Week of {weekStart:MMM dd} - {now:MMM dd, yyyy}");
            sb.AppendLine("================================================");
            sb.AppendLine();
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            decimal weeklyBudget = GetBudgetForType(BudgetType.Weekly);
            decimal weeklySpent = _expenseManager.GetWeeklyTotal(_username);

            sb.AppendLine("--- BUDGET SUMMARY ---");
            sb.AppendLine($"Weekly Budget: {weeklyBudget:C}");
            sb.AppendLine($"Total Spent This Week: {weeklySpent:C}");
            sb.AppendLine($"Remaining: {(weeklyBudget - weeklySpent):C}");
            sb.AppendLine();

            sb.AppendLine("--- EXPENSES ---");
            if (expenses.Count == 0)
            {
                sb.AppendLine("No expenses recorded this week.");
            }
            else
            {
                foreach (var day in expenses.GroupBy(e => e.Date.Date).OrderBy(g => g.Key))
                {
                    sb.AppendLine($"  {day.Key:dddd, MMM dd} - Total: {day.Sum(e => e.Amount):C}");
                    sb.AppendLine($"  {"Name",-25} {"Category",-18} {"Amount",-12}");
                    sb.AppendLine($"  {new string('-', 55)}");
                    foreach (var exp in day)
                    {
                        sb.AppendLine($"  {exp.ExpenseName,-25} {exp.Category,-18} {exp.Amount,-12:C}");
                    }
                    sb.AppendLine();
                }
            }

            AppendCategoryBreakdown(sb, expenses);
            AppendWarnings(sb, weeklyBudget, weeklySpent);
            AppendFooter(sb);

            return sb.ToString();
        }

        public string GenerateMonthlyReport()
        {
            var sb = new StringBuilder();
            var now = DateTime.Now;
            var expenses = _expenseManager.GetThisMonthExpenses(_username);

            sb.AppendLine("================================================");
            sb.AppendLine("       SMART BUDGET TRACKER - MONTHLY REPORT");
            sb.AppendLine($"        {now:MMMM yyyy}");
            sb.AppendLine("================================================");
            sb.AppendLine();
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            decimal monthlyBudget = GetBudgetForType(BudgetType.Monthly);
            decimal monthlySpent = _expenseManager.GetMonthlyTotal(_username);

            sb.AppendLine("--- BUDGET SUMMARY ---");
            sb.AppendLine($"Monthly Budget: {monthlyBudget:C}");
            sb.AppendLine($"Total Spent This Month: {monthlySpent:C}");
            sb.AppendLine($"Remaining: {(monthlyBudget - monthlySpent):C}");
            sb.AppendLine();

            sb.AppendLine("--- CATEGORY BREAKDOWN ---");
            var breakdown = _expenseManager.GetCategoryBreakdown(_username);
            foreach (var cat in breakdown.OrderByDescending(c => c.Value))
            {
                decimal pct = monthlySpent > 0 ? (cat.Value / monthlySpent) * 100 : 0;
                sb.AppendLine($"  {cat.Key,-18} {cat.Value,12:C} ({pct,5:F1}%)");
            }
            sb.AppendLine();

            sb.AppendLine("--- BILLS STATUS ---");
            var bills = _billManager.GetAllBills(_username);
            var unpaid = bills.Where(b => b.Status != BillStatus.Paid).ToList();
            if (unpaid.Count == 0)
                sb.AppendLine("  All bills are paid.");
            else
            {
                sb.AppendLine($"  {"Bill Name",-25} {"Amount",-12} {"Due Date",-15} {"Status",-10}");
                sb.AppendLine($"  {new string('-', 62)}");
                foreach (var b in unpaid)
                {
                    sb.AppendLine($"  {b.BillName,-25} {b.Amount,-12:C} {b.DueDate:MMM dd, yyyy,-15} {b.Status,-10}");
                }
            }
            sb.AppendLine();

            AppendSavingsSummary(sb);
            AppendWarnings(sb, monthlyBudget, monthlySpent);
            AppendFooter(sb);

            return sb.ToString();
        }

        public string GenerateYearlyReport()
        {
            var sb = new StringBuilder();
            var now = DateTime.Now;
            var expenses = _expenseManager.GetThisYearExpenses(_username);

            sb.AppendLine("================================================");
            sb.AppendLine("       SMART BUDGET TRACKER - YEARLY REPORT");
            sb.AppendLine($"        Year {now.Year}");
            sb.AppendLine("================================================");
            sb.AppendLine();
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            decimal yearlyBudget = GetBudgetForType(BudgetType.Yearly);
            decimal yearlySpent = _expenseManager.GetYearlyTotal(_username);

            sb.AppendLine("--- ANNUAL SUMMARY ---");
            sb.AppendLine($"Yearly Budget: {yearlyBudget:C}");
            sb.AppendLine($"Total Spent This Year: {yearlySpent:C}");
            sb.AppendLine($"Remaining: {(yearlyBudget - yearlySpent):C}");
            sb.AppendLine();

            sb.AppendLine("--- MONTHLY BREAKDOWN ---");
            var trend = _expenseManager.GetMonthlyTrend(_username, now.Year);
            foreach (var month in trend)
            {
                sb.AppendLine($"  {month.Key,-10} {month.Value,12:C}");
            }
            sb.AppendLine();

            sb.AppendLine("--- CATEGORY BREAKDOWN ---");
            var breakdown = _expenseManager.GetCategoryBreakdown(_username);
            foreach (var cat in breakdown.OrderByDescending(c => c.Value))
            {
                decimal pct = yearlySpent > 0 ? (cat.Value / yearlySpent) * 100 : 0;
                sb.AppendLine($"  {cat.Key,-18} {cat.Value,12:C} ({pct,5:F1}%)");
            }
            sb.AppendLine();

            AppendSavingsSummary(sb);
            AppendFooter(sb);

            return sb.ToString();
        }

        public string GenerateExpenseSummary()
        {
            var sb = new StringBuilder();
            var expenses = _expenseManager.GetExpensesByUser(_username);
            var total = expenses.Sum(e => e.Amount);

            sb.AppendLine("================================================");
            sb.AppendLine("       SMART BUDGET TRACKER - EXPENSE SUMMARY");
            sb.AppendLine("================================================");
            sb.AppendLine();
            sb.AppendLine($"Total Expenses All Time: {total:C}");
            sb.AppendLine($"Total Number of Expenses: {expenses.Count}");
            sb.AppendLine();
            sb.AppendLine("--- CATEGORY BREAKDOWN ---");
            var breakdown = _expenseManager.GetCategoryBreakdown(_username);
            foreach (var cat in breakdown.OrderByDescending(c => c.Value))
            {
                int count = expenses.Count(e => e.Category == cat.Key);
                decimal pct = total > 0 ? (cat.Value / total) * 100 : 0;
                sb.AppendLine($"  {cat.Key,-18} {cat.Value,12:C} ({pct,5:F1}%) - {count} expense(s)");
            }
            sb.AppendLine();

            var highest = _expenseManager.GetHighestCategory(_username);
            sb.AppendLine($"Highest Spending Category: {highest.Key} ({highest.Value:C})");
            sb.AppendLine();

            AppendFooter(sb);
            return sb.ToString();
        }

        public string GenerateBillSummary()
        {
            var sb = new StringBuilder();
            var bills = _billManager.GetAllBills(_username);

            sb.AppendLine("================================================");
            sb.AppendLine("       SMART BUDGET TRACKER - BILL SUMMARY");
            sb.AppendLine("================================================");
            sb.AppendLine();
            sb.AppendLine($"Total Bills: {bills.Count}");
            sb.AppendLine($"Total Unpaid: {_billManager.GetTotalUnpaid(_username):C}");
            sb.AppendLine($"Total Paid: {_billManager.GetTotalPaid(_username):C}");
            sb.AppendLine();

            sb.AppendLine("--- UNPAID BILLS ---");
            var unpaid = bills.Where(b => b.Status != BillStatus.Paid).ToList();
            if (unpaid.Count == 0)
            {
                sb.AppendLine("  No unpaid bills.");
            }
            else
            {
                sb.AppendLine($"  {"Name",-25} {"Amount",-12} {"Due Date",-15} {"Status",-10}");
                sb.AppendLine($"  {new string('-', 62)}");
                foreach (var b in unpaid)
                {
                    sb.AppendLine($"  {b.BillName,-25} {b.Amount,-12:C} {b.DueDate:MMM dd, yyyy,-15} {b.Status,-10}");
                }
            }
            sb.AppendLine();

            sb.AppendLine("--- PAID BILLS ---");
            var paid = bills.Where(b => b.Status == BillStatus.Paid).ToList();
            if (paid.Count == 0)
            {
                sb.AppendLine("  No paid bills.");
            }
            else
            {
                foreach (var b in paid)
                {
                    sb.AppendLine($"  {b.BillName,-25} {b.Amount,-12:C} Paid on: {b.PaidAt:MMM dd, yyyy}");
                }
            }
            sb.AppendLine();

            AppendFooter(sb);
            return sb.ToString();
        }

        public string GenerateFullReport()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GenerateMonthlyReport());
            sb.AppendLine(GenerateExpenseSummary());
            sb.AppendLine(GenerateBillSummary());

            var savings = _savingsManager.GetActiveGoals(_username);
            if (savings.Any())
            {
                sb.AppendLine("--- SAVINGS GOALS ---");
                foreach (var s in savings)
                {
                    sb.AppendLine($"  {s.GoalName} - {s.CurrentAmount:C} / {s.GoalAmount:C} ({s.ProgressPercent:F0}%)");
                }
            }

            return sb.ToString();
        }

        public string ToCsv(string report)
        {
            var lines = report.Split('\n');
            var sb = new StringBuilder();
            sb.AppendLine("Smart Budget Tracker Report");
            sb.AppendLine($"Generated,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            foreach (var line in lines)
            {
                if (line.Contains(":") && !line.StartsWith("---") && !line.StartsWith("=="))
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        sb.AppendLine($"\"{parts[0].Trim()}\",\"{parts[1].Trim()}\"");
                    }
                }
            }

            return sb.ToString();
        }

        public void ExportToTxt(string filePath, string content)
        {
            FileManager.ExportToTxt(filePath, content);
        }

        public void ExportToCsv(string filePath, string content)
        {
            FileManager.ExportToCsv(filePath, content);
        }

        public void ExportToPdf(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, "Smart Budget Tracker - Report" + Environment.NewLine);
                File.AppendAllText(filePath, $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}" + Environment.NewLine);
                File.AppendAllText(filePath, Environment.NewLine + content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export PDF: {ex.Message}");
            }
        }

        public void ExportToExcel(string filePath)
        {
            try
            {
                var lines = new List<string>();
                lines.Add("Smart Budget Tracker - Expense Report");
                lines.Add($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                lines.Add($"User: {_username}");
                lines.Add("");
                lines.Add("Name,Category,Amount,Date,Notes");

                var expenses = _expenseManager.GetExpensesByUser(_username);
                foreach (var exp in expenses)
                {
                    lines.Add($"{exp.ExpenseName},{exp.Category},{exp.Amount},{exp.Date:yyyy-MM-dd},{exp.Notes ?? ""}");
                }

                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export Excel: {ex.Message}");
            }
        }

        private decimal GetBudgetForType(BudgetType type)
        {
            var budget = _budgetManager.GetBudgetByType(_username, type);
            return budget?.Amount ?? 0;
        }

        private void AppendCategoryBreakdown(StringBuilder sb, List<Expense> expenses)
        {
            if (expenses.Count == 0) return;

            sb.AppendLine("--- CATEGORY BREAKDOWN ---");
            var breakdown = expenses.GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
            decimal total = expenses.Sum(e => e.Amount);

            foreach (var cat in breakdown.OrderByDescending(c => c.Value))
            {
                decimal pct = total > 0 ? (cat.Value / total) * 100 : 0;
                sb.AppendLine($"  {cat.Key,-18} {cat.Value,12:C} ({pct,5:F1}%)");
            }
            sb.AppendLine();
        }

        private void AppendWarnings(StringBuilder sb, decimal budget, decimal spent)
        {
            if (budget > 0 && spent > budget)
            {
                sb.AppendLine("*** WARNING: Budget has been exceeded! ***");
                sb.AppendLine($"    Overspent by {(spent - budget):C}");
                sb.AppendLine();
            }
            else if (budget > 0 && spent >= budget * 0.8m)
            {
                sb.AppendLine("*** ALERT: You have used 80% or more of your budget. ***");
                sb.AppendLine();
            }

            var overdue = _billManager.GetOverdueBills(_username);
            if (overdue.Any())
            {
                sb.AppendLine("*** OVERDUE BILLS ***");
                foreach (var b in overdue)
                {
                    sb.AppendLine($"  {b.BillName} - {b.Amount:C} (Due: {b.DueDate:MMM dd, yyyy})");
                }
                sb.AppendLine();
            }
        }

        private void AppendSavingsSummary(StringBuilder sb)
        {
            var activeGoals = _savingsManager.GetActiveGoals(_username);
            if (activeGoals.Any())
            {
                sb.AppendLine("--- SAVINGS GOALS ---");
                foreach (var goal in activeGoals)
                {
                    sb.AppendLine($"  {goal.GoalName,-20} {goal.CurrentAmount,10:C} / {goal.GoalAmount,10:C} ({goal.ProgressPercent,5:F0}%)");
                }
                sb.AppendLine();
            }
        }

        private void AppendFooter(StringBuilder sb)
        {
            sb.AppendLine("================================================");
            sb.AppendLine("     Thank you for using Smart Budget Tracker!");
            sb.AppendLine("================================================");
        }
    }
}
