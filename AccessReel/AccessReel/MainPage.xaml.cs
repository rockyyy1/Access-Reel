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

        private string ReadWebsite()
        {
            
            var web = new HtmlWeb();
            text = web.Load("https://accessreel.com/").Text;
            //Debug.WriteLine(text);
            
            return text;
        }

        private void Retrieve(string text)
        {
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(text);

            var wpd_wrapper = document.DocumentNode.SelectSingleNode("/html/body/div[3]/div[2]/div[3]/div[1]/div[2]/div[1]/div/div");
            //retrieve list of blog posts
            HtmlNodeCollection itemsNode = wpd_wrapper.SelectNodes("//section[contains(@class, 'gp-post-item') and contains(@class, 'type-article')]");
            // var wrapperNodes = itemsNode.SelectNodes("//div[contains(@class, 'gp-post-item')]");

            foreach ( var node in itemsNode )
            {
                var item = node as HtmlNode;
                string title = item.SelectSingleNode(".//h2[contains(@class, 'gp-loop-title')]//a").GetAttributeValue("title", string.Empty);
                string image = item.SelectSingleNode(".//img").GetAttributeValue("src", string.Empty);

                HtmlNode? paragraphItem = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-text')]//p");
                string paragraph = paragraphItem == null ? "" : paragraphItem.InnerText;

                Debug.WriteLine(title + "\n" + paragraph + "\n\n");
            }

            // edited out for later / testing
            /*foreach (HtmlNode item in wrapperNodes)
            {

                //var productHTMLElement = item.SelectSingleNode("//section[contains(@class, 'gp-post-item')]");

                var title = item
            // var title = wrapperNodes.(".//h2[contains(@class, 'gp-loop-title')]");
            foreach (var details in title)
            {
                Debug.WriteLine(details.InnerText);
                Debug.WriteLine(details.GetAttributeValue("href", string.Empty));
            } 
           /* }

            //var title_link = title.GetAttributes(".//a[contains(@class, 'gp-loop-title')]").InnerText);
            //var image = HtmlEntity.DeEntitize(productHTMLElement.SelectSingleNode(".//img").Attributes["src"].Value);
            //var text = HtmlEntity.DeEntitize(productHTMLElement.SelectSingleNode(".//p").InnerText);
            //var time = HtmlEntity.DeEntitize(productHTMLElement.SelectSingleNode(".//time").InnerText);

           /* Debug.WriteLine("title: " + title +*/
           /*     "image: " + image +*/
           /*     "text: " + text +*/
           /*     "time: " + time);*/
/**/
        }
    }

}
