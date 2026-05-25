using SmartBudgetMobile.Managers;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Pages;

public partial class BillsPage : ContentPage
{
    private readonly BillManager _billManager;
    private readonly AuthenticationManager _authManager;
    private int? _selectedBillId;

    public BillsPage(AuthenticationManager authManager, BillManager billManager)
    {
        InitializeComponent();
        _authManager = authManager;
        _billManager = billManager;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadBills();
    }

    private string Username => _authManager.CurrentUser?.Username ?? "";

    private void LoadBills()
    {
        var bills = _billManager.GetAllBills(Username);
        var display = bills.Select(b =>
        {
            int daysLeft = (b.DueDate.Date - DateTime.Now.Date).Days;
            bool isOverdue = daysLeft < 0 && b.Status != BillStatus.Paid;
            if (isOverdue && b.Status != BillStatus.Paid)
            {
                b.Status = BillStatus.Overdue;
                _billManager.Save();
            }
            return new BillDisplay
            {
                Id = b.Id,
                BillName = b.BillName,
                Amount = b.Amount,
                DueDate = b.DueDate,
                StatusDisplay = isOverdue ? "Overdue" :
                    b.Status == BillStatus.Paid ? "Paid" :
                    daysLeft == 0 ? "Due Today" : $"{daysLeft} days left",
                StatusColor = isOverdue ? Colors.Red :
                    b.Status == BillStatus.Paid ? Color.FromArgb("#2ECC71") : Color.FromArgb("#E67E22"),
                NameColor = isOverdue ? Colors.Red :
                    b.Status == BillStatus.Paid ? Color.FromArgb("#2ECC71") : Color.FromArgb("#2C3E50"),
                DaysLeftDisplay = isOverdue ? $"Overdue by {-daysLeft} days" :
                    b.Status == BillStatus.Paid ? $"Paid on {b.PaidAt:MMM dd}" : "",
                Notes = b.Notes ?? "",
                IsRecurring = b.IsRecurring,
                Status = b.Status
            };
        }).ToList();
        billsList.ItemsSource = null;
        billsList.ItemsSource = display;
    }

    private void ResetForm()
    {
        txtBillName.Text = "";
        txtAmount.Text = "";
        dateDueDate.Date = DateTime.Today.AddDays(7);
        txtNotes.Text = "";
        switchRecurring.IsToggled = false;
        _selectedBillId = null;
        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
        btnMarkPaid.IsEnabled = false;
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtBillName.Text))
        {
            await DisplayAlert("Error", "Bill name is required.", "OK");
            return;
        }
        if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
        {
            await DisplayAlert("Error", "Enter a valid amount.", "OK");
            return;
        }
        var bill = new Bill
        {
            BillName = txtBillName.Text.Trim(),
            Amount = amount,
            DueDate = dateDueDate.Date,
            Notes = txtNotes.Text?.Trim() ?? "",
            IsRecurring = switchRecurring.IsToggled,
            Username = Username
        };
        var (success, message) = _billManager.AddBill(bill);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            ResetForm();
            LoadBills();
        }
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (!_selectedBillId.HasValue) return;
        if (string.IsNullOrWhiteSpace(txtBillName.Text))
        {
            await DisplayAlert("Error", "Bill name is required.", "OK");
            return;
        }
        if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
        {
            await DisplayAlert("Error", "Enter a valid amount.", "OK");
            return;
        }
        var bill = new Bill
        {
            Id = _selectedBillId.Value,
            BillName = txtBillName.Text.Trim(),
            Amount = amount,
            DueDate = dateDueDate.Date,
            Notes = txtNotes.Text?.Trim() ?? "",
            IsRecurring = switchRecurring.IsToggled,
            Username = Username
        };
        var (success, message) = _billManager.UpdateBill(bill);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            ResetForm();
            LoadBills();
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (!_selectedBillId.HasValue) return;
        bool confirm = await DisplayAlert("Confirm", "Delete this bill?", "Yes", "No");
        if (!confirm) return;
        var (success, message) = _billManager.DeleteBill(_selectedBillId.Value);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            ResetForm();
            LoadBills();
        }
    }

    private async void OnMarkPaidClicked(object sender, EventArgs e)
    {
        if (!_selectedBillId.HasValue) return;
        bool confirm = await DisplayAlert("Confirm", "Mark this bill as paid?", "Yes", "No");
        if (!confirm) return;
        var (success, message) = _billManager.MarkAsPaid(_selectedBillId.Value);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            ResetForm();
            LoadBills();
        }
    }

    private void OnResetClicked(object sender, EventArgs e) => ResetForm();

    private void OnBillSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not BillDisplay bill) return;
        _selectedBillId = bill.Id;
        txtBillName.Text = bill.BillName;
        txtAmount.Text = bill.Amount.ToString("F2");
        dateDueDate.Date = bill.DueDate;
        txtNotes.Text = bill.Notes;
        switchRecurring.IsToggled = bill.IsRecurring;
        btnAdd.IsEnabled = false;
        btnUpdate.IsEnabled = true;
        btnDelete.IsEnabled = true;
        btnMarkPaid.IsEnabled = bill.Status != BillStatus.Paid;
    }

    private class BillDisplay
    {
        public int Id { get; set; }
        public string BillName { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string StatusDisplay { get; set; }
        public Color StatusColor { get; set; }
        public Color NameColor { get; set; }
        public string DaysLeftDisplay { get; set; }
        public string Notes { get; set; }
        public bool IsRecurring { get; set; }
        public BillStatus Status { get; set; }
    }
}
