using HtmlAgilityPack;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml;


namespace AccessReel
{
    public partial class MainPage : ContentPage
    {
        public static string ACCESSREELURL = "https://accessreel.com/";
        public static string NEWSURL = "https://accessreel.com/categories/news/";

        HtmlDocument document;
        string text;

        public MainPage()
        {
            InitializeComponent();
            //Test Data
            List<Review> postList = new List<Review>();
            Review c = new Review { Author = "Test Author", Date = DateTime.Today, Description = "Article Body", Title = "Article title", ReviewScore = "10" };
            postList.Add(c);
            postList.Add(c);
            postList.Add(c);
            postList.Add(c);
            //CVImageheader.ItemsSource = postList; //DONE - Check region "CAROUSEL"
            //CVNews.ItemsSource = postList; //DONE 
            //CVInterviews.ItemsSource = postList; //DONE 
            //CVReviews.ItemsSource = postList; //DONE
            CVUserReviews.ItemsSource = postList;
            CVTrailers.ItemsSource = postList;
            

            Retrieve(ReadWebsite());
        }

        // Returns the html
        private string ReadWebsite()
        {
            var web = new HtmlWeb();
            text = web.Load("https://accessreel.com/").Text;            
            return text;
        }

        // Extracts the details of details details we need from the node
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
        }

        private void Retrieve(string text)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(text);

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

                    // DEBUG - COMMENT OUT ONCE FINISHED
             /*       Debug.WriteLine("Title: " + title);
                    Debug.WriteLine("Href Link: " + href);
                    Debug.WriteLine("Image Source: " + imageSource);
                    Debug.WriteLine("Critic Rating: " + criticRating);
                    Debug.WriteLine("");*/
                    
                    // Create a new item (using Review for now because it has everything we need) - Could create a new class "Carousel" if we wanted
                    Review carouselItem = new Review {Title = title, Url = href, Image = imageSource, ReviewScore = criticRating };
                    carouselList.Add(carouselItem);
                }
            }
            // Populate XAML carousel
            CVImageheader.ItemsSource = carouselList;

            #endregion

            // Node wrapper that contains "Latest News", "Latest Interviews", "Latest Reviews", "Top User Rated Reviews" or "TURR" for short and "New Trailers"
            var wpdWrapper = document.DocumentNode.SelectSingleNode("/html/body/div[3]/div[2]/div[3]/div[1]/div[2]/div[1]/div/div");

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
                Debug.WriteLine($"{title}\n{paragraph}\n{image}\n");
                Review lastReviewsItem = new Review { Title = title, Url = href, Description = paragraph, Image = image };
                lastestReviewsList.Add(lastReviewsItem);
            }
            // Populate XAML list
            CVReviews.ItemsSource = lastestReviewsList;

            #endregion
            
            // Retrieve "Top User Rated Reviews"
            Debug.WriteLine("LASTEST REVIEWS:" + "\n");

            var postItem = document.DocumentNode.SelectSingleNode("//*[@id=\"ghostpool_showcase_wrapper_1\"]/div[5]/section");
            var smallPosts = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'gp-small-posts')]");
            var sectionSmallPosts = smallPosts.SelectNodes(".//section[contains(@class, 'gp-post-item')]");

            // Extract href link
            var linkNode = postItem.SelectSingleNode(".//a[@href]");
            string thref = linkNode.Attributes["href"].Value;

            // Extract title
            string ttitle = linkNode.SelectSingleNode(".//h2").InnerText.Trim();

            // Extract source image 
            var timageNode = linkNode.SelectSingleNode(".//img[contains(@class, 'gp-large-image')]");
            string sourceImage = timageNode.Attributes["src"].Value;

            // Extract member and critic ratings
            var ratingWrapper = postItem.SelectSingleNode(".//div[@class='gp-rating-wrapper']");
            string memberRating = ratingWrapper.SelectSingleNode(".//div[@class='gp-average-rating']").InnerText.Trim();
            string tcriticRating = ratingWrapper.SelectSingleNode(".//div[@class='gp-rating-inner']").InnerText.Trim();

            // Write to console or log file
            Debug.WriteLine("href: " + thref);
            Debug.WriteLine("title: " + ttitle);
            Debug.WriteLine("sourceImage: " + sourceImage);
            Debug.WriteLine("memberRating: " + memberRating);
            Debug.WriteLine("criticRating: " + tcriticRating);
            Debug.WriteLine("");

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
                Debug.WriteLine("smallHref: " + smallHref);
                Debug.WriteLine("smallTitle: " + smallTitle);
                Debug.WriteLine("smallSourceImage: " + smallSourceImage);
                Debug.WriteLine("smallMemberRating: " + smallMemberRating);
                Debug.WriteLine("smallCriticRating: " + smallCriticRating);
                Debug.WriteLine("");
            }

            // Retrieve "New Trailers"
            Debug.WriteLine("NEW TRAILERS:");
            var newTrailersNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-trailers')]");
            foreach (var node in newTrailersNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                     , out string user, out string datetime, out string href, out string userHref);
                Debug.WriteLine($"{title}\n{image}\n\n");
            }

        }

    }
}