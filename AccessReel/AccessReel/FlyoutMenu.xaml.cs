namespace AccessReel;

public partial class FlyoutMenu : FlyoutPage
{
	public FlyoutMenu()
	{
		InitializeComponent();

        Application.Current.UserAppTheme = AppTheme.Light;
    }

    private void BtnCloseMenu_Clicked(object sender, EventArgs e)
    {
        IsPresented = false;
    }

    private void BtnHomePage_Clicked(object sender, EventArgs e)
    {
        Detail = new NavigationPage(new MainPage());
        IsPresented = false;
    }

    private void BtnNewsPage_Clicked(object sender, EventArgs e)
    {
        Detail = new NavigationPage(new ListPage("News"));
        IsPresented = false;
    }

    private void BtnReviewPage_Clicked(object sender, EventArgs e)
    {
        Detail = new NavigationPage(new ListPage("Reviews"));
        IsPresented = false;
    }

    private void BtnFilmsPage_Clicked(object sender, EventArgs e)
    {
        Detail = new NavigationPage(new ListPage("Films"));
        IsPresented = false;
    }

    private void BtnTrailers_Clicked(object sender, EventArgs e)
    {

        Detail = new NavigationPage(new TrailerListPage());
        IsPresented = false;
    }

    private void BtnInterviews_Clicked(object sender, EventArgs e)
    {
        Detail = new NavigationPage(new ListPage("Interviews"));
        IsPresented = false;
    }

    private void BtnLogin_Clicked(object sender, EventArgs e)
    {
        Detail = new NavigationPage(new LoginPage());
        IsPresented = false;
    }

    // Link to website for new registration
    private async void BtnRegister_Clicked(object sender, EventArgs e)
    {
        string url = "https://accessreel.com/wp/wp-login.php?action=register";

        // Launch the URL
        await Launcher.OpenAsync(url);
    }

    private void BtnFindTheaters_Clicked(object sender, EventArgs e)
    {
        Detail = new NavigationPage(new FindNearby());
        IsPresented = false;
    }

    private void AccessReelSearchbar_SearchButtonPressed(object sender, EventArgs e)
    {
        string url = "https://accessreel.com/?s=" + AccessReelSearchbar.Text;

        url = url.Replace(" ", "+");

        Detail = new NavigationPage(new ListPage("Search", url));
        IsPresented = false;


    }
}
