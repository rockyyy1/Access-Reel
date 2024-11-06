using System.Collections;
using System.Diagnostics;
using System.Globalization;
using HtmlAgilityPack;
using Microsoft.Maui.Controls;

namespace AccessReel;

public partial class ListPage : ContentPage
{
    public List<Review> reviewList { get; set; }
    public List<Posts> postList { get; set; }

    public ListPage() //Used for xaml to prevent error, might be removed later
    {
        InitializeComponent();
    }

    public ListPage(string pageType = "")
	{
		InitializeComponent();
        //Test Data
        if(pageType == "Reviews" || pageType == "Films")
        {
            Title = pageType;
            //LblPageTitle.Text = pageType;
            reviewList = new List<Review>();
            Review a = new Review { Author = "Test Author", Date = DateTime.Today, Description = "Film Description", ReviewScore = "10", Title = "Film title" };
            Review b = new Review { Author = "Test Author", Date = DateTime.Today, Description = "Film Description", ReviewScore = "3", Title = "Film title" };
            reviewList.Add(a);
            reviewList.Add(b);
            CVArticles.ItemTemplate = DTMovieArticle;
            CVArticles.ItemsSource = reviewList;
        }
        else //News, Trailers and Interviews
        {
            Title = pageType;

            if (pageType == "News")
            {
                LoadNews();
            }

            /*postList = new List<Posts>();
            Posts c = new Posts { Author = "Test Author", Date = DateTime.Today, Description = "Article Body", Title = "Article title" };
            Posts d = new Posts { Author = "Test Author", Date = DateTime.Today, Description = "Article Body", Title = "Article title" };
            postList.Add(c);
            postList.Add(d);
            CVArticles.ItemTemplate = DTArticle;
            CVArticles.ItemsSource = postList;*/
        }
    }

    private void LoadNews()
    {
        //Debug.WriteLine("You have opened the News flyout");
        List<Posts> newsList = new List<Posts>();

        var url = "https://accessreel.com/categories/news/"; 
        var web = new HtmlWeb();
        var document = web.Load(url);

        var parentContainer = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'gp-blog-wrapper gp-blog-standard')]");
        var nodes = parentContainer.SelectNodes(".//section[contains(@class, 'gp-post-item')]");
        if (nodes != null)
        {
            foreach (var node in nodes)
            {
                var post = new Posts();

                // Extract Title
                var titleNode = node.SelectSingleNode(".//h2[@class='gp-loop-title']/a");
                string title = titleNode.InnerText.Trim();
                title = HtmlEntity.DeEntitize(title);
                post.Title = title;

                // Extract Paragraph
                var paragraphNode = node.SelectSingleNode(".//div[@class='gp-loop-text']/p");
                string para = paragraphNode.InnerText.Trim();
                para = HtmlEntity.DeEntitize(para);
                post.Description = para;

                // Extract the main link
                var linkNode = node.SelectSingleNode(".//div[@class='gp-image-align-left']/a");
                post.Url = linkNode?.GetAttributeValue("href", string.Empty);

                var imageNode = linkNode?.SelectSingleNode(".//img");
                post.Image = imageNode?.GetAttributeValue("src", string.Empty);

                // Extract Author
                var authorNode = node.SelectSingleNode(".//span[@class='gp-post-meta gp-meta-author']/a");
                post.Author = authorNode?.InnerText.Trim();
                post.AuthorUrl = authorNode?.GetAttributeValue("href", string.Empty);

                // Extract Date Published
                var dateNode = node.SelectSingleNode(".//time[@class='gp-post-meta gp-meta-date']");
                var dateText = dateNode?.InnerText.Trim();
                if (DateTime.TryParse(dateText, out var parsedDate))
                {
                    post.Date = parsedDate;
                }
              /*  Debug.WriteLine(post.Title);
                Debug.WriteLine(post.Description);
                Debug.WriteLine(post.Author);
                Debug.WriteLine(post.FormattedDate);
                Debug.WriteLine(post.Url);
                Debug.WriteLine(post.Image);*/


                newsList.Add(post);
            }
        }
        CVArticles.ItemTemplate = DTArticle;
        CVArticles.ItemsSource = newsList;

    }

    private void BtnFlyoutMenu_Clicked(object sender, EventArgs e)
    {
        if(Application.Current.MainPage is FlyoutMenu flyoutPage)
        {
            flyoutPage.IsPresented = true;
        }        
    }

    private void ItemTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label || sender is Image image)
        {
            // Access the DataContext of the Label
            var item = (Posts)((VisualElement)sender).BindingContext;
            Debug.WriteLine(item.Url);
        }
    }

    private void AuthorTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label)
        {
            var item = (Posts)((VisualElement)sender).BindingContext;
            Debug.WriteLine(item.AuthorUrl);
        }
    }
}



//This changes the postion of the label within the circle for the review score
public class ReviewScoreToAbsLayoutConverter : IValueConverter
{
    public Object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is string text)
        {
            if (text.Length == 2)
            {
                return new Rect(0.22, 0.25, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            }
            else
            {
                return new Rect(0.28, 0.25, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            }
        }
        return null; //new Rect(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
        
    }

    public Object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

