using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata;
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
    //FILMS, REVIEWS, AUTHORS AND TAGS HAVE RED RATINGS IN THEIR LISTS
    // USE TEMPLATE DTMovieArticle

    // NEWS, INTERVIEWS AND TAGS (GENRE, CASTS, DIRECTORS) DO NOT HAVE RATINGS
    // USE TEMPLATE DTArticle


    // TAGS ARE OPENED WHEN USER CLICKS ON THE THINGS E.G https://accessreel.com/saturday-night/ IN THE BOTTOM RIGHT.
    // TAGS MIGHT BE A BIT TRICKY AS THEY HAVE DIFFERENT URL paths 'genre' 'cast' and 'director' 
    // https://accessreel.com/genre/comedy/
    //https://accessreel.com/cast/cooper-hoffman/
    //https://accessreel.com/director/jason-reitman/
    // THERE IS ALSO A /tag/ paths FOUND ON REVIEWS E.G : https://accessreel.com/venom-the-last-dance/venom-the-last-dance-review/
    // https://accessreel.com/tag/alanna-ubach/
    // https://accessreel.com/tag/kelly-marcel/
    // ALL WILL USE A DTArticle ListPage, it's just how arrange the path Urls

    string Author;
    string Tag;
    string Group;
    public ListPage(string pageType = "", string? authorName = null, string? tagUrl = null)
    {
        InitializeComponent();
        Title.Text = pageType;
        if (authorName != null)
        {
            Author = authorName;
        }
        if (tagUrl != null)
        {
            Tag = ProcessUrl(tagUrl);
            Group = ExtractGroup(tagUrl);
            Title.Text = Tag;
            //Debug.WriteLine("group is: " + Group);
            //Debug.WriteLine(tagUrl);
            //Debug.WriteLine(Tag);
        }
        LoadData(pageType);
        
    }

    // function loads data from all pages
    private async Task LoadDataOnAllPages(string pageType)
    {
        List<Posts> newsList = new List<Posts>();

        int page = 1;
        int lastPage = 1;
        string group = pageType == "News" || pageType == "Interviews" ? "categories/" : "hubs/";

        if (pageType == "Author")
        {
            group = "author/";
            pageType = Author;
            Title.Text = Author;
        }

        if (pageType == "Tag")
        {
            group = Group + "/";
            pageType = Tag;
        }

        #region FIND LAST PAGE NUMBER
        var urlp = "https://accessreel.com/" + group + pageType.Replace(" ", "-");
        var webp = new HtmlWeb();
        var documentp = webp.Load(urlp);
        var pageLinks = documentp.DocumentNode.SelectNodes("//a[@class='page-numbers']");

        if (pageLinks != null && pageLinks.Count > 0)
        {
            // Get the last page number
            string lastPageUrl = pageLinks[pageLinks.Count - 1].GetAttributeValue("href", string.Empty);
            // Extract the last page number from the URL
            lastPage = int.Parse(lastPageUrl.Split('/')[^2]);
            //Debug.WriteLine($"Number of pages: {lastPage}");
        }
        else
        {
            lastPage = 1;
        }
        #endregion
        
        while (page <= lastPage)
        {
            //loop until last page number
            var url = "https://accessreel.com/" + group + pageType.Replace(" ", "-") + "/page/" + page.ToString();
            //Debug.WriteLine(url);
            var web = new HtmlWeb();
            var document = web.Load(url);

            var parentContainer = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'gp-blog-wrapper gp-blog-standard')]");
            if (parentContainer == null)
            {
                break;
            }
            var nodes = parentContainer.SelectNodes(".//section[contains(@class, 'gp-post-item')]");
            if (nodes == null || nodes.Count == 0)
            {
                break;
            }
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var post = new Posts();
                    if (pageType == "Reviews" || pageType == "Films" || pageType == Author || pageType == Tag)
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
                    string para = paragraphNode?.InnerText.Trim();
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

                    if (post.GetType() == typeof(Review))
                    {
                        var review = (Review)post;

                        var reviewScoreNode = node.SelectSingleNode(".//div[@class='gp-rating-inner']");
                        string reviewScore = reviewScoreNode?.InnerText.Trim();
                        review.ReviewScore = reviewScore;
                    }

                    newsList.Add(post);

                    page += 1;

                    if (page > lastPage)  
                    {
                        break;
                    }
                }
            }
            //Debug.WriteLine(url);
        }
        if (group == "categories/")
        {
            CVArticles.ItemTemplate = DTArticle;
            CVArticles.ItemsSource = newsList;
        }
        else if (group == "hubs/" || group == "author/" || group == Group + "/")
        {
            CVArticles.ItemTemplate = DTMovieArticle;
            CVArticles.ItemsSource = newsList;
        }
    }

    private async void LoadData(string pageType)
    {
        await LoadDataOnAllPages(pageType);

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

    public string ProcessUrl(string url)
    {
        string lastPart = url.TrimEnd('/').Split('/').Last();
        // Replace any spaces with hyphens and capitalize the result
        string formatted = lastPart.Replace("-", " ");
        string tag = string.Join(" ", formatted.Split(' ').Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
        //Debug.WriteLine("tag is:" + tag);
        return tag;
    }

    public string ExtractGroup(string url)
    {
        url = url.TrimEnd('/');

        // Find the index after ".com/"
        int startIndex = url.IndexOf(".com/") + 5;  // Add 5 to get the position after ".com/"

        // Extract the substring between ".com/" and the next "/"
        int endIndex = url.IndexOf('/', startIndex);

        // If there is no further slash, take the rest of the string after ".com/"
        if (endIndex == -1)
        {
            return url.Substring(startIndex);
        }

        // Extract and return the category part of the URL
        return url.Substring(startIndex, endIndex - startIndex);
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
                return new Rect(0.22, 0.10, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            }
            else
            {
                return new Rect(0.28, 0.10, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            }
        }
        return null; //new Rect(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);

    }

    public Object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }  
}

public class NotEmptyStringConverters : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo
 culture)
    {
        return !string.IsNullOrEmpty((string)value);
    }

    public object ConvertBack(object value, Type targetType,
 object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
