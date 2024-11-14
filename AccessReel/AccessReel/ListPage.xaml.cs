using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Security.AccessControl;
using HtmlAgilityPack;
using Microsoft.Maui.Controls;

namespace AccessReel;

public partial class ListPage : ContentPage
{
    ObservableCollection<Posts> newsList = new ObservableCollection<Posts>();

    List<string> sortPickerItems = new List<string> { "Newest", "Oldest", "Title (A-Z)", "Title (Z-A)",
        "Author (A-Z)", "Author (Z-A)"  };
    List<string> sortPickerIfReviews = new List<string> { "Top Site Rated" };

    private string? tagurl;
    string? Authorurl;
    public int currentPage = 1;
    public int lastPage = 0;
    public string group;
    public string pageType;

    public ListPage() 
    {
        InitializeComponent();
    }

    public ListPage(string pageType = "", string? tagurl = null, string? authorurl = null)
    {
        InitializeComponent();

        #region SETUP
        this.pageType = pageType;
        Sorter.ItemsSource = sortPickerItems;
        Sorter.SelectedIndex = 0;
        this.tagurl = tagurl;
        Authorurl = authorurl;
        Banner.Source = pageType + ".jpg";
        Title.Text = pageType;;
        LoadData(pageType);
        #endregion
    }

    // EXTRACT GROUP AND TITLE FROM URL
    private void GetTagInfo(string url, out string group, out string title)
    {
        //int index1 = url.IndexOf(".com/") + 4;
        //string group = url.Substring(index1, url.Length - 1);

        List<string> items = url.Split("/").ToList();

        group = items[3] + "/";
        title = items[4];
        //Debug.WriteLine("INDEX - " + items[2]);
    }
    // EXTRACT AUTHOR NAME FROM URL
    private string GetAuthorName(string url)
    {
        List<string> items = url.Split("/").Where(item => !string.IsNullOrEmpty(item)).ToList();
        string authorName = items.LastOrDefault();
        //Debug.WriteLine("GetAuthorName returns: " + authorName);
        return authorName;
    }

    // LOAD THE DATA FROM AN SINGLE PAGE
    private async void LoadDataOnAnPage(int page, string group, string pageType)
    {
        #region SETUP
        /*int page = 1;
        int lastPage = 1;

        string group = pageType == "News" || pageType == "Interviews" ? "categories/" : "hubs/";
        if (this.tagurl != null)
        {
            string taggroup;
            this.GetTagInfo(this.tagurl, out taggroup, out pageType);

            group = taggroup;

            //change title
            Title.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pageType.ToLower()).Replace("-", " ");

            //change banner
            Banner.Source = null;
        }
        if (Authorurl != null)
        {
            group = "author/";
            pageType = GetAuthorName(Authorurl);
            //change title
            Title.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pageType.ToLower()).Replace("-", " ");
            Banner.Source = null;
        }*/
        #endregion

        var url = "https://accessreel.com/" + group + pageType + "/page/" + page.ToString();
        Debug.WriteLine(url);
        var web = new HtmlWeb();
        var document = await web.LoadFromWebAsync(url);

        var parentContainer = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'gp-blog-wrapper gp-blog-standard')]");
        if (parentContainer == null)
        {
            return;
        }
        var nodes = parentContainer.SelectNodes(".//section[contains(@class, 'gp-post-item')]");
        if (nodes == null || nodes.Count == 0)
        {
            return;
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
            }
            this.currentPage += 1;

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
    }

    // LOAD ALL DATA V2
    private async void LoadDataOnAllPages(string pageType)
    {
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

            //change banner
            Banner.Source = null;
        }
        if (Authorurl != null)
        {
            group = "author/";
            pageType = GetAuthorName(Authorurl);
            //change title
            Title.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pageType.ToLower()).Replace("-", " ");
            Banner.Source = null;
        }


        while (page <= 1)
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

                    if (page > lastPage)
                    {
                        break;
                    }
                }
                page += 1;
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

    // FIND LAST PAGE NUMBER
    private async Task<int> FindLastPageNumber(string group, string pageType)
    {
        var urlp = "https://accessreel.com/" + group + pageType;
        var webp = new HtmlWeb();
        var documentp = await webp.LoadFromWebAsync(urlp);
        var pageLinks = documentp.DocumentNode.SelectNodes("//a[@class='page-numbers']");
        if (pageLinks != null && pageLinks.Count > 0)
        {
            // Get the last page number
            string lastPageUrl = pageLinks[pageLinks.Count - 1].GetAttributeValue("href", string.Empty);
            // Extract the last page number from the URL
            return int.Parse(lastPageUrl.Split('/')[^2]);
            //Debug.WriteLine($"Number of pages: {lastPage}");
        }
        return -1;
    }
    // LOAD ALL DATA V1 - BACKUP
    private async void LoadData(string pageType)
    {

        #region SETUP

        string group = pageType == "News" || pageType == "Interviews" ? "categories/" : "hubs/";
        if (this.tagurl != null)
        {
            string taggroup;
            this.GetTagInfo(this.tagurl, out taggroup, out pageType);

            group = taggroup;

            //change title
            Title.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pageType.ToLower()).Replace("-", " ");

            //change banner
            Banner.Source = null;
        }
        if (Authorurl != null)
        {
            group = "author/";
            pageType = GetAuthorName(Authorurl);
            //change title
            Title.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pageType.ToLower()).Replace("-", " ");
            Banner.Source = null;
        }
        #endregion
        this.lastPage = await this.FindLastPageNumber(group, pageType);

        this.group = group;
        LoadDataOnAnPage(this.currentPage, this.group, this.pageType);

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

    // NAVIGATE TO ARTICLE PAGE
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

    // NAVIGATE TO AUTHOR LISTPAGE
    private async void AuthorTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label)
        {
            var item = (Posts)((VisualElement)sender).BindingContext;
            NavigationPage authorListPage = new NavigationPage(new ListPage("Author", authorurl: item.AuthorUrl));
            await Navigation.PushAsync(authorListPage);
        }
    }
    // SORTER CHANGE
    private void Sorter_SelectedIndexChanged(object sender, EventArgs e)
    {
        int index = Sorter.SelectedIndex;

        List<Posts> sortedList = new List<Posts>();
        switch (index)
        {
            case 0:
                Debug.WriteLine("Sorted by Newest");
                sortedList = newsList.OrderBy(p => p, new Posts.SortByDate()).ToList();
                break;
            case 1:
                Debug.WriteLine("Sorted by Oldest");
                sortedList = newsList.OrderBy(p => p, new Posts.SortByDate(ascending: true)).ToList();
                break;
            case 2:
                Debug.WriteLine("Sorted by Title (A-Z)");
                sortedList = newsList.OrderBy(p => p, new Posts.SortByTitle(ascending: true)).ToList();
                break;
            case 3:
                Debug.WriteLine("Sorted by Title (Z-A)");
                sortedList = newsList.OrderBy(p => p, new Posts.SortByTitle()).ToList();
                break;
            case 4:
                Debug.WriteLine("Sorted by Author (A-Z)");
                sortedList = newsList.OrderBy(p => p, new Posts.SortByAuthor(ascending: true)).ToList();
                break;
            case 5:
                Debug.WriteLine("Sorted by Author (Z-A)");
                sortedList = newsList.OrderBy(p => p, new Posts.SortByAuthor()).ToList();
                break;
            case 6:
                Debug.WriteLine("Sorted by Review Score");
                sortedList = newsList.Where(post => post is Review review && review.ReviewScore != null).OrderBy(p => (Review)p, new Review.SortbyReviewScore()).ThenBy(post => post.Title).ToList();
                break;/*
            case 7:
                Debug.WriteLine("Sorted by Member Review Score");
                Review.SortbyReviewScore compMember = new Review.SortbyReviewScore();
                reviewList.Sort(compMember);
                break;*/
        }
        if(index >= 0 && index < 8)
        {
            newsList.Clear();
            foreach (var item in sortedList)
            {
                newsList.Add(item);
            }
        }
    }

    private void LoadMoreContentButton_Clicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;

        LoadDataOnAnPage(this.currentPage, this.group, this.pageType);
        if ( this.currentPage >= this.lastPage )
        {
            button.IsEnabled = false;
        }
    }
}


// This changes the postion of the label within the circle for the review score
public class ReviewScoreToAbsLayoutConverter : IValueConverter
{
    public Object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text)
        {
            if (text.Length == 2)
            {
                return new Rect(0.22, 0.20, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            }
            else
            {
                return new Rect(0.35, 0.20, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            }
        }
        return null; //new Rect(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);

    }

    public Object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}