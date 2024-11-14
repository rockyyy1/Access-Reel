using HtmlAgilityPack;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml;



namespace AccessReel
{
    public partial class MainPage : ContentPage
    {
        public static string ACCESSREELURL = "https://accessreel.com/";
        public static string NEWSURL = "https://accessreel.com/categories/news/";
        string text;

        public MainPage()
        {
            InitializeComponent();
            //Retrieve(ReadWebsite());
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            CheckForNewPostsSendNotification();
            Retrieve(await ReadWebsite());
        }

        // RETURN HTML DOC
        private async Task<string> ReadWebsite()
        {
            var web = new HtmlWeb();
            text = (await web.LoadFromWebAsync("https://accessreel.com/")).Text;
            return text;
        }
        /*        private string ReadWebsite()
                {
                    var web = new HtmlWeb();
                    text = web.Load("https://accessreel.com/").Text;
                    return text;
                }*/

        // EXTRACT NODE
        private void RetrieveItemDetails(HtmlNode item, out string title, out string image, out string paragraph,
            out string user, out string datetime, out string href, out string userHref)
        {
            title = item.SelectSingleNode(".//h2[contains(@class, 'gp-loop-title')]//a").GetAttributeValue("title", string.Empty);
            href = item.SelectSingleNode(".//h2[contains(@class, 'gp-loop-title')]//a").GetAttributeValue("href", string.Empty);
            image = item.SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty);
            paragraph = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-text')]//p")?.InnerText ?? string.Empty;
            user = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-meta')]//span//a")?.InnerText;
            userHref = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-meta')]//span//a")?.GetAttributeValue("href", string.Empty);
            datetime = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-meta')]//time")?.GetAttributeValue("datetime", string.Empty);

            title = HtmlEntity.DeEntitize(title);
            paragraph = HtmlEntity.DeEntitize(paragraph);
            DateTime? time = datetime != null ? DateTime.Parse(datetime) : null;

            datetime = time?.ToString("dd - MMM - yyyy");
        }

        private void Retrieve(string text)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(text);

            // Node wrapper that contains "Latest News", "Latest Interviews", "Latest Reviews", "Top User Rated Reviews" or "TURR" for short and "New Trailers"
            var wpdWrapper = document.DocumentNode.SelectSingleNode("/html/body/div[3]/div[2]/div[3]/div[1]/div[2]/div[1]/div/div");

            // Image Carousel at the top of the page
            #region CAROUSEL
            // Debug.WriteLine("TOP CAROUSEL");
            List<Review> carouselList = new List<Review>();
            var carouselWrapper = document.DocumentNode.SelectSingleNode("//ul[@class='slides']");
            foreach (HtmlNode liNode in carouselWrapper.SelectNodes(".//li"))
            {
                // Extract information from each li
                HtmlNode link = liNode.SelectSingleNode(".//a");

                if (link != null)
                {
                    // Title
                    HtmlNode titleNode = link.SelectSingleNode(".//h2[@class='gp-slide-caption-title']//span[@class='gp-text-highlight']");
                    string title = titleNode?.InnerText ?? "";
                    title = HtmlEntity.DeEntitize(title);

                    // Href link
                    string href = link.GetAttributeValue("href", "");

                    // Image source
                    HtmlNode imageNode = liNode.SelectSingleNode(".//div[@class='gp-post-thumbnail']//img[@class='gp-post-image']");
                    string imageSource = imageNode?.GetAttributeValue("src", "");

                    // Critic rating
                    HtmlNode ratingNode = liNode.SelectSingleNode(".//div[@class='gp-rating-outer']//div[@class='gp-rating-inner']");
                    string criticRating = ratingNode?.InnerText?.Trim() ?? "";

                    // Member rating
                    HtmlNode membersRating = liNode.SelectSingleNode(".//div[@class='gp-user-rating-wrapper gp-large-rating']");
                    string membersRatings = "";

                    if (membersRating != null)
                    {
                        HtmlNode ratingInner = membersRating.SelectSingleNode(".//div[@class='gp-rating-inner']");
                        membersRatings = ratingInner?.InnerText?.Trim() ?? "";
                    }

                    // DEBUG - COMMENT OUT ONCE FINISHED
                    /*       Debug.WriteLine("Title: " + title);
                           Debug.WriteLine("Href Link: " + href);
                           Debug.WriteLine("Image Source: " + imageSource);
                           Debug.WriteLine("Critic Rating: " + criticRating);
                           Debug.WriteLine("");*/

                    // Create a new item (using Review for now because it has everything we need) - Could create a new class "Carousel" if we wanted
                    Review carouselItem = new Review {Title = title, Url = href, Image = imageSource, ReviewScore = criticRating, MemberReviewScore = membersRatings };
                    carouselList.Add(carouselItem);
                }
            }
            // Populate XAML carousel
            CVImageheader.ItemsSource = carouselList;

            #endregion

            // Retrieve "Latest News"
            #region LATEST NEWS
            
            // Create a new list for latest news
            List<Posts> latestNewsList = new List<Posts>();

            //Debug.WriteLine("LASTEST NEWS:");
            // Go to the needed node, loop through what we need and create items with Posts class
            var latestNewsNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-news')]");
            foreach (var node in latestNewsNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                    , out string user, out string datetime, out string href, out string userHref);
                //Debug.WriteLine($"{title}\n{paragraph}\n{image}\n{user}\n{datetime}\nhref: {href}\nauthor href: {userHref}\n\n");
                Posts latestNewsItem = new Posts { Title = title, Url = href, Description = paragraph, Image = image, Author = user, AuthorUrl = userHref, Date = datetime != null ? DateTime.Parse(datetime) : null};
                latestNewsList.Add(latestNewsItem);
            }
            // Populate XAML list
            CVNews.ItemsSource = latestNewsList;
            #endregion

            // Retrieve "Latest Interviews"
            #region LATEST INTERVIEWS

            // Create a new list for latest interviews
            List<Posts> lastestInterviewsList = new List<Posts>();
            //Debug.WriteLine("LASTEST INTERVIEWS:");
            var latestInterviewsNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-interviews')]");
            foreach (var node in latestInterviewsNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                    , out string user, out string datetime, out string href, out string userHref);
                //Debug.WriteLine($"{title}\n{paragraph}\n{image}\n");
                Posts latestInterviewItem = new Posts { Title = title, Url = href, Description = paragraph, Image = image, Author = user, AuthorUrl = userHref, Date = datetime != null ? DateTime.Parse(datetime) : null };
                lastestInterviewsList.Add(latestInterviewItem);
            }
            // Populate XAML list
            CVInterviews.ItemsSource = lastestInterviewsList;
            #endregion

            // Retrieve "Latest Reviews"
            #region LATEST REVIEWS

            // Create a new list for latest reviews
            List<Review> lastestReviewsList = new List<Review>();

            //Debug.WriteLine("LASTEST REVIEWS:");
            var latestReviewsNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'gp_hubs-reviews')]");
            foreach (var node in latestReviewsNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                    , out string user, out string datetime, out string href, out string userHref);
                //Debug.WriteLine($"{title}\n{paragraph}\n{image}\n");
                Review lastReviewsItem = new Review { Title = title, Url = href, Description = paragraph, Image = image };
                lastestReviewsList.Add(lastReviewsItem);
            }
            // Populate XAML list
            CVReviews.ItemsSource = lastestReviewsList;

            #endregion

            // Retrieve "Top User Rated Reviews"
            #region TOP USER RATED REVIEWS (TURR)

            // Create a new list for latest reviews
            List<Review> TURRList = new List<Review>();

            //Debug.WriteLine("Top User Rated Reviews):" + "\n");

            var postItem = document.DocumentNode.SelectSingleNode("//*[@id=\"ghostpool_showcase_wrapper_1\"]/div[5]/section");
            var smallPosts = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'gp-small-posts')]");
            var sectionSmallPosts = smallPosts.SelectNodes(".//section[contains(@class, 'gp-post-item')]");

            // Extract href link
            var linkNode = postItem.SelectSingleNode(".//a[@href]");
            string TURRhref = linkNode.Attributes["href"].Value;

            // Extract title
            string TURRtitle = linkNode.SelectSingleNode(".//h2").InnerText.Trim();

            // Extract source image 
            var TURRimageNode = linkNode.SelectSingleNode(".//img[contains(@class, 'gp-large-image')]");
            string TURRsourceImage = TURRimageNode.Attributes["src"].Value;

            // Extract member and critic ratings
            var ratingWrapper = postItem.SelectSingleNode(".//div[@class='gp-rating-wrapper']");
            string memberRating = ratingWrapper.SelectSingleNode(".//div[@class='gp-average-rating']").InnerText.Trim();
            string TURRcriticRating = ratingWrapper.SelectSingleNode(".//div[@class='gp-rating-inner']").InnerText.Trim();

            // Debug
/*            Debug.WriteLine("href: " + TURRhref);
            Debug.WriteLine("title: " + TURRtitle);
            Debug.WriteLine("sourceImage: " + TURRsourceImage);
            Debug.WriteLine("memberRating: " + memberRating);
            Debug.WriteLine("criticRating: " + TURRcriticRating);
            Debug.WriteLine("");*/

            Review TURRitem = new Review { Title = TURRtitle, Url = TURRhref, Image = TURRsourceImage, ReviewScore = TURRcriticRating, MemberReviewScore = memberRating };
            TURRList.Add(TURRitem);

            foreach (var posts in sectionSmallPosts)
            {
                // Extract href link
                var smalllinkNode = posts.SelectSingleNode(".//a[@href]");
                string smallHref = smalllinkNode.Attributes["href"].Value;

                // Extract title
                string smallTitle = smalllinkNode.Attributes["title"].Value;

                // Extract source image with high resolution (data-rel points to larger image)
                var smallimageNode = smalllinkNode.SelectSingleNode(".//img");
                string smallSourceImage = smallimageNode.Attributes["data-rel"].Value;

                // Extract member and critic ratings
                var smallratingWrapper = posts.SelectSingleNode(".//div[@class='gp-rating-wrapper']");
                string smallMemberRating = smallratingWrapper.SelectSingleNode(".//div[@class='gp-average-rating']").InnerText.Trim();
                string smallCriticRating = smallratingWrapper.SelectSingleNode(".//div[@class='gp-rating-inner']").InnerText.Trim();

                // Write to console or log file (add prefix as requested)
    /*            Debug.WriteLine("smallHref: " + smallHref);
                Debug.WriteLine("smallTitle: " + smallTitle);
                Debug.WriteLine("smallSourceImage: " + smallSourceImage);
                Debug.WriteLine("smallMemberRating: " + smallMemberRating);
                Debug.WriteLine("smallCriticRating: " + smallCriticRating);
                Debug.WriteLine("");*/

                Review smallTURRitem = new Review { Title = smallTitle, Url = smallHref, Image = smallSourceImage, ReviewScore = smallCriticRating, MemberReviewScore = smallMemberRating };
                TURRList.Add(smallTURRitem);
            }
            
            // Populate XAML list
            CVUserReviews.ItemsSource = TURRList;

            #endregion

            // Retrieve "New Trailers"
            #region NEW TRAILERS
            // Create a new list for New Trailers
            List<Review> newTrailersList = new List<Review>();
            //Debug.WriteLine("NEW TRAILERS:");
            var newTrailersNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-trailers')]");
            foreach (var node in newTrailersNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                     , out string user, out string datetime, out string href, out string userHref);
                //Debug.WriteLine($"{title}\n{image}\n{href}\n\n");
                Review newTrailerItem = new Review { Title = title, Url = href, Image = image };
                newTrailersList.Add(newTrailerItem);
            }
            CVTrailers.ItemsSource = newTrailersList;

            #endregion

        }

        // NAVIGATE TO ARTICLE ITEM
        private async void ArticleTapped(object sender, TappedEventArgs e)
        {
            if (sender is Label label || sender  is Image image)
            {
                // Access the DataContext of the Label
                var article = (Posts)((VisualElement)sender).BindingContext;

                // Debug
                //Debug.WriteLine(article.Url);

                string webpageURL = article.Url.ToString();
                    
                // make new article page
                ArticlePage newArticle = new ArticlePage(webpageURL);

                await Navigation.PushAsync(newArticle);

            }
        }

        // NAVIGATE TO AUTHOR LISTPAGE
        private async void AuthorTapped(object sender, TappedEventArgs e)
        {
            if (sender is Label label)
            {
                // Access the DataContext of the Label
                var article = (Posts)label.BindingContext;
                NavigationPage authorListPage = new NavigationPage(new ListPage("Author", authorurl: article.AuthorUrl));
                await Navigation.PushAsync(authorListPage);

            }
        }

        // NAVIGATE TO FILMPAGE
        private async void FilmTapped(object sender, TappedEventArgs e)
        {
            if (sender is Label label || sender is Image image)
            {
                // Access the DataContext of the Label
                var film = (Review)((VisualElement)sender).BindingContext;

                // make new article page
                FilmPage newFilm = new FilmPage(film);

                await Navigation.PushAsync(newFilm);

            }
        }

        // NAVIGATE TO TRAILERPAGE
        private async void TrailerTapped(object sender, TappedEventArgs e)
        {
            if (sender is Label label || sender is Image image)
            {
                var film = (Review)((VisualElement)sender).BindingContext;
                //Debug.WriteLine(film.Url);
                //Debug.WriteLine(film.Title);
                TrailerPage trailerItem = new TrailerPage(film);

                await Navigation.PushAsync(trailerItem);

            }
        }

        //SEND PUSH NOTIFICATION
        private void CheckForNewPostsSendNotification()
        {

        }
    }

    // SHOW/HIDE MEMBER RATING IF NULL
    public class NotEmptyStringConverter : IValueConverter
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
}