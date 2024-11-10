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

    private string? tagurl;

    public ListPage() //Used for xaml to prevent error, might be removed later
    {
        InitializeComponent();
    }

    //FILMS, REVIEWS, AUTHORS AND TAGS HAVE RED RATINGS IN THEIR LISTS
    // USE TEMPLATE DTMovieArticle

    // NEWS, INTERVIEWS AND TAGS (GENRE, CASTS, DIRECTORS) DO NOT HAVE RATINGS
    // USE TEMPLATE DTArticle


    // TAGS ARE OPENED WHEN USER CLICKS ON THE THINGS E.G https://accessreel.com/saturday-night/ IN THE BOTTOM RIGHT.
    // TAGS MIGHT BE A BIT TRICKY AS THEY HAVE DIFFERENT URL paths 'genre' 'cast' and 'director'
    // https://accessreel.com/genre/comedy/
    //https://accessreel.com/cast/cooper-hoffman/
    //https://accessreel.com/director/jason-reitman/
    public ListPage(string pageType = "", string? tagurl = null)
    {
        InitializeComponent();

        this.tagurl = tagurl;

        if (tagurl != null)
        {
            // GetTagInfo(tagurl);
        }

        if (pageType == "Author" /*|| pageType == "Tags"*/)
        {
            Title.Text = pageType;
        }

        else if (pageType == "News" || pageType == "Interviews" || pageType == "Films"
            || pageType == "Reviews" || pageType == "Tags")

        {
            Title.Text = pageType;
            LoadData(pageType);
        }
    }

    private void GetTagInfo(string url, out string group, out string title)
    {
        //int index1 = url.IndexOf(".com/") + 4;
        //string group = url.Substring(index1, url.Length - 1);

        List<string> items = url.Split("/").ToList();

        group = items[3] + "/";
        title = items[4];



        Debug.WriteLine("INDEX - " + items[2]);
    }

    // function loads data from all pages
    private void LoadDataOnAllPages(string pageType, string _url = "")
    {
        List<Posts> newsList = new List<Posts>();

        int page = 1;

        string group = pageType == "News" || pageType == "Interviews" ? "categories/" : "hubs/";
        if (this.tagurl != null)
        {
            string taggroup;
            this.GetTagInfo(this.tagurl, out taggroup, out pageType);

            group = taggroup;
        }

        while (true)
        {
            var url = "https://accessreel.com/" + group + pageType + "/page/" + (page > 0 ? page.ToString() : "");
            var web = new HtmlWeb();
            var document = web.Load(url);

            var parentContainer = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'gp-blog-wrapper gp-blog-standard')]");
            if (parentContainer == null)
            {
                break;
            }
            var nodes = parentContainer.SelectNodes(".//section[contains(@class, 'gp-post-item')]");
            if (nodes != null)
            {

                foreach (var node in nodes)
                {
                    var post = new Posts();
                    if (pageType == "Reviews" || pageType == "Films")
                    {
                        post = new Review();
                    }


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

                    if (linkNode == null)
                    {
                        linkNode = node.SelectSingleNode(".//div[@class='gp-post-thumbnail gp-loop-featured']/div/a");
                    }
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

                    if (post.GetType() == typeof(Review))
                    {
                        var review = (Review)post;

                        var reviewScoreNode = node.SelectSingleNode(".//div[@class='gp-rating-inner']");
                        string reviewScore = reviewScoreNode?.InnerText.Trim();
                        review.ReviewScore = reviewScore;
                    }

                    newsList.Add(post);

                    page += 1;
                }
            }
        }
        if (group == "categories/" || this.tagurl != null)
        {
            CVArticles.ItemTemplate = DTArticle;
            CVArticles.ItemsSource = newsList;
        }
        else if (group == "hubs/")
        {
            CVArticles.ItemTemplate = DTMovieArticle;
            CVArticles.ItemsSource = newsList;
        }
    }

    private async void LoadData(string pageType)
    {
        LoadDataOnAllPages(pageType);

        #region ROCKY'S CODE - BACKUP
        // sorry i commented out your standard code, just testing getting data from all the pages at once :)

        //Debug.WriteLine("You have opened the News flyout");
        //List<Posts> newsList = new List<Posts>();

        //var url = "https://accessreel.com/categories/" + pageType;
        //var web = new HtmlWeb();
        //var document = web.Load(url);

        //var parentContainer = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'gp-blog-wrapper gp-blog-standard')]");
        //var nodes = parentContainer.SelectNodes(".//section[contains(@class, 'gp-post-item')]");
        //if (nodes != null)
        //{
        //    foreach (var node in nodes)
        //    {
        //        var post = new Posts();

        //        // Extract Title
        //        var titleNode = node.SelectSingleNode(".//h2[@class='gp-loop-title']/a");
        //        string title = titleNode.InnerText.Trim();
        //        title = HtmlEntity.DeEntitize(title);
        //        post.Title = title;

        //        // Extract Paragraph
        //        var paragraphNode = node.SelectSingleNode(".//div[@class='gp-loop-text']/p");
        //        string para = paragraphNode.InnerText.Trim();
        //        para = HtmlEntity.DeEntitize(para);
        //        post.Description = para;

        //        // Extract the main link
        //        var linkNode = node.SelectSingleNode(".//div[@class='gp-image-align-left']/a");
        //        post.Url = linkNode?.GetAttributeValue("href", string.Empty);

        //        var imageNode = linkNode?.SelectSingleNode(".//img");
        //        post.Image = imageNode?.GetAttributeValue("src", string.Empty);

        //        // Extract Author
        //        var authorNode = node.SelectSingleNode(".//span[@class='gp-post-meta gp-meta-author']/a");
        //        post.Author = authorNode?.InnerText.Trim();
        //        post.AuthorUrl = authorNode?.GetAttributeValue("href", string.Empty);

        //        // Extract Date Published
        //        var dateNode = node.SelectSingleNode(".//time[@class='gp-post-meta gp-meta-date']");
        //        var dateText = dateNode?.InnerText.Trim();
        //        if (DateTime.TryParse(dateText, out var parsedDate))
        //        {
        //            post.Date = parsedDate;
        //        }
        //        /*  Debug.WriteLine(post.Title);
        //          Debug.WriteLine(post.Description);
        //          Debug.WriteLine(post.Author);
        //          Debug.WriteLine(post.FormattedDate);
        //          Debug.WriteLine(post.Url);
        //          Debug.WriteLine(post.Image);*/


        //        newsList.Add(post);
        //    }
        //}
        //CVArticles.ItemTemplate = DTArticle;
        //CVArticles.ItemsSource = newsList;
        #endregion
    }

    private void BtnFlyoutMenu_Clicked(object sender, EventArgs e)
    {
        if (Application.Current.MainPage is FlyoutMenu flyoutPage)
        {
            flyoutPage.IsPresented = true;
        }
    }

    // When the user taps on a Article Title/Image, brings them to an Article Page
    private async void ItemTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label || sender is Image image)
        {
            // Access the DataContext of the Label
            var item = (Posts)((VisualElement)sender).BindingContext;

            //Debug.WriteLine(item.Url);

            ArticlePage newArticle = new ArticlePage(item.Url);

            await Navigation.PushAsync(newArticle);
        }
    }

    // When the user taps on an user it should bring up the Author Page
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
        if (value is string text)
        {
            if (text.Length == 2)
            {
                return new Rect(0.22, 0.15, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            }
            else
            {
                return new Rect(0.28, 0.15, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            }
        }
        return null; //new Rect(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);

    }

    public Object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}