namespace AccessReel;
/// <summary>
/// This page is for individual films
/// </summary>
public partial class FilmPage : ContentPage
{
	public FilmPage()
	{
		InitializeComponent();
	}

    public FilmPage(string a = "")
    {
        InitializeComponent();
        

    }

    private void BtnOverview_Clicked(object sender, EventArgs e)
    {
        FilmOverview.IsVisible = true;
        FilmReview.IsVisible = false;
    }

    private void BtnReview_Clicked(object sender, EventArgs e)
    {
        FilmOverview.IsVisible = false;
        FilmReview.IsVisible = true;
    }
}