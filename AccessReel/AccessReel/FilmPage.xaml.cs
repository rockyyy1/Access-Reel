using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;

namespace AccessReel;
/// <summary>
/// This page is for individual films e.g https://accessreel.com/venom-the-last-dance/
/// </summary>
public partial class FilmPage : ContentPage
{
    HtmlDocument document;
    string text;
    private Review film;
    public FilmPage()
	{
		InitializeComponent();
	}

    //Receive the url
    public FilmPage(Review film)
    {
        InitializeComponent();

        string htmlText = ReadWebsite(film.Url);
        HtmlDocument document = new HtmlDocument();
        document.LoadHtml(htmlText);
        BindingContext = film;

        #region VIDEO - DOESN'T WORK ATM
        // Getting the Video is not possible from what i can tell 
        // The site only updates the node when the user clicks play - otherwise it's not there
        // Someone else can give it go...

        var videoLinkNode = document.DocumentNode.SelectSingleNode("//a[@class='gp-play-video-button']");


        if (videoLinkNode != null)
        {
            // Extract the iframe HTML
            var iframeHtml = videoLinkNode.OuterHtml;

            // Create a WebView to render the iframe
            var webView = new WebView
            {
                Source = new HtmlWebViewSource { Html = iframeHtml },
                HeightRequest = 405,
                WidthRequest = 720
            };

            // Add the WebView to the StackLayout
            var VideostackLayout = new StackLayout
            {
                Children = { webView }
            };

            videoStackLayout.Children.Add(VideostackLayout);


        }
        #endregion - 

        #region RELEASE DATES
        var releaseInfoDivs = document.DocumentNode.SelectNodes("//div[@class='release-info']");

        if (releaseInfoDivs != null)
        {
            foreach (var releaseInfoDiv in releaseInfoDivs)
            {
                // Extract the text content of the div
                var releaseInfoText = releaseInfoDiv.InnerText;

                if (releaseInfoText.Contains("AU"))
                {
                    AUreleaseDate.Text = releaseInfoText;
                }
                else if (releaseInfoText.Contains("USA"))
                {
                    USreleaseDate.Text = releaseInfoText;
                }
            }
        }
        #endregion

        #region POSTER
        var posterNode = document.DocumentNode.SelectSingleNode("//div[@class='wpb_wrapper']//p//img");
        string src = posterNode?.GetAttributeValue("src", null);
        if (src != null)
        {
            ImagePoster.Source = src;
            //Debug.WriteLine(src);
        }
        #endregion

        #region MOVIE SYNOPSIS / PLOT
        var paragraphNode = document.DocumentNode.SelectSingleNode("//div[@class='wpb_wrapper']//p");

        // Extract the inner text
        string description = string.Empty;

        if (paragraphNode != null)
        {
            description = HtmlEntity.DeEntitize(paragraphNode.InnerText);
            film.Description = description;
            LblDesc.Text = film.Description;
        }
        #endregion

        #region AVERAGE USER RATING

        #endregion

        #region VOTING SYSTEM / SUBMIT BUTTON
        #endregion

        #region GENRE, CAST, DIRECTOR INFO
        var hubFieldsDiv = document.DocumentNode.SelectSingleNode("//div[@class='gp-hub-fields']");

        if (hubFieldsDiv != null)
        {
            // Extract Genre
            var genreField = hubFieldsDiv.SelectSingleNode(".//div[@class='gp-hub-field'][span[text()='Genre:']]");
            if (genreField != null)
            {
                var genreList = genreField.SelectSingleNode(".//span[@class='gp-hub-field-list']");
                if (genreList != null)
                {
                    var genreText = string.Join(", ", genreList.Descendants("a").Select(a => a.InnerText.Trim()));
                    LblGenre.Text = $"{genreText}";
                }
            }

            // Extract Cast
            var castField = hubFieldsDiv.SelectSingleNode(".//div[@class='gp-hub-field'][span[text()='Cast:']]");
            if (castField != null)
            {
                var castList = castField.SelectSingleNode(".//span[@class='gp-hub-field-list']");
                if (castList != null)
                {
                    var castText = string.Join(", ", castList.Descendants("a").Select(a => a.InnerText.Trim()));
                    LblCast.Text = $"{castText}";
                }
            }

            // Extract Director
            var directorField = hubFieldsDiv.SelectSingleNode(".//div[@class='gp-hub-field'][span[text()='Director:']]");
            if (directorField != null)
            {
                var directorText = directorField.SelectSingleNode(".//span[@class='gp-hub-field-list']//a").InnerText.Trim();
                LblDirector.Text = $"{directorText}";
            }
        }
        #endregion

    }

    private string ReadWebsite(string webpage)
    {
        var web = new HtmlWeb();
        text = web.Load(webpage).Text;
        return text;
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

    private void BtnVideos_Clicked(object sender, EventArgs e)
    {

    }

    private void BtnNews_Clicked(object sender, EventArgs e)
    {

    }
}