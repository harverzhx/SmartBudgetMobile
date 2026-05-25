using Microsoft.Extensions.Logging;
using SmartBudgetMobile.Managers;

namespace SmartBudgetMobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<AuthenticationManager>();
		builder.Services.AddSingleton<ExpenseManager>();
		builder.Services.AddSingleton<BudgetManager>();
		builder.Services.AddSingleton<BillManager>();
		builder.Services.AddSingleton<SavingsManager>();
		builder.Services.AddSingleton<IncomeManager>();
		builder.Services.AddSingleton<NotificationManager>();
		builder.Services.AddSingleton<ExpenseTemplateManager>();

		builder.Services.AddTransient<Pages.LoginPage>();
		builder.Services.AddTransient<Pages.DashboardPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
