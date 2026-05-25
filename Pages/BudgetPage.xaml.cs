using SmartBudgetMobile.Managers;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Pages;

public partial class BudgetPage : ContentPage
{
    private readonly BudgetManager _budgetManager;
    private readonly ExpenseManager _expenseManager;
    private readonly AuthenticationManager _authManager;
    private BudgetType? _selectedType;

    public BudgetPage(AuthenticationManager authManager, BudgetManager budgetManager, ExpenseManager expenseManager)
    {
        InitializeComponent();
        _authManager = authManager;
        _budgetManager = budgetManager;
        _expenseManager = expenseManager;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _budgetManager.RecalculateSpent(Username, _expenseManager);
        LoadBudgets();
    }

    private string Username => _authManager.CurrentUser?.Username ?? "";

    private void LoadBudgets()
    {
        var budgets = _budgetManager.GetAllBudgets(Username);
        var display = budgets.Select(b => new BudgetDisplay
        {
            BudgetType = b.BudgetType,
            Amount = b.Amount,
            Spent = b.Spent,
            RemainingBalance = b.Amount - b.Spent,
            UsagePercent = b.Amount > 0 ? (double)(b.Spent / b.Amount) : 0,
            UsageColor = b.Amount > 0 && b.Spent >= b.Amount ? Colors.Red :
                         b.Amount > 0 && b.Spent >= b.Amount * 0.8m ? Colors.Orange : Colors.Green
        }).ToList();
        budgetsList.ItemsSource = null;
        budgetsList.ItemsSource = display;
    }

    private void UpdateSpentInfo()
    {
        if (!_selectedType.HasValue)
        {
            lblCurrentSpent.Text = "₱0";
            lblRemaining.Text = "₱0";
            return;
        }
        var budget = _budgetManager.GetBudgetByType(Username, _selectedType.Value);
        decimal spent = budget?.Spent ?? 0;
        decimal amount = budget?.Amount ?? 0;
        decimal remaining = amount - spent;
        lblCurrentSpent.Text = spent.ToString("C");
        lblRemaining.Text = remaining.ToString("C");
        lblRemaining.TextColor = remaining >= 0 ? Color.FromArgb("#155724") : Colors.Red;
        lblCurrentSpent.TextColor = spent > amount ? Colors.Red : Color.FromArgb("#856404");
    }

    private void OnBudgetTypeChanged(object sender, EventArgs e)
    {
        if (pickerBudgetType.SelectedIndex < 0) return;
        _selectedType = (BudgetType)pickerBudgetType.SelectedIndex;
        var budget = _budgetManager.GetBudgetByType(Username, _selectedType.Value);
        txtAmount.Text = budget?.Amount.ToString("F2") ?? "";
        UpdateSpentInfo();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_selectedType == null)
        {
            await DisplayAlert("Error", "Select a budget type.", "OK");
            return;
        }
        if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
        {
            await DisplayAlert("Error", "Enter a valid budget amount.", "OK");
            return;
        }
        var (success, message) = _budgetManager.SetBudget(Username, _selectedType.Value, amount);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            _budgetManager.RecalculateSpent(Username, _expenseManager);
            UpdateSpentInfo();
            LoadBudgets();
        }
    }

    private async void OnResetClicked(object sender, EventArgs e)
    {
        if (_selectedType == null)
        {
            await DisplayAlert("Error", "Select a budget type.", "OK");
            return;
        }
        bool confirm = await DisplayAlert("Confirm", $"Reset {_selectedType} budget?", "Yes", "No");
        if (!confirm) return;
        var (success, message) = _budgetManager.ResetBudget(Username, _selectedType.Value);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            txtAmount.Text = "";
            UpdateSpentInfo();
            LoadBudgets();
        }
    }

    private void OnBudgetSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not BudgetDisplay display) return;
        pickerBudgetType.SelectedIndex = (int)display.BudgetType;
    }

    private class BudgetDisplay
    {
        public BudgetType BudgetType { get; set; }
        public decimal Amount { get; set; }
        public decimal Spent { get; set; }
        public decimal RemainingBalance { get; set; }
        public double UsagePercent { get; set; }
        public Color UsageColor { get; set; }
    }
}
