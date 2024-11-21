using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AccessReel;
/// <summary>
/// This page is for displaying individual articles e.g. news article https://accessreel.com/article/2024-cinfestoz-film-prize-finalists-announced/
/// </summary>
public partial class ArticlePage : ContentPage
{
    string text;

    public ArticlePage()
	{
		InitializeComponent();
	}

    private string ReadWebsite(string webpage)
    {
        var web = new HtmlWeb();
        text = web.Load(webpage).Text;
        return text;
    }

    public ArticlePage(string webpage = "")
    {
        InitializeComponent();

        #region SCRAPE
        string htmlText = ReadWebsite(webpage);
        HtmlDocument document = new HtmlDocument();
        document.LoadHtml(htmlText);
        #endregion

        #region REVIEW
        if (webpage.Contains("-review"))
        {
            #region TITLE
            var titleNode = document.DocumentNode.SelectSingleNode("//h1[@class='gp-entry-title']");
            if (titleNode != null)
            {
                string title = titleNode.InnerText;
                title = HtmlEntity.DeEntitize(title);
                articleTitle.Text = title;
            }
            #endregion

            #region CONTENT
            FilmReviewContainer.IsVisible = true;
            var reviewContent = new FilmReviewContent(webpage);
            reviewContent.ActionOnTagTapped += this.NavigateToTags;
            reviewContent.ActionOnAuthorTapped += this.NavigateToAuthor;
            FilmReviewContainer.Content = reviewContent.Content;
            #endregion
        }
        #endregion

        #region ARTICLE
        else
        {
            FilmReviewContainer.IsVisible = false;
            #region TITLE
            // Use XPath to select the title element
            var titleNode = document.DocumentNode.SelectSingleNode("//*[(@id='gp-content')]/article/h1");
            string title = titleNode.InnerText;
            title = HtmlEntity.DeEntitize(title);
            articleTitle.Text = title;
            #endregion

            #region BANNER
            var imageNode = document.DocumentNode.SelectSingleNode("//*[(@id='gp-content')]/article/div[3]/img");

            // Get the image source
            string imageUrl = imageNode?.GetAttributeValue("src", string.Empty);

            // Create an Image control and set its source
            if (imageUrl != null)
            {
                banner.Source = ImageSource.FromUri(new Uri(imageUrl));
            }


            #endregion

            #region PARAGRAPHS & IMAGES
            var paragraphs = document.DocumentNode.SelectNodes("//div[@class='gp-entry-text']/p");
            var sb = new StringBuilder();
            sb.Append("<html><head><style>");
            sb.Append("img { max-width: 100%; height: auto; }"); // Make images responsive
            sb.Append("a { color: blue; text-decoration: underline; }"); // Style hyperlinks

            // Style the iframe to make it fully responsive
            sb.Append("iframe { width: 100%; height: 56.25vw; max-width: 100%; border: none; }"); // 16:9 aspect ratio

            sb.Append("</style></head><body style='font-family: OpenSans-Regular, sans-serif;'>");

            foreach (var paragraph in paragraphs)
            {
                var text = paragraph.InnerHtml.Trim();

                // Remove unnecessary tags but retain iframe and hyperlinks
                text = Regex.Replace(text, "<(span|em|wbr).*?>|</(span|em)>", string.Empty);

                if (!string.IsNullOrEmpty(text))
                {
                    sb.Append("<p>");
                    sb.Append(text);
                    sb.Append("</p>");
                }
            }

            sb.Append("</body></html>");
            string fullHtmlContent = sb.ToString();

            // Create WebView to display the HTML content
            var webView = new WebView
            {
                Source = new HtmlWebViewSource
                {
                    Html = fullHtmlContent
                },
                WidthRequest = 370, // Full width of the screen
                HeightRequest = DeviceDisplay.MainDisplayInfo.Height, // Allow dynamic height
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Start
            };

            // Add the WebView to the contentStackLayout
            contentStackLayout.Children.Add(webView);

            #endregion
        }
        #endregion

        #region VIDEO
        /*var iframeNode = document.DocumentNode.SelectSingleNode("//iframe");

        if (iframeNode != null)
        {
            // Extract the iframe HTML
            var iframeHtml = iframeNode.OuterHtml;

            string css = @"
        <style>
            body, html { margin: 0; padding: 0; overflow: hidden; }
            iframe { width: 100% !important; height: 100% !important; }
        </style>";

            iframeHtml = css + iframeHtml;

            // Create a WebView to render the iframe
            var webView = new WebView
            {
                Source = new HtmlWebViewSource { Html = iframeHtml },
                HeightRequest = 255,
                WidthRequest = 400,
            };

            // Add the WebView to the StackLayout
            var VideostackLayout = new StackLayout
            {
                Children = { webView }
            };

            contentStackLayout.Children.Add(VideostackLayout);


        }*/
        #endregion
    }

    // Navigate to AccessReel AccessReel ListPage
    private async void Button_Clicked(object sender, EventArgs e)
    {
        NavigationPage authorListPage = new NavigationPage(new ListPage("Author", authorurl: "https://accessreel.com/author/accessreel"));
        await Navigation.PushAsync(authorListPage);
    }
    // Navigate to AccessReel Tags ListPage
    private async void NavigateToTags(string pageType, string? tagurl)
    {
        ListPage page = new ListPage(pageType, tagurl);

        await Navigation.PushAsync(page);
    }
    // Navigate to AccessReel Author ListPage
    private async void NavigateToAuthor(string pageType, string? tagurl)
    {
        ListPage page = new ListPage(pageType, authorurl: tagurl);

        await Navigation.PushAsync(page);
    }
}