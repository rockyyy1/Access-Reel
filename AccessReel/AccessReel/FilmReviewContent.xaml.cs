using HtmlAgilityPack;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;

namespace AccessReel;

public partial class FilmReviewContent : ContentPage
{
    public Action<string, string> ActionOnTagTapped;
    public Action<string, string> ActionOnAuthorTapped;

    public FilmReviewContent()
	{
		InitializeComponent();
	}
    Review review = new Review();
    public FilmReviewContent(string reviewUrl)
    {
        InitializeComponent();
        FilmPage page = new FilmPage();

        string reviewHtmlText = page.ReadWebsite(reviewUrl);
        //Debug.WriteLine(reviewUrl);

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
            review.Description = reviewParagraphs.ToString();
            LblReview.Text = reviewParagraphs.ToString();
        }

        // author details
        var imgNode = reviewDocument.DocumentNode.SelectSingleNode("//div[@class='gp-author-info']//img");
        string authorImageUrl = imgNode.GetAttributeValue("src", string.Empty);

        // Extract the author's name and URL from the <a> tag
        var authorNode = reviewDocument.DocumentNode.SelectSingleNode("//div[@class='gp-author-name']//a");
        review.Author = authorNode.InnerText;
        review.AuthorUrl = authorNode.GetAttributeValue("href", string.Empty);
        //Debug.WriteLine(authorName);
        //Debug.WriteLine(authorURL);
        //Debug.WriteLine(authorImageUrl);
        LblAuthor.Text = review.Author;
        ImageAuthor.Source = authorImageUrl;

        // review score
        var ratingNode = reviewDocument.DocumentNode.SelectSingleNode("//div[@class='gp-rating-inner']");

        if (ratingNode != null)
        {
            string ratingText = ratingNode.InnerText.Trim();
            review.ReviewScore = ratingText;
        }
            #endregion

        #region TAGS
        var tagNodes = reviewDocument.DocumentNode.SelectNodes("//div[@class='gp-entry-tags']//a");

        if (tagNodes != null)
        {
            // horizontal StackLayout to hold the tags
            var horizontalStackLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 10,
                VerticalOptions = LayoutOptions.Center
            };

            foreach (var tagNode in tagNodes)
            {
                string tagName = tagNode.InnerText.Trim();
                tagName = HtmlEntity.DeEntitize(tagName);
                string tagUrl = tagNode.GetAttributeValue("href", string.Empty);

                // Label to hold the formatted text
                var tagLabel = new Label
                {
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    FontSize = 14,
                    BackgroundColor = Colors.LightGray,
                    TextColor = Colors.Black
                };

                tagLabel.Text = tagName;

                // Create a TapGestureRecognizer for each tag
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += (s, e) => OnTagTapped(tagUrl);
                tagLabel.GestureRecognizers.Add(tapGestureRecognizer);

                // Add the Label to the horizontal StackLayout
                horizontalStackLayout.Children.Add(tagLabel);
            }

            TagsStackLayout.Children.Add(horizontalStackLayout);
        }
        #endregion
    }
    //THIS DOESNT WORK :(
    public async void AuthorTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label)
        {
            ListPage page = new ListPage("Author", authorurl: review.AuthorUrl);
            ActionOnAuthorTapped?.Invoke("Author", review.AuthorUrl);
        }
    }

    public async void OnTagTapped(string tagUrl)
    {
        ListPage page = new ListPage("Tags", tagUrl);
        ActionOnTagTapped.Invoke("Tags", tagUrl);
    }
}