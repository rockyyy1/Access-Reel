namespace AccessReel;

public partial class FlyoutMenu : FlyoutPage
{
	public FlyoutMenu()
	{
		InitializeComponent();
	}

    private void BtnListPage_Clicked(object sender, EventArgs e)
    {
        Detail = new NavigationPage(new ListPage());
        IsPresented = false;
    }

    private void BtnMainPage_Clicked(object sender, EventArgs e)
    {
        Detail = new NavigationPage(new MainPage());
        IsPresented = false;
    }

    private void BtnCloseMenu_Clicked(object sender, EventArgs e)
    {
        IsPresented = false;
    }
}