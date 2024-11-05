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

    private void BtnFilmPage_Clicked(object sender, EventArgs e)
    {
        Detail = new NavigationPage(new FilmPage(""));
        IsPresented = false;
    }
}
