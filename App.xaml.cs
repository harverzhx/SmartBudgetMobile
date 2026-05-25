using SmartBudgetMobile.Managers;

namespace SmartBudgetMobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		FileManager.Initialize();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
