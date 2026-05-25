using SmartBudgetMobile.Managers;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Pages;

public partial class SavingsPage : ContentPage
{
    private readonly SavingsManager _savingsManager;
    private readonly AuthenticationManager _authManager;
    private int? _selectedGoalId;

    public SavingsPage(AuthenticationManager authManager, SavingsManager savingsManager)
    {
        InitializeComponent();
        _authManager = authManager;
        _savingsManager = savingsManager;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadGoals();
    }

    private string Username => _authManager.CurrentUser?.Username ?? "";

    private void LoadGoals()
    {
        var goals = _savingsManager.GetAllSavings(Username);
        decimal totalSaved = goals.Sum(g => g.CurrentAmount);
        decimal totalTarget = goals.Sum(g => g.GoalAmount);
        decimal overallProgress = totalTarget > 0 ? totalSaved / totalTarget * 100 : 0;

        lblTotalSavings.Text = totalSaved.ToString("C");
        lblProgress.Text = $"{overallProgress:F0}%";

        var display = goals.Select(g => new GoalDisplay
        {
            Id = g.Id,
            GoalName = g.GoalName,
            GoalAmount = g.GoalAmount,
            CurrentAmount = g.CurrentAmount,
            ProgressPercent = g.ProgressPercent,
            ProgressDecimal = g.GoalAmount > 0 ? (double)(g.CurrentAmount / g.GoalAmount) : 0.0,
            StatusText = g.IsCompleted ? "Completed" : $"{g.RemainingAmount:C} remaining",
            StatusTextColor = g.IsCompleted ? Color.FromArgb("#2ECC71") : Color.FromArgb("#7F8C8D"),
            IsCompleted = g.IsCompleted
        }).ToList();

        goalsList.ItemsSource = null;
        goalsList.ItemsSource = display;
    }

    private void ResetForm()
    {
        txtGoalName.Text = "";
        txtGoalAmount.Text = "";
        txtCurrentAmount.Text = "";
        txtAddAmount.Text = "";
        dateTargetDate.Date = DateTime.Today.AddMonths(6);
        _selectedGoalId = null;
        btnAddGoal.IsEnabled = true;
        btnUpdateGoal.IsEnabled = false;
        btnDeleteGoal.IsEnabled = false;
        btnAddToGoal.IsEnabled = false;
    }

    private async void OnAddGoalClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtGoalName.Text))
        {
            await DisplayAlert("Error", "Goal name is required.", "OK");
            return;
        }
        if (!decimal.TryParse(txtGoalAmount.Text, out decimal goalAmount) || goalAmount <= 0)
        {
            await DisplayAlert("Error", "Enter a valid target amount.", "OK");
            return;
        }
        decimal current = decimal.TryParse(txtCurrentAmount.Text, out decimal c) ? c : 0;
        var savings = new Savings
        {
            GoalName = txtGoalName.Text.Trim(),
            GoalAmount = goalAmount,
            CurrentAmount = current,
            TargetDate = dateTargetDate.Date,
            Username = Username
        };
        var (success, message) = _savingsManager.AddGoal(savings);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            ResetForm();
            LoadGoals();
        }
    }

    private async void OnUpdateGoalClicked(object sender, EventArgs e)
    {
        if (!_selectedGoalId.HasValue) return;
        if (string.IsNullOrWhiteSpace(txtGoalName.Text))
        {
            await DisplayAlert("Error", "Goal name is required.", "OK");
            return;
        }
        if (!decimal.TryParse(txtGoalAmount.Text, out decimal goalAmount) || goalAmount <= 0)
        {
            await DisplayAlert("Error", "Enter a valid target amount.", "OK");
            return;
        }
        decimal current = decimal.TryParse(txtCurrentAmount.Text, out decimal c) ? c : 0;
        var savings = new Savings
        {
            Id = _selectedGoalId.Value,
            GoalName = txtGoalName.Text.Trim(),
            GoalAmount = goalAmount,
            CurrentAmount = current,
            TargetDate = dateTargetDate.Date,
            Username = Username
        };
        var (success, message) = _savingsManager.UpdateGoal(savings);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            ResetForm();
            LoadGoals();
        }
    }

    private async void OnDeleteGoalClicked(object sender, EventArgs e)
    {
        if (!_selectedGoalId.HasValue) return;
        bool confirm = await DisplayAlert("Confirm", "Delete this savings goal?", "Yes", "No");
        if (!confirm) return;
        var (success, message) = _savingsManager.DeleteGoal(_selectedGoalId.Value);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            ResetForm();
            LoadGoals();
        }
    }

    private async void OnAddToGoalClicked(object sender, EventArgs e)
    {
        if (!_selectedGoalId.HasValue) return;
        if (!decimal.TryParse(txtAddAmount.Text, out decimal amount) || amount <= 0)
        {
            await DisplayAlert("Error", "Enter a valid amount to add.", "OK");
            return;
        }
        var (success, message) = _savingsManager.AddToGoal(_selectedGoalId.Value, amount);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            txtAddAmount.Text = "";
            LoadGoals();
        }
    }

    private void OnResetClicked(object sender, EventArgs e) => ResetForm();

    private void OnGoalSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not GoalDisplay goal) return;
        _selectedGoalId = goal.Id;
        txtGoalName.Text = goal.GoalName;
        txtGoalAmount.Text = goal.GoalAmount.ToString("F2");
        txtCurrentAmount.Text = goal.CurrentAmount.ToString("F2");
        btnAddGoal.IsEnabled = false;
        btnUpdateGoal.IsEnabled = true;
        btnDeleteGoal.IsEnabled = true;
        btnAddToGoal.IsEnabled = !goal.IsCompleted;
    }

    private class GoalDisplay
    {
        public int Id { get; set; }
        public string GoalName { get; set; }
        public decimal GoalAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal ProgressPercent { get; set; }
        public double ProgressDecimal { get; set; }
        public string StatusText { get; set; }
        public Color StatusTextColor { get; set; }
        public bool IsCompleted { get; set; }
    }
}
