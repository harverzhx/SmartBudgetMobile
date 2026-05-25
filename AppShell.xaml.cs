using SmartBudgetMobile.Pages;

namespace SmartBudgetMobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("expenses", typeof(ExpensesPage));
		Routing.RegisterRoute("budget", typeof(BudgetPage));
		Routing.RegisterRoute("bills", typeof(BillsPage));
		Routing.RegisterRoute("savings", typeof(SavingsPage));
	}
}
