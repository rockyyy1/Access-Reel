using HtmlAgilityPack;
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
            CVInterviews.ItemsSource = postList;
            CVNews.ItemsSource = postList;
            CVReviews.ItemsSource = postList;
            CVUserReviews.ItemsSource = postList;
            CVImageheader.ItemsSource = postList;
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
            out string user, out string datetime)
        {
            title = item.SelectSingleNode(".//h2[contains(@class, 'gp-loop-title')]//a").GetAttributeValue("title", string.Empty);
            image = item.SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty);
            paragraph = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-text')]//p")?.InnerText ?? string.Empty;
            user = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-meta')]//span//a")?.InnerText;
            datetime = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-meta')]//time")?.GetAttributeValue("datetime", string.Empty);

            title = HtmlEntity.DeEntitize(title);
            paragraph = HtmlEntity.DeEntitize(paragraph);
            DateTime? time = datetime != null ? DateTime.Parse(datetime) : null;

            datetime = time?.ToString("ddd - MMM - yyyy");
        }

        private void Retrieve(string text)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(text);

            // Retrieve the carousel at the top of the page
            Debug.WriteLine("TOP CAROUSEL");
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

                    // Href link
                    string href = link.GetAttributeValue("href", "");

                    // Image source
                    HtmlNode imageNode = liNode.SelectSingleNode(".//div[@class='gp-post-thumbnail']//img[@class='gp-post-image']");
                    string imageSource = imageNode?.GetAttributeValue("src", "");

                    // Critic rating
                    HtmlNode ratingNode = liNode.SelectSingleNode(".//div[@class='gp-rating-outer']//div[@class='gp-rating-inner']");
                    string criticRating = ratingNode?.InnerText?.Trim() ?? "";

                    // Process the extracted information (e.g., print to console)
                    Debug.WriteLine("Title: " + title);
                    Debug.WriteLine("Href Link: " + href);
                    Debug.WriteLine("Image Source: " + imageSource);
                    Debug.WriteLine("Critic Rating: " + criticRating);
                    Debug.WriteLine("");
                }
            }

            var wpdWrapper = document.DocumentNode.SelectSingleNode("/html/body/div[3]/div[2]/div[3]/div[1]/div[2]/div[1]/div/div");

            // Retrieve "Latest News"
            Debug.WriteLine("LASTEST NEWS:");
            var latestNewsNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-news')]");
            foreach (var node in latestNewsNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                    , out string user, out string datetime);
                Debug.WriteLine($"{title}\n{paragraph}\n{image}\n{user}\n{datetime}\n");
            }

            // Retrieve "Latest Interviews"
            Debug.WriteLine("LASTEST INTERVIEWS:");
            var latestInterviewsNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-interviews')]");
            foreach (var node in latestInterviewsNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                    , out string user, out string datetime);
                Debug.WriteLine($"{title}\n{paragraph}\n{image}\n");
            }

            // Retrieve "Latest Reviews"
            Debug.WriteLine("LASTEST REVIEWS:");
            var latestReviewsNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'gp_hubs-reviews')]");
            foreach (var node in latestReviewsNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                    , out string user, out string datetime);
                Debug.WriteLine($"{title}\n{paragraph}\n{image}\n");
            }

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
                     , out string user, out string datetime);
                Debug.WriteLine($"{title}\n{image}\n\n");
            }

        }

    }
}