using HtmlAgilityPack;
using System.Diagnostics;
using System.Net.Http;
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
    private string authorURL;
    string reviewURL;
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
        reviewURL = anchorNode != null ? anchorNode.GetAttributeValue("href", string.Empty) : string.Empty;
        string reviewHtmlText = ReadWebsite(reviewURL);
        //Debug.WriteLine(reviewURL);

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
            // Handle Genre
            var genreField = hubFieldsDiv.SelectSingleNode(".//div[@class='gp-hub-field'][span[text()='Genre:']]");
            if (genreField != null)
            {
                var genreList = genreField.SelectSingleNode(".//span[@class='gp-hub-field-list']");
                if (genreList != null)
                {
                    var genreLinks = genreList.Descendants("a")
                                              .Select(a => new
                                              {
                                                  Text = a.InnerText.Trim(),
                                                  Url = a.GetAttributeValue("href", string.Empty)
                                              }).ToList();

                    // Create a stack layout for Genre
                    var genreStackLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 10,
                    };

                    // Add each genre as a label with a TapGestureRecognizer
                    foreach (var genre in genreLinks)
                    {
                        var genreSpan = new Span
                        {
                            Text = genre.Text,
                            TextColor = Colors.DarkGray,
                            BackgroundColor = Color.FromArgb("#edeef2"),
                        };

                        var genreLabel = new Label
                        {
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalTextAlignment = TextAlignment.Start,
                            FontSize = 14,
                            LineBreakMode = LineBreakMode.NoWrap,
                            FormattedText = new FormattedString()
                        };

                        genreLabel.FormattedText.Spans.Add(genreSpan);

                        var genreTapGesture = new TapGestureRecognizer();
                        //genreTapGesture.Tapped += (s, e) => Launcher.OpenAsync(genre.Url);
                        genreTapGesture.Tapped += (s, e) => OnTagTapped(genre.Url);
                        genreLabel.GestureRecognizers.Add(genreTapGesture);

                        genreStackLayout.Children.Add(genreLabel);
                    }

                    GenreStackLayout.Children.Add(genreStackLayout);
                }
            }

            // Handle Cast
            var castField = hubFieldsDiv.SelectSingleNode(".//div[@class='gp-hub-field'][span[text()='Cast:']]");
            if (castField != null)
            {
                var castList = castField.SelectSingleNode(".//span[@class='gp-hub-field-list']");
                if (castList != null)
                {
                    var castLinks = castList.Descendants("a")
                                            .Select(a => new
                                            {
                                                Text = a.InnerText.Trim(),
                                                Url = a.GetAttributeValue("href", string.Empty)
                                            }).ToList();

                    // Create a stack layout for Cast
                    var castStackLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Spacing = 10,
                    };

                    // Add each cast as a label with a TapGestureRecognizer
                    foreach (var cast in castLinks)
                    {
                        var castSpan = new Span
                        {
                            Text = cast.Text,
                            TextColor = Colors.DarkGray,
                            BackgroundColor = Color.FromArgb("#edeef2"),
                        };

                        var castLabel = new Label
                        {
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalTextAlignment = TextAlignment.Start,
                            FontSize = 14,
                            LineBreakMode = LineBreakMode.NoWrap,
                            FormattedText = new FormattedString()
                        };

                        castLabel.FormattedText.Spans.Add(castSpan);

                        var castTapGesture = new TapGestureRecognizer();
                        //castTapGesture.Tapped += (s, e) => Launcher.OpenAsync(cast.Url);
                        castTapGesture.Tapped += (s, e) => OnTagTapped(cast.Url);
                        castLabel.GestureRecognizers.Add(castTapGesture);

                        castStackLayout.Children.Add(castLabel);
                    }

                    CastStackLayout.Children.Add(castStackLayout);
                }
            }

            // Handle Director
            var directorField = hubFieldsDiv.SelectSingleNode(".//div[@class='gp-hub-field'][span[text()='Director:']]");
            if (directorField != null)
            {
                var directorList = directorField.SelectSingleNode(".//span[@class='gp-hub-field-list']");
                if (directorList != null)
                {
                    var directorLink = directorList.Descendants("a").FirstOrDefault();
                    if (directorLink != null)
                    {
                        var directorText = directorLink.InnerText.Trim();
                        var directorUrl = directorLink.GetAttributeValue("href", string.Empty);

                        // Create a label for Director with a TapGestureRecognizer
                        var directorSpan = new Span
                        {
                            Text = directorText,
                            TextColor = Colors.DarkGray, 
                            BackgroundColor = Color.FromArgb("#edeef2"), 
                        };

                        var directorLabel = new Label
                        {
                            VerticalTextAlignment = TextAlignment.Center,
                            HorizontalTextAlignment = TextAlignment.Start,
                            FontSize = 14,
                            LineBreakMode = LineBreakMode.NoWrap,
                            FormattedText = new FormattedString()
                        };

                        directorLabel.FormattedText.Spans.Add(directorSpan);

                        var directorTapGesture = new TapGestureRecognizer();
                        //directorTapGesture.Tapped += (s, e) => Launcher.OpenAsync(directorUrl);
                        directorTapGesture.Tapped += (s, e) => OnTagTapped(directorUrl);
                        directorLabel.GestureRecognizers.Add(directorTapGesture);

                        DirectorStackLayout.Children.Add(directorLabel);
                    }
                }
            }
        }
        #endregion


    }

    public string ReadWebsite(string webpage)
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

        var filmReviewContent = new FilmReviewContent(reviewURL);
        FilmReviewContainer.Content = filmReviewContent.Content;
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

    private void BtnVideos_Clicked(object sender, EventArgs e)
    {

    }

    private void AuthorTapped(object sender, TappedEventArgs e)
    {
        Debug.WriteLine(authorURL);
    }

    private void OnTagTapped(string tagUrl)
    {
        Debug.WriteLine(tagUrl);
    }
}