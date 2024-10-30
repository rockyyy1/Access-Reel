using HtmlAgilityPack;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml;


namespace AccessReel
{
    public partial class MainPage : ContentPage
    {
        HtmlDocument document;
        string text;

        public MainPage()
        {
            InitializeComponent();
            
            Retrieve(ReadWebsite());
        }

        // Returns the html
        private string ReadWebsite()
        {
            var web = new HtmlWeb();
            text = web.Load("https://accessreel.com/").Text;            
            return text;
        }

        private void Retrieve(string text)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(text);

            var wpd_wrapper = document.DocumentNode.SelectSingleNode("/html/body/div[3]/div[2]/div[3]/div[1]/div[2]/div[1]/div/div");

            // Retrieve "Latest News"
            Debug.WriteLine("LASTEST NEWS:");
            HtmlNodeCollection latestNewsNodes = wpd_wrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-news')]");

            foreach ( var node in latestNewsNodes)
            {
                var item = node as HtmlNode;
                string title = item.SelectSingleNode(".//h2[contains(@class, 'gp-loop-title')]//a").GetAttributeValue("title", string.Empty);
                string image = item.SelectSingleNode(".//img").GetAttributeValue("src", string.Empty);

                HtmlNode? paragraphItem = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-text')]//p");
                string paragraph = paragraphItem == null ? "" : paragraphItem.InnerText;

                Debug.WriteLine(title + "\n" + paragraph + "\n" + image + "\n");
            }

            // Retrieve "Latest Interviews"
            Debug.WriteLine("LASTEST INTERVIEWS:");
            HtmlNodeCollection latestInterviewsNodes = wpd_wrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-interviews')]");

            foreach (var node in latestInterviewsNodes)
            {
                var item = node as HtmlNode;
                string title = item.SelectSingleNode(".//h2[contains(@class, 'gp-loop-title')]//a").GetAttributeValue("title", string.Empty);
                string image = item.SelectSingleNode(".//img").GetAttributeValue("src", string.Empty);

                HtmlNode? paragraphItem = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-text')]//p");
                string paragraph = paragraphItem == null ? "" : paragraphItem.InnerText;

                Debug.WriteLine(title + "\n" + paragraph + "\n" + image + "\n");
            }

            // Retrieve "Latest Reviews"
            Debug.WriteLine("LASTEST REVIEWS:");
            HtmlNodeCollection latestReviewsNodes = wpd_wrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'gp_hubs-reviews')]");

            foreach (var node in latestReviewsNodes)
            {
                var item = node as HtmlNode;
                string title = item.SelectSingleNode(".//h2[contains(@class, 'gp-loop-title')]//a").GetAttributeValue("title", string.Empty);
                string image = item.SelectSingleNode(".//img").GetAttributeValue("src", string.Empty);

                HtmlNode? paragraphItem = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-text')]//p");
                string paragraph = paragraphItem == null ? "" : paragraphItem.InnerText;

                Debug.WriteLine(title + "\n" + paragraph + "\n" + image + "\n");
            }

            // Retrieve "Top User Rated Reviews"
            Debug.WriteLine("LASTEST REVIEWS:" + "\n\n");
            // Don't really know how to get this...

            //Retrieve "New Trailers"
            Debug.WriteLine("NEW TRAILERS:");
            HtmlNodeCollection newTrailersNodes = wpd_wrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-trailers')]");

            foreach (var node in newTrailersNodes)
            {
                var item = node as HtmlNode;
                string title = item.SelectSingleNode(".//h2[contains(@class, 'gp-loop-title')]//a").GetAttributeValue("title", string.Empty);
                string image = item.SelectSingleNode(".//img").GetAttributeValue("src", string.Empty);

                Debug.WriteLine(title + "\n" + image + "\n\n");
            }

        }
    }
}
