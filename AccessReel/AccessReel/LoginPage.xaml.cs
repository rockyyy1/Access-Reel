namespace AccessReel;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}
    //SIGN IN
    private void SignInBtn_Clicked(object sender, EventArgs e)
    {
        //NEED API?
    }
    // REGISTER
    private async void RegisterBtn_Clicked(object sender, EventArgs e)
    {
        string url = "https://accessreel.com/wp/wp-login.php?action=register";

        // Launch the URL
        await Launcher.OpenAsync(url);
    }
    // FORGOT PASSWORD 
    private async void ForgotPasswordBtn_Clicked(object sender, EventArgs e)
    {
        string url = "https://accessreel.com/my-account/lost-password/";

        // Launch the URL
        await Launcher.OpenAsync(url);
    }


}