using HtmlAgilityPack;
using System.Diagnostics;
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

            //retrieve list of blog posts
            HtmlNode itemsNode = document.DocumentNode.SelectSingleNode("//div[contains(@class, 'wpb_wrapper')]");
            var wrapperNodes = itemsNode.SelectNodes("//div[contains(@class, 'gp-blog-wrapper')]");
            foreach (HtmlNode item in wrapperNodes)
            {
                
            var productHTMLElement = item.SelectSingleNode("//section[contains(@class, 'gp-post-item')]");

            var title = productHTMLElement.SelectNodes(".//h2[contains(@class, 'gp-loop-title')]");
            foreach (var details in title)
            {
                Debug.WriteLine(details.InnerText);
                Debug.WriteLine(details.GetAttributeValue("href", string.Empty));
            } 
            }

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
