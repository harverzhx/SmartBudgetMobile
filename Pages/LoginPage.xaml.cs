using SmartBudgetMobile.Managers;

namespace SmartBudgetMobile.Pages;

public partial class LoginPage : ContentPage
{
	private readonly AuthenticationManager _authManager;
	private bool _loginSuccessful;

	public LoginPage(AuthenticationManager authManager)
	{
		InitializeComponent();
		_authManager = authManager;
	}

	private async void OnLoginClicked(object sender, EventArgs e)
	{
		btnLogin.IsEnabled = false;
		lblError.IsVisible = false;

		var (success, message) = _authManager.Login(txtUsername.Text, txtPassword.Text);

		if (success)
		{
			_loginSuccessful = true;
			await Shell.Current.GoToAsync("//dashboard");
		}
		else
		{
			lblError.Text = message;
			lblError.IsVisible = true;
			btnLogin.IsEnabled = true;
		}
	}

	private async void OnRegisterClicked(object sender, EventArgs e)
	{
		string username = await DisplayPromptAsync("Register", "Enter username:");
		if (string.IsNullOrWhiteSpace(username)) return;

		string password = await DisplayPromptAsync("Register", "Enter password (min 6 chars):");
		if (string.IsNullOrWhiteSpace(password)) return;

		string fullName = await DisplayPromptAsync("Register", "Enter full name:");
		if (string.IsNullOrWhiteSpace(fullName)) return;

		string email = await DisplayPromptAsync("Register", "Enter email (optional):");

		var (success, message) = _authManager.Register(username, password, fullName, email ?? "");

		await DisplayAlert(success ? "Success" : "Error", message, "OK");

		if (success)
		{
			_loginSuccessful = true;
			await Shell.Current.GoToAsync("//dashboard");
		}
	}

	private async void OnForgotPasswordClicked(object sender, EventArgs e)
	{
		string username = await DisplayPromptAsync("Forgot Password", "Enter your username:");
		if (string.IsNullOrWhiteSpace(username)) return;

		string newPassword = await DisplayPromptAsync("Forgot Password", "Enter new password (min 6 chars):");
		if (string.IsNullOrWhiteSpace(newPassword)) return;

		var (success, message) = _authManager.ForgotPassword(username, newPassword);
		await DisplayAlert(success ? "Success" : "Error", message, "OK");
	}
}
