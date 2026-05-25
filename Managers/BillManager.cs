using System;
using System.Collections.Generic;
using System.Linq;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Managers
{
    public class BillManager
    {
        private List<Bill> _bills;
        private int _nextId;

        public BillManager()
        {
            _bills = FileManager.LoadBills();
            _nextId = _bills.Any() ? _bills.Max(b => b.Id) + 1 : 1;
        }

        public List<Bill> GetAllBills(string username)
        {
            return _bills.Where(b => b.Username == username)
                        .OrderBy(b => b.Status == BillStatus.Paid ? 1 : 0)
                        .ThenBy(b => b.DueDate)
                        .ToList();
        }

        public List<Bill> GetUnpaidBills(string username)
        {
            return _bills.Where(b => b.Username == username && b.Status != BillStatus.Paid)
                        .OrderBy(b => b.DueDate)
                        .ToList();
        }

        public List<Bill> GetDueBills(string username, int withinDays = 7)
        {
            return _bills.Where(b => b.Username == username &&
                b.Status != BillStatus.Paid &&
                b.DueDate <= DateTime.Now.AddDays(withinDays) &&
                b.DueDate >= DateTime.Now)
                .OrderBy(b => b.DueDate)
                .ToList();
        }

        public List<Bill> GetOverdueBills(string username)
        {
            return _bills.Where(b => b.Username == username && b.Status == BillStatus.Overdue)
                        .OrderBy(b => b.DueDate)
                        .ToList();
        }

        public (bool Success, string Message) AddBill(Bill bill)
        {
            if (string.IsNullOrWhiteSpace(bill.BillName))
                return (false, "Bill name cannot be empty.");

            if (bill.Amount <= 0)
                return (false, "Amount must be greater than zero.");

            bill.Id = _nextId++;
            bill.Status = BillStatus.Unpaid;
            bill.CreatedAt = DateTime.Now;
            _bills.Add(bill);

            Save();
            return (true, "Bill added successfully.");
        }

        public (bool Success, string Message) UpdateBill(Bill updatedBill)
        {
            var existing = _bills.FirstOrDefault(b => b.Id == updatedBill.Id);
            if (existing == null)
                return (false, "Bill not found.");

            if (string.IsNullOrWhiteSpace(updatedBill.BillName))
                return (false, "Bill name cannot be empty.");

            if (updatedBill.Amount <= 0)
                return (false, "Amount must be greater than zero.");

            existing.BillName = updatedBill.BillName;
            existing.Amount = updatedBill.Amount;
            existing.DueDate = updatedBill.DueDate;
            existing.Notes = updatedBill.Notes;
            existing.IsRecurring = updatedBill.IsRecurring;

            Save();
            return (true, "Bill updated successfully.");
        }

        public (bool Success, string Message) DeleteBill(int billId)
        {
            var bill = _bills.FirstOrDefault(b => b.Id == billId);
            if (bill == null)
                return (false, "Bill not found.");

            _bills.Remove(bill);
            Save();
            return (true, "Bill deleted successfully.");
        }

        public (bool Success, string Message) MarkAsPaid(int billId)
        {
            var bill = _bills.FirstOrDefault(b => b.Id == billId);
            if (bill == null)
                return (false, "Bill not found.");

            bill.Status = BillStatus.Paid;
            bill.PaidAt = DateTime.Now;

            if (bill.IsRecurring)
            {
                var recurring = new Bill
                {
                    BillName = bill.BillName,
                    Amount = bill.Amount,
                    DueDate = bill.DueDate.AddMonths(1),
                    Status = BillStatus.Unpaid,
                    Username = bill.Username,
                    Notes = bill.Notes,
                    IsRecurring = true,
                    CreatedAt = DateTime.Now
                };
                _bills.Add(recurring);
            }

            Save();
            return (true, $"Bill '{bill.BillName}' marked as paid.");
        }

        public decimal GetTotalUnpaid(string username)
        {
            return _bills.Where(b => b.Username == username && b.Status != BillStatus.Paid)
                        .Sum(b => b.Amount);
        }

        public decimal GetTotalPaid(string username)
        {
            return _bills.Where(b => b.Username == username && b.Status == BillStatus.Paid)
                        .Sum(b => b.Amount);
        }

        public Bill GetBillById(int id)
        {
            return _bills.FirstOrDefault(b => b.Id == id);
        }

        public void Save()
        {
            FileManager.SaveBills(_bills);
        }

        public void Reload()
        {
            _bills = FileManager.LoadBills();
            _nextId = _bills.Any() ? _bills.Max(b => b.Id) + 1 : 1;
        }
    }
}
