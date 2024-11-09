using HtmlAgilityPack;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;

namespace AccessReel;

public partial class FilmReviewContent : ContentPage
{
	public FilmReviewContent()
	{
		InitializeComponent();
	}
    string authorURL;
	public FilmReviewContent(string reviewUrl)
	{
        InitializeComponent();
        FilmPage page = new FilmPage();

        string reviewHtmlText = page.ReadWebsite(reviewUrl);
        Debug.WriteLine(reviewUrl);

        HtmlDocument reviewDocument = new HtmlDocument();
        reviewDocument.LoadHtml(reviewHtmlText);

        // FILM REVIEW CONTENT:
        #region REVIEWS
        var paragraphNodes = reviewDocument.DocumentNode.SelectNodes("//div[@class='gp-entry-text']//p");
        if (paragraphNodes != null)
        {
            StringBuilder reviewParagraphs = new StringBuilder();

            // Loop through each <p> tag and extract its text
            foreach (var paragraph in paragraphNodes)
            {
                string p = paragraph.InnerText.Trim() + "\n";
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
        //Debug.WriteLine(authorName);
        //Debug.WriteLine(authorURL);
        //Debug.WriteLine(authorImageUrl);
        LblAuthor.Text = authorName;
        ImageAuthor.Source = authorImageUrl;

        #endregion

        #region TAGS
        // Extract all <a> tags inside <div class='gp-entry-tags'>
        var tagNodes = reviewDocument.DocumentNode.SelectNodes("//div[@class='gp-entry-tags']//a");

        if (tagNodes != null)
        {
            // Create a horizontal StackLayout to hold the tags
            var horizontalStackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal, // Arrange tags horizontally
                Spacing = 10, // Space between the tags
                VerticalOptions = LayoutOptions.Center
            };

            foreach (var tagNode in tagNodes)
            {
                string tagName = tagNode.InnerText.Trim();
                string tagUrl = tagNode.GetAttributeValue("href", string.Empty);

                var tagSpan = new Span
                {
                    Text = tagName,
                    TextColor = Colors.DarkGray, // Dark grey
                    BackgroundColor = Color.FromArgb("#edeef2") // Light grey 
                };

                // Create a Label to hold the formatted text
                var tagLabel = new Label
                {
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontSize = 14,
                };

                // Create a FormattedString and add the Span to it
                var formattedString = new FormattedString();
                formattedString.Spans.Add(tagSpan);

                // Set the formatted string to the Label
                tagLabel.FormattedText = formattedString;

                // Create a TapGestureRecognizer for each tag
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) => OnTagTapped(tagUrl); //see method
                tagLabel.GestureRecognizers.Add(tapGestureRecognizer);

                // Add the Label to the horizontal StackLayout
                horizontalStackLayout.Children.Add(tagLabel);
            }

            TagsStackLayout.Children.Add(horizontalStackLayout);
        }

        #endregion
    }

    private async void AuthorTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label)
        {
            // Access the DataContext of the Label
            var article = (Posts)label.BindingContext;

            // Debug
            //Debug.WriteLine(article.Author);
            //Debug.WriteLine(article.AuthorUrl);

            //create new listpage:
            NavigationPage authorListPage = new NavigationPage(new ListPage("Author", article.Author));
            await Navigation.PushAsync(authorListPage);

        }
    }

    private void OnTagTapped(string tagUrl)
    {
        Debug.WriteLine(tagUrl);
    }
}