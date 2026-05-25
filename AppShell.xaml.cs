using SmartBudgetMobile.Pages;

namespace SmartBudgetMobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("dashboard", typeof(DashboardPage));
	}
}
