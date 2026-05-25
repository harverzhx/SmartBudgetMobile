using SmartBudgetMobile.Managers;
using SmartBudgetMobile.Models;

namespace SmartBudgetMobile.Pages;

public partial class DashboardPage : ContentPage
{
	private readonly AuthenticationManager _authManager;
	private readonly ExpenseManager _expenseManager;
	private readonly BudgetManager _budgetManager;
	private readonly IncomeManager _incomeManager;

	public DashboardPage(AuthenticationManager authManager, ExpenseManager expenseManager,
		BudgetManager budgetManager, IncomeManager incomeManager)
	{
		InitializeComponent();
		_authManager = authManager;
		_expenseManager = expenseManager;
		_budgetManager = budgetManager;
		_incomeManager = incomeManager;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		LoadDashboard();
	}

	private void LoadDashboard()
	{
		var username = _authManager.CurrentUser?.Username ?? "";
		lblWelcome.Text = $"Welcome, {_authManager.CurrentUser?.FullName ?? username}!";

		decimal totalIncome = _incomeManager.GetTotalIncome(username);
		decimal totalExpenses = _expenseManager.GetTotalExpenses(username);
		decimal balance = totalIncome - totalExpenses;
		decimal monthly = _expenseManager.GetMonthlyTotal(username);
		decimal weekly = _expenseManager.GetWeeklyTotal(username);
		decimal today = _expenseManager.GetDailyTotal(username);

		lblAllowance.Text = totalIncome.ToString("C");
		lblExpenses.Text = totalExpenses.ToString("C");
		lblBalance.Text = balance.ToString("C");
		lblBalance.TextColor = balance >= 0 ? Colors.DodgerBlue : Colors.Red;
		lblMonthly.Text = monthly.ToString("C");
		lblWeekly.Text = weekly.ToString("C");
		lblToday.Text = today.ToString("C");
	}

	private async void OnExpensesClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("expenses");
	}

	private async void OnBudgetClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("budget");
	}

	private async void OnBillsClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("bills");
	}

	private async void OnSavingsClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("savings");
	}

	private async void OnAllowanceClicked(object sender, EventArgs e)
	{
		string amountStr = await DisplayPromptAsync("Add Allowance", "Enter amount received:",
			keyboard: Keyboard.Numeric);
		if (string.IsNullOrWhiteSpace(amountStr)) return;

		if (!decimal.TryParse(amountStr, out decimal amount) || amount <= 0)
		{
			await DisplayAlert("Error", "Enter a valid amount.", "OK");
			return;
		}

		var income = new Income
		{
			Amount = amount,
			Date = DateTime.Now,
			Notes = "",
			Username = _authManager.CurrentUser?.Username ?? ""
		};

		var (success, message) = _incomeManager.AddIncome(income);
		await DisplayAlert(success ? "Success" : "Error", message, "OK");
		LoadDashboard();
	}

	private async void OnSettingsClicked(object sender, EventArgs e)
	{
		string action = await DisplayActionSheet("Settings", "Cancel", null,
			"Backup Data", "Restore from Backup", "Share Data", "Reset All Data", "Logout");

		switch (action)
		{
			case "Backup Data":
				try
				{
					FileManager.CreateBackup();
					await DisplayAlert("Success", "Backup created.", "OK");
				}
				catch (Exception ex)
				{
					await DisplayAlert("Error", ex.Message, "OK");
				}
				break;

			case "Restore from Backup":
				try
				{
					var backups = FileManager.GetAvailableBackups();
					if (backups.Count == 0)
					{
						await DisplayAlert("No Backups", "No backups found.", "OK");
						break;
					}

					string[] backupNames = backups.Select(b => Path.GetFileName(b)).ToArray();
					string selected = await DisplayActionSheet("Select Backup", "Cancel", null, backupNames);
					if (selected == null) break;

					string selectedPath = backups[Array.IndexOf(backupNames, selected)];
					FileManager.RestoreFromBackup(selectedPath);
					_expenseManager.Reload();
					_budgetManager.Reload();
					await DisplayAlert("Success", "Backup restored. Please re-login.", "OK");
					_authManager.Logout();
					await Shell.Current.GoToAsync("//login");
				}
				catch (Exception ex)
				{
					await DisplayAlert("Error", ex.Message, "OK");
				}
				break;

			case "Share Data":
				try
				{
					string zipPath = FileManager.ExportBackupForSharing();
					await Share.RequestAsync(new ShareFileRequest
					{
						Title = "SmartBudget Backup",
						File = new ShareFile(zipPath)
					});
				}
				catch (Exception ex)
				{
					await DisplayAlert("Error", ex.Message, "OK");
				}
				break;

			case "Reset All Data":
				bool confirm = await DisplayAlert("WARNING",
					"This will delete ALL data including users! Continue?", "Yes", "No");
				if (confirm)
				{
					string typeReset = await DisplayPromptAsync("Confirm Reset",
						"Type RESET to confirm:", "Confirm", "Cancel");
					if (typeReset == "RESET")
					{
						FileManager.ResetAllData();
						await DisplayAlert("Done", "All data has been reset.", "OK");
						_authManager.Logout();
						await Shell.Current.GoToAsync("//login");
					}
				}
				break;

			case "Logout":
				_authManager.Logout();
				await Shell.Current.GoToAsync("//login");
				break;
		}
	}
}
