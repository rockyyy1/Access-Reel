using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AccessReel
{
    internal class Webscraping
    {
        public HtmlDocument document;
        string text;

        #region HomePage
        // Gets / Scrapes the access-reel homepage and returns an object with all informaton
        public Homepage GetHomepage()
        {
            var wpdWrapper = document.DocumentNode.SelectSingleNode("/html/body/div[3]/div[2]/div[3]/div[1]/div[2]/div[1]/div/div");

            List<Posts> latestNews = new List<Posts>();
            var latestNewsNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-news')]");
            foreach (var node in latestNewsNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                   , out string user, out string datetime, out string url);
                Posts post = new Posts()
                {
                    Title = title,
                    Image = ImageSource.FromUri(new Uri(image)),
                    Description = paragraph,
                    Author = user,
                    Date = datetime != null && datetime != string.Empty
                    ? DateTime.Parse(datetime) : null,
                    Url = url
                };

                latestNews.Add(post);
            }

            // Retrieve "Latest Interviews"
            List<Posts> latestInterviews = new List<Posts>();
            var latestInterviewsNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-interviews')]");
            foreach (var node in latestInterviewsNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                    , out string user, out string datetime, out string url);
                Posts post = new Posts()
                {
                    Title = title,
                    Image = ImageSource.FromUri(new Uri(image)),
                    Description = paragraph,
                    Author = user,
                    Date = datetime != null && datetime != string.Empty
                   ? DateTime.Parse(datetime) : null,
                    Url = url
                };

                latestInterviews.Add(post);
            }

            // Retrieve "Latest Reviews"
            List<Posts> latestReviews = new List<Posts>();
            
            var latestReviewsNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'gp_hubs-reviews')]");
            foreach (var node in latestReviewsNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                  , out string user, out string datetime, out string url);
                Posts post = new Posts()
                {
                    Title = title,
                    Image = ImageSource.FromUri(new Uri(image)),
                    Description = paragraph,
                    Author = user,
                    Date = datetime != null && datetime != string.Empty
                   ? DateTime.Parse(datetime) : null,
                    Url = url
                };

                latestReviews.Add(post);
            }

            // Retrieve "Top User Rated Reviews"
            //Debug.WriteLine("LASTEST REVIEWS:" + "\n\n");
            // ... Dont know how to do this lol
            var TURRWrapper = document.DocumentNode.SelectSingleNode("/html/body/div[3]/div[2]/div[3]/div[1]/div[2]/div[2]/div/div/div[3]");

            // Retrieve "New Trailers"
            List<Posts> newTrailers = new List<Posts>();
            var newTrailersNodes = wpdWrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article') and contains(@class, 'categories-trailers')]");
            foreach (var node in newTrailersNodes)
            {
                RetrieveItemDetails(node, out string title, out string image, out string paragraph
                  , out string user, out string datetime, out string url);
                Posts post = new Posts()
                {
                    Title = title,
                    Image = ImageSource.FromUri(new Uri(image)),
                    Description = paragraph,
                    Author = user,
                    Date = datetime != null && datetime != string.Empty
                   ? DateTime.Parse(datetime) : null,
                    Url = url
                };

                newTrailers.Add(post);
            }

            Homepage homepage = new Homepage() { 
            latestNews = latestNews,
            latestInterviews = latestInterviews, 
            latestReviews = latestReviews,
            newTrailers = newTrailers
            };


            return homepage;

        }
        #endregion

        public Webscraping()
        {
            string text;
            var web = new HtmlWeb();
            text = web.Load("https://accessreel.com/").Text;

            document = new HtmlDocument();
            document.LoadHtml(text);
        }

        public string ReadWebsite(string? url)
        {
            var web = new HtmlWeb();
            text = web.Load("https://accessreel.com/").Text;
            return text;
        }

        private void RetrieveItemDetails(HtmlNode item, out string title, out string image, out string paragraph,
            out string user, out string datetime, out string url)
        {
            title = item.SelectSingleNode(".//h2[contains(@class, 'gp-loop-title')]//a").GetAttributeValue("title", string.Empty);
            image = item.SelectSingleNode(".//img")?.GetAttributeValue("src", string.Empty);
            paragraph = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-text')]//p")?.InnerText ?? string.Empty;
            user = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-meta')]//span//a")?.InnerText;
            datetime = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-meta')]//time")?.GetAttributeValue("datetime", string.Empty);
            url = item.SelectSingleNode(".//a")?.GetAttributeValue("href", string.Empty);

            title = HtmlEntity.DeEntitize(title);
            paragraph = HtmlEntity.DeEntitize(paragraph);
            DateTime? time = datetime != null ? DateTime.Parse(datetime) : null;

            datetime = time?.ToString();
        }

    }
}
