using System.Collections;
using System.Diagnostics;
using System.Globalization;
using HtmlAgilityPack;
using Microsoft.Maui.Controls;

namespace AccessReel;

public partial class ListPage : ContentPage
{
    List<Posts> newsList = new List<Posts>();

    List<string> sortPickerItems = new List<string> { "Newest", "Oldest", "Title (A-Z)", "Title (Z-A)",
        "Author (A-Z)", "Author (Z-A)"  };
    List<string> sortPickerIfReviews = new List<string> { "Top Site Rated" };

    private string? tagurl;
    string? Authorurl;

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
    public ListPage(string pageType = "", string? tagurl = null, string? authorurl = null)
    {
        InitializeComponent();

        Sorter.ItemsSource = sortPickerItems;
        Sorter.SelectedIndex = 0;

        this.tagurl = tagurl;
        Authorurl = authorurl;

        if (tagurl != null)
        {
            // GetTagInfo(tagurl);
        }

        Title.Text = pageType;
        LoadData(pageType);
        
    }

    private void GetTagInfo(string url, out string group, out string title)
    {
        //int index1 = url.IndexOf(".com/") + 4;
        //string group = url.Substring(index1, url.Length - 1);

        List<string> items = url.Split("/").ToList();

        group = items[3] + "/";
        title = items[4];
        //Debug.WriteLine("INDEX - " + items[2]);
    }
    private string GetAuthorName(string url)
    {
        List<string> items = url.Split("/").Where(item => !string.IsNullOrEmpty(item)).ToList();
        string authorName = items.LastOrDefault();
        //Debug.WriteLine("GetAuthorName returns: " + authorName);
        return authorName; 
    }

    // function loads data from all pages
    private void LoadDataOnAllPages(string pageType, string _url = "")
    {
        //List<Posts> newsList = new List<Posts>();

        int page = 1;
        int lastPage = 1;

        string group = pageType == "News" || pageType == "Interviews" ? "categories/" : "hubs/";
        if (this.tagurl != null)
        {
            string taggroup;
            this.GetTagInfo(this.tagurl, out taggroup, out pageType);

            group = taggroup;

            //change title
            Title.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pageType.ToLower()).Replace("-"," ");
        }
        if (Authorurl != null)
        {
            group = "author/";
            pageType = GetAuthorName(Authorurl);
            //change title
            Title.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pageType.ToLower()).Replace("-", " ");
        }
        #region FIND LAST PAGE NUMBER
        var urlp = "https://accessreel.com/" + group + pageType;
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
        #endregion
        while (page < lastPage)
        {
            var url = "https://accessreel.com/" + group + pageType + "/page/" + page.ToString();
            Debug.WriteLine(url);
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
                    if (pageType == "Reviews" || pageType == "Films" || this.tagurl != null || Authorurl != null)
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

                    if (page == lastPage)
                    {
                        break;
                    }
                    page += 1;
                }
            }
        }
        if (group == "categories/")
        {
            CVArticles.ItemTemplate = DTArticle;
            CVArticles.ItemsSource = newsList;
        }
        else if (group == "hubs/" || this.tagurl != null || Authorurl != null)
        {
            CVArticles.ItemTemplate = DTMovieArticle;
            CVArticles.ItemsSource = newsList;
            List<string> SortOptions = sortPickerItems.Concat(sortPickerIfReviews).ToList();
            Sorter.ItemsSource = SortOptions;          
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
            var item = (Review)((VisualElement)sender).BindingContext;
            NavigationPage authorListPage = new NavigationPage(new ListPage("Author", authorurl: item.AuthorUrl));
            await Navigation.PushAsync(authorListPage);
        }
    }

    private void Sorter_SelectedIndexChanged(object sender, EventArgs e)
    {
        int index = Sorter.SelectedIndex;
        List<Review> reviewList = newsList.ConvertAll(post => post as Review);
        switch (index)
        {
            case 0:
                Debug.WriteLine("Sorted by Newest");
                Posts.SortByDate compNewest = new Posts.SortByDate(ascending : false);
                newsList.Sort(compNewest);
                break;
            case 1:
                Debug.WriteLine("Sorted by Oldest");
                Posts.SortByDate compOldest = new Posts.SortByDate(ascending: true);
                newsList.Sort(compOldest);
                break;
            case 2:
                Debug.WriteLine("Sorted by Title (A-Z)");
                Posts.SortByTitle compTitleDesc = new Posts.SortByTitle(ascending: true);
                newsList.Sort(compTitleDesc);
                break;
            case 3:
                Debug.WriteLine("Sorted by Title (Z-A)");
                Posts.SortByTitle compTitle = new Posts.SortByTitle();
                newsList.Sort(compTitle);
                break;
            case 4:
                Debug.WriteLine("Sorted by Author (A-Z)");
                Posts.SortByAuthor compAuthor = new Posts.SortByAuthor(ascending: true);
                newsList.Sort(compAuthor);
                break;
            case 5:
                Debug.WriteLine("Sorted by Author (Z-A)");
                Posts.SortByAuthor compAuthor2 = new Posts.SortByAuthor();
                newsList.Sort(compAuthor2);               
                break;
            case 6:
                Debug.WriteLine("Sorted by Review Score");                
                Review.SortbyReviewScore compReview = new Review.SortbyReviewScore();
                reviewList.Sort(compReview);
                CVArticles.ItemsSource = null;
                CVArticles.ItemsSource = reviewList;
                break;
            case 7:
                Debug.WriteLine("Sorted by Member Review Score");
                Review.SortbyReviewScore compMember = new Review.SortbyReviewScore();
                reviewList.Sort(compMember);
                CVArticles.ItemsSource = null;
                CVArticles.ItemsSource = reviewList;
                break;
        }
        if(index >= 0 && index < 6)
        {
            CVArticles.ItemsSource = null;
            CVArticles.ItemsSource = newsList;
        }
        else if(index < 8)
        {
            CVArticles.ItemsSource = null;
            CVArticles.ItemsSource = reviewList;
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