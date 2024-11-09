using HtmlAgilityPack;
using System.Diagnostics;
using static System.Net.WebRequestMethods;

namespace AccessReel;

public partial class TrailerPage : ContentPage
{
    string text;

    public TrailerPage()
	{
		InitializeComponent();
	}

    public string ReadWebsite(string webpage)
    {
        var web = new HtmlWeb();
        text = web.Load(webpage).Text;
        return text;
    }

    public TrailerPage(Review film)
	{
        InitializeComponent();
        Webscraping scrape = new Webscraping();
        string htmlText = ReadWebsite(film.Url);
        HtmlDocument document = new HtmlDocument();
        document.LoadHtml(htmlText);
        BindingContext = film;

        #region VIDEO
        var iframeNode = document.DocumentNode.SelectSingleNode("//iframe");
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

        #endregion

        #region POSTER & SYNOPSIS
        var imgNode = document.DocumentNode.SelectSingleNode("//img[@class='trailer_thumbnail']");
        if (imgNode != null)
        {
            string imageUrl = imgNode.GetAttributeValue("src", null);
            ImagePoster.Source = imageUrl;
        }

        var paragraphNode = document.DocumentNode.SelectSingleNode("//div[@class='gp-entry-text']/p");
        string description = string.Empty;

        if (paragraphNode != null)
        {
            description = HtmlEntity.DeEntitize(paragraphNode.InnerText);
            film.Description = description;
            LblDesc.Text = film.Description;
        }
        #endregion

        #region RELEASE DATES
        var AUReleaseNode = document.DocumentNode.SelectSingleNode("//aside[@class='trailer-sidebar']//div[contains(text(), 'In Theaters (AU):')]");
        string AUreleaseDateText = string.Empty;

        if (AUReleaseNode != null)
        {
            AUreleaseDateText = HtmlEntity.DeEntitize(AUReleaseNode.InnerText.Replace("In Theaters (AU):", "").Trim());
            AUreleaseDate.Text = $"In Theaters (AU): {AUreleaseDateText}";
        }
        var USReleaseNode = document.DocumentNode.SelectSingleNode("//aside[@class='trailer-sidebar']//div[contains(text(), 'In Theaters (USA):')]");
        string USreleaseDateText = string.Empty;

        if (USReleaseNode != null)
        {
            USreleaseDateText = HtmlEntity.DeEntitize(USReleaseNode.InnerText.Replace("In Theaters (USA):", "").Trim());
            USreleaseDate.Text = $"In Theaters (USA): {USreleaseDateText}";
        }
        #endregion

        #region GENRE/CAST/DIRECTOR/STUDIO/HOMEPAGE
        var trailerInfoTable = document.DocumentNode.SelectSingleNode("//table[@id='trailerInfoTable']");

        if (trailerInfoTable != null)
        {
            // Handle Genre
            var genreField = trailerInfoTable.SelectSingleNode(".//tr[td[text()='Genre']]");
            if (genreField != null)
            {
                var genreList = genreField.SelectSingleNode(".//th");
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
                        genreTapGesture.Tapped += (s, e) => OnTagTapped(genre.Url);
                        genreLabel.GestureRecognizers.Add(genreTapGesture);

                        genreStackLayout.Children.Add(genreLabel);
                    }

                    GenreStackLayout.Children.Add(genreStackLayout);
                }
            }

            // Handle Cast
            var castField = trailerInfoTable.SelectSingleNode(".//tr[td[text()='Cast']]");
            if (castField != null)
            {
                var castList = castField.SelectSingleNode(".//th");
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
                        castTapGesture.Tapped += (s, e) => OnTagTapped(cast.Url);
                        castLabel.GestureRecognizers.Add(castTapGesture);

                        castStackLayout.Children.Add(castLabel);
                    }

                    CastStackLayout.Children.Add(castStackLayout);
                }
            }

            // Handle Director
            var directorField = trailerInfoTable.SelectSingleNode(".//tr[td[text()='Director']]");
            if (directorField != null)
            {
                var directorList = directorField.SelectSingleNode(".//th");
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
                        directorTapGesture.Tapped += (s, e) => OnTagTapped(directorUrl);
                        directorLabel.GestureRecognizers.Add(directorTapGesture);

                        DirectorStackLayout.Children.Add(directorLabel);
                    }
                }
            }

            // Handle Studio
            var studioField = trailerInfoTable.SelectSingleNode(".//tr[td[text()='Studio']]");
            if (studioField != null)
            {
                var studioName = studioField.SelectSingleNode(".//th")?.InnerText.Trim();
                if (!string.IsNullOrEmpty(studioName))
                {
                    var studioLabel = new Label
                    {
                        Text = studioName,
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Start,
                        FontSize = 14,
                        TextColor = Colors.DarkGray,
                    };

                    StudioStackLayout.Children.Add(studioLabel);
                }
            }

            // Handle Homepage
            var homepageField = trailerInfoTable.SelectSingleNode(".//tr[td[text()='Homepage']]");
            if (homepageField != null)
            {
                var homepageLink = homepageField.SelectSingleNode(".//th/a");
                if (homepageLink != null)
                {
                    var homepageText = homepageLink.InnerText.Trim();
                    var homepageUrl = homepageLink.GetAttributeValue("href", string.Empty);

                    var homepageSpan = new Span
                    {
                        Text = homepageText,
                        TextColor = Colors.Blue,
                    };

                    var homepageLabel = new Label
                    {
                        VerticalTextAlignment = TextAlignment.Center,
                        HorizontalTextAlignment = TextAlignment.Start,
                        FontSize = 14,
                        LineBreakMode = LineBreakMode.NoWrap,
                        FormattedText = new FormattedString()
                    };

                    homepageLabel.FormattedText.Spans.Add(homepageSpan);

                    var homepageTapGesture = new TapGestureRecognizer();
                    homepageTapGesture.Tapped += (s, e) => OnTagTapped(homepageUrl);
                    homepageLabel.GestureRecognizers.Add(homepageTapGesture);

                    HomepageStackLayout.Children.Add(homepageLabel);
                }
            }
        }
        #endregion



    }
    private void OnTagTapped(string tagUrl)
    {
        Debug.WriteLine(tagUrl);
    }
}