using SmartBudgetMobile.Managers;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Pages;

public partial class ExpensesPage : ContentPage
{
    private readonly ExpenseManager _expenseManager;
    private readonly BudgetManager _budgetManager;
    private readonly ExpenseTemplateManager _templateManager;
    private readonly AuthenticationManager _authManager;
    private int? _selectedExpenseId;
    private string _currentFilter = "All";

    private static readonly string[] Units = { "pc", "kg", "L", "pack", "sack", "bottle", "box" };

    public ExpensesPage(AuthenticationManager authManager, ExpenseManager expenseManager,
        BudgetManager budgetManager, ExpenseTemplateManager templateManager)
    {
        InitializeComponent();
        _authManager = authManager;
        _expenseManager = expenseManager;
        _budgetManager = budgetManager;
        _templateManager = templateManager;
        pickerCategory.ItemsSource = Enum.GetValues<ExpenseCategory>().Cast<ExpenseCategory>().Select(e => e.ToString()).ToList();
        pickerUnit.ItemsSource = Units.ToList();
        pickerFilter.SelectedIndex = 0;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadExpenses();
        LoadTemplates();
    }

    private string Username => _authManager.CurrentUser?.Username ?? "";

    private void LoadExpenses()
    {
        List<Expense> expenses = _currentFilter switch
        {
            "Today" => _expenseManager.GetTodayExpenses(Username),
            "This Week" => _expenseManager.GetThisWeekExpenses(Username),
            "This Month" => _expenseManager.GetThisMonthExpenses(Username),
            "This Year" => _expenseManager.GetThisYearExpenses(Username),
            _ => _expenseManager.GetExpensesByUser(Username)
        };
        string search = txtSearch.Text?.Trim().ToLower() ?? "";
        if (!string.IsNullOrEmpty(search))
            expenses = expenses.Where(e =>
                e.ExpenseName.ToLower().Contains(search) ||
                e.Category.ToString().ToLower().Contains(search) ||
                (e.Notes?.ToLower().Contains(search) == true)).ToList();
        expensesList.ItemsSource = null;
        expensesList.ItemsSource = expenses;
    }

    private void LoadTemplates()
    {
        templatesList.ItemsSource = null;
        templatesList.ItemsSource = _templateManager.GetTemplatesByUser(Username);
    }

    private void ResetForm()
    {
        txtExpenseName.Text = "";
        pickerCategory.SelectedIndex = -1;
        txtQuantity.Text = "";
        pickerUnit.SelectedIndex = -1;
        txtPricePerUnit.Text = "";
        txtAmount.Text = "";
        datePicker.Date = DateTime.Today;
        txtNotes.Text = "";
        _selectedExpenseId = null;
        btnAdd.IsEnabled = true;
        btnUpdate.IsEnabled = false;
        btnDelete.IsEnabled = false;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e) => LoadExpenses();

    private void OnFilterChanged(object sender, EventArgs e)
    {
        _currentFilter = pickerFilter.SelectedItem as string ?? "All";
        pickerMonth.IsVisible = _currentFilter == "This Month" || _currentFilter == "This Year";
        LoadExpenses();
    }

    private void OnMonthChanged(object sender, EventArgs e) => LoadExpenses();

    private void OnQtyOrPriceChanged(object sender, TextChangedEventArgs e)
    {
        decimal.TryParse(txtQuantity.Text, out decimal qty);
        decimal.TryParse(txtPricePerUnit.Text, out decimal price);
        decimal amount = qty * price;
        txtAmount.Text = amount > 0 ? amount.ToString("F2") : "";
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtExpenseName.Text))
        {
            await DisplayAlert("Error", "Expense name is required.", "OK");
            return;
        }
        if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
        {
            await DisplayAlert("Error", "Enter a valid amount.", "OK");
            return;
        }
        if (pickerCategory.SelectedIndex < 0)
        {
            await DisplayAlert("Error", "Select a category.", "OK");
            return;
        }
        var expense = new Expense
        {
            ExpenseName = txtExpenseName.Text.Trim(),
            Category = (ExpenseCategory)Enum.Parse(typeof(ExpenseCategory), pickerCategory.SelectedItem as string),
            Amount = amount,
            Quantity = decimal.TryParse(txtQuantity.Text, out decimal qty) ? qty : 0,
            PricePerUnit = decimal.TryParse(txtPricePerUnit.Text, out decimal ppu) ? ppu : 0,
            UnitOfMeasure = pickerUnit.SelectedItem as string ?? "",
            Date = datePicker.Date,
            Notes = txtNotes.Text?.Trim() ?? "",
            Username = Username
        };
        var (success, message) = _expenseManager.AddExpense(expense);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            _budgetManager.RecalculateSpent(Username, _expenseManager);
            ResetForm();
            LoadExpenses();
        }
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (!_selectedExpenseId.HasValue) return;
        if (string.IsNullOrWhiteSpace(txtExpenseName.Text))
        {
            await DisplayAlert("Error", "Expense name is required.", "OK");
            return;
        }
        if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
        {
            await DisplayAlert("Error", "Enter a valid amount.", "OK");
            return;
        }
        if (pickerCategory.SelectedIndex < 0)
        {
            await DisplayAlert("Error", "Select a category.", "OK");
            return;
        }
        var expense = new Expense
        {
            Id = _selectedExpenseId.Value,
            ExpenseName = txtExpenseName.Text.Trim(),
            Category = (ExpenseCategory)Enum.Parse(typeof(ExpenseCategory), pickerCategory.SelectedItem as string),
            Amount = amount,
            Quantity = decimal.TryParse(txtQuantity.Text, out decimal qty) ? qty : 0,
            PricePerUnit = decimal.TryParse(txtPricePerUnit.Text, out decimal ppu) ? ppu : 0,
            UnitOfMeasure = pickerUnit.SelectedItem as string ?? "",
            Date = datePicker.Date,
            Notes = txtNotes.Text?.Trim() ?? "",
            Username = Username
        };
        var (success, message) = _expenseManager.UpdateExpense(expense);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            _budgetManager.RecalculateSpent(Username, _expenseManager);
            ResetForm();
            LoadExpenses();
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (!_selectedExpenseId.HasValue) return;
        bool confirm = await DisplayAlert("Confirm", "Delete this expense?", "Yes", "No");
        if (!confirm) return;
        var (success, message) = _expenseManager.DeleteExpense(_selectedExpenseId.Value);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success)
        {
            _budgetManager.RecalculateSpent(Username, _expenseManager);
            ResetForm();
            LoadExpenses();
        }
    }

    private void OnResetClicked(object sender, EventArgs e) => ResetForm();

    private void OnExpenseSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Expense expense) return;
        _selectedExpenseId = expense.Id;
        txtExpenseName.Text = expense.ExpenseName;
        pickerCategory.SelectedIndex = (int)expense.Category;
        txtQuantity.Text = expense.Quantity > 0 ? expense.Quantity.ToString() : "";
        pickerUnit.SelectedIndex = Array.IndexOf(Units, expense.UnitOfMeasure);
        txtPricePerUnit.Text = expense.PricePerUnit > 0 ? expense.PricePerUnit.ToString("F2") : "";
        txtAmount.Text = expense.Amount.ToString("F2");
        datePicker.Date = expense.Date;
        txtNotes.Text = expense.Notes ?? "";
        btnAdd.IsEnabled = false;
        btnUpdate.IsEnabled = true;
        btnDelete.IsEnabled = true;
    }

    private void OnTemplateSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ExpenseTemplate template) return;
        pickerCategory.SelectedIndex = (int)template.Category;
        pickerUnit.SelectedIndex = Array.IndexOf(Units, template.DefaultUnit);
        if (template.PricePerUnit > 0)
        {
            txtPricePerUnit.Text = template.PricePerUnit.ToString("F2");
            OnQtyOrPriceChanged(null, null);
        }
    }

    private async void OnSaveTemplateClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtExpenseName.Text))
        {
            await DisplayAlert("Error", "Enter an expense name first.", "OK");
            return;
        }
        if (pickerCategory.SelectedIndex < 0)
        {
            await DisplayAlert("Error", "Select a category.", "OK");
            return;
        }
        string name = await DisplayPromptAsync("Save Template", "Template name:", initialValue: txtExpenseName.Text.Trim());
        if (string.IsNullOrWhiteSpace(name)) return;
        var template = new ExpenseTemplate
        {
            TemplateName = name,
            Category = (ExpenseCategory)Enum.Parse(typeof(ExpenseCategory), pickerCategory.SelectedItem as string),
            PricePerUnit = decimal.TryParse(txtPricePerUnit.Text, out decimal ppu) ? ppu : 0,
            DefaultUnit = pickerUnit.SelectedItem as string ?? "",
            Username = Username
        };
        var (success, message) = _templateManager.AddTemplate(template);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success) LoadTemplates();
    }

    private async void OnDeleteTemplateClicked(object sender, EventArgs e)
    {
        if (templatesList.SelectedItem is not ExpenseTemplate template)
        {
            await DisplayAlert("Error", "Select a template first.", "OK");
            return;
        }
        bool confirm = await DisplayAlert("Confirm", $"Delete template '{template.TemplateName}'?", "Yes", "No");
        if (!confirm) return;
        var (success, message) = _templateManager.DeleteTemplate(template.Id);
        await DisplayAlert(success ? "Success" : "Error", message, "OK");
        if (success) LoadTemplates();
    }
}
