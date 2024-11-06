using HtmlAgilityPack;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace AccessReel;
/// <summary>
/// This page is for individual films e.g https://accessreel.com/venom-the-last-dance/
/// </summary>
public partial class FilmPage : ContentPage
{
    string text;
    private Review film;
    private string authorURL;
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

        #region VIDEO
        // Just getthing the snapshot of video for now.
        // Getting the Video is not possible from what i can tell - there is only a href that links to dead videos e.g https://vimeo.com/989216189 (Joker film)
        // The site only updates the node to a proper iframe when the user clicks play - otherwise it's not there - not sure how to fix

        // I managed to find a workaround by going to the review article e.g - https://accessreel.com/hellboy-the-crooked-man/hellboy-the-crooked-man-review/ which usually has trailers

        var anchorNode = document.DocumentNode.SelectSingleNode("//li/a[@title='Review']");
        string ReviewURL = anchorNode != null ? anchorNode.GetAttributeValue("href", string.Empty) : string.Empty;
        string reviewHtmlText = ReadWebsite(ReviewURL);
        //Debug.WriteLine(ReviewURL);

        HtmlDocument reviewDocument = new HtmlDocument();
        reviewDocument.LoadHtml(reviewHtmlText);
        var iframeNode = reviewDocument.DocumentNode.SelectSingleNode("//iframe");
        if (iframeNode != null)
        {
            // Extract the iframe HTML
            var iframeHtml = iframeNode.OuterHtml;

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
            /*        // getting snapshot of video instead
                    HtmlNode header = document.DocumentNode.SelectSingleNode("//header[contains(@class, 'gp-page-header') and contains(@class, 'gp-has-video')]");

                    if (header != null)
                    {
                        string style = header.GetAttributeValue("style", string.Empty);
                        //Debug.WriteLine(style);
                        var start = style.IndexOf("url(") + 4; 
                        var end = style.IndexOf(")", start);
                        var imageUrl = style.Substring(start, end - start).Trim('\"');

                        // Create an Image 
                        var image = new Image
                        {
                            Source = imageUrl
                        };

                        videoStackLayout.Children.Add(image);
                    }*/


            #endregion 

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

        if (film.MemberReviewScore == "")
        {
            AverageUserRating.Text = "0";
        }
        else
        {
            AverageUserRating.Text = film.MemberReviewScore;
        }



        #endregion

        #region VOTING SYSTEM / SUBMIT BUTTON
        var numberUserVotesNode = document.DocumentNode.SelectSingleNode("//span[@class='gp-count']");
        int numberUserVotes = numberUserVotesNode != null ? int.Parse(numberUserVotesNode.InnerText) : 0;

        // Determine ("Vote" or "Votes")
        string voteSuffix = numberUserVotes == 1 ? " Vote" : " Votes";

        // Set the text
        NumberUserVotes.Text = numberUserVotes.ToString() + voteSuffix;
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

        #region REVIEWS
        var paragraphNodes = reviewDocument.DocumentNode.SelectNodes("//div[@class='gp-entry-text']//p");
        if (paragraphNodes != null)
        {
            StringBuilder reviewParagraphs = new StringBuilder();

            // Loop through each <p> tag and extract its text
            foreach (var paragraph in paragraphNodes)
            {
                string p = paragraph.InnerText.Trim();
                p = HtmlEntity.DeEntitize(p);
                reviewParagraphs.AppendLine(p);
            }
            LblReview.Text = reviewParagraphs.ToString();
        }

        // author details
        var imgNode = reviewDocument.DocumentNode.SelectSingleNode("//div[@class='gp-author-info']//img");
        string authorImageUrl = imgNode.GetAttributeValue("src", string.Empty);

        // Extract the author's name and URL from the <a> tag
        var authorNode = reviewDocument.DocumentNode.SelectSingleNode("//div[@class='gp-author-name']//a");
        string authorName = authorNode.InnerText.Trim();
        authorURL = authorNode.GetAttributeValue("href", string.Empty);
        Debug.WriteLine(authorName);
        Debug.WriteLine(authorURL);
        Debug.WriteLine(authorImageUrl);
        LblAuthor.Text = authorName;
        ImageAuthor.Source = authorImageUrl;

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

    private void FollowBtn_Clicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        string currentText = button.Text;

        if (currentText.Contains("Follow +"))
        {
            button.Text = "Unfollow +";
        }
        else
        {
            button.Text = "Follow +";
        }
    }

    private void AuthorTapped(object sender, TappedEventArgs e)
    {
        Debug.WriteLine(authorURL);
    }
}