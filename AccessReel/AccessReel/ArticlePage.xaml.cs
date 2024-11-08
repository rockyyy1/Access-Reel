using HtmlAgilityPack;
using System.Diagnostics;
using System.Net;
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
        string htmlText = ReadWebsite(webpage);
        HtmlDocument document = new HtmlDocument();
        document.LoadHtml(htmlText);

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
            string imageUrl = imageNode.GetAttributeValue("src", string.Empty);

            // Create an Image control and set its source
            banner.Source = ImageSource.FromUri(new Uri(imageUrl));

            #endregion

            #region PARAGRAPHS & IMAGES
            var paragraphs = document.DocumentNode.SelectNodes("//div[@class='gp-entry-text']/p");

            foreach (var paragraph in paragraphs)
            {
                var imageNodes = paragraph.SelectSingleNode(".//img");

                if (imageNodes != null)
                {
                    var imageUrls = imageNodes.GetAttributeValue("src", string.Empty);
                    contentStackLayout.Children.Add(new Image { Source = ImageSource.FromUri(new Uri(imageUrls)) });

                    // Remove the image node from the paragraph
                    imageNodes.Remove();
                }

                var text = paragraph.InnerHtml.Trim();
                text = Regex.Replace(text, "<(span|a|em|wbr).*?>|</(span|a|em)>", string.Empty);

                if (!string.IsNullOrEmpty(text))
                {
                    //text = HtmlEntity.DeEntitize(text);

                    contentStackLayout.Children.Add(new Label
                    {
                        Text = text + "\n",
                        FontSize = 14,
                        TextType = TextType.Html
                    });
                }
            }

            #endregion
        }
        #endregion
        
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

            contentStackLayout.Children.Add(VideostackLayout);


        }
        #endregion
    }

    //Clicking the button will navigate to AccessReel Author Page (ListPage)
    // https://accessreel.com/author/accessreel/"
    private void Button_Clicked(object sender, EventArgs e)
    {
        // DO LATER
    }
}