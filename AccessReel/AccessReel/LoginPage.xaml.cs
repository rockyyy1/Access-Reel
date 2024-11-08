namespace AccessReel;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

    private void SignInBtn_Clicked(object sender, EventArgs e)
    {
        //NEED API
    }

    private async void RegisterBtn_Clicked(object sender, EventArgs e)
    {
        string url = "https://accessreel.com/wp/wp-login.php?action=register";

        // Launch the URL
        await Launcher.OpenAsync(url);
    }

    private void ForgotPasswordBtn_Clicked(object sender, EventArgs e)
    {
        //NEED API
    }


}