using HtmlAgilityPack;
using System.Diagnostics;
using System.Net;

namespace AccessReel;
/// <summary>
/// This page is for displaying individual articles e.g. news article https://accessreel.com/article/2024-cinfestoz-film-prize-finalists-announced/
/// </summary>
public partial class ArticlePage : ContentPage
{
    HtmlDocument document;
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

        #region TITLE
        // Use XPath to select the title element
        var titleNode = document.DocumentNode.SelectSingleNode("//*[(@id='gp-content')]/article/h1");
        string title = titleNode.InnerText;
        articleTitle.Text = title;
        #endregion

        #region IMAGE
        var imageNode = document.DocumentNode.SelectSingleNode("//*[(@id='gp-content')]/article/div[3]/img");

        // Get the image source
        string imageUrl = imageNode.GetAttributeValue("src", string.Empty);

        // Create an Image control and set its source
        banner.Source = ImageSource.FromUri(new Uri(imageUrl));

        #endregion

        #region PARAGRAPHS
        //class="gp-entry-text"
        var paragraphs = document.DocumentNode.SelectNodes("//div[@class='gp-entry-text']/p");

        if (paragraphs != null)
        {
            var text = string.Empty;
            foreach (var paragraph in paragraphs)
            {
                // Note to self: InnerHtml if we want to include all the html tags e.g <strong> etc && InnerText to exclude them
                // in XAML - TextType = "Html" will render the tags
                text += paragraph.InnerHtml + "\n\n";
            }
            paragraphsLbl.Text = text;
        }

        #endregion

    }

    //Clicking the button on the button will navigate to AccessReel Author Page (ListPage)
    // https://accessreel.com/author/accessreel/"
    private void Button_Clicked(object sender, EventArgs e)
    {

    }
}