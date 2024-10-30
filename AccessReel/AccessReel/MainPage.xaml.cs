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

            foreach ( var node in itemsNode )
            {
                var item = node as HtmlNode;
                string title = item.SelectSingleNode(".//h2[contains(@class, 'gp-loop-title')]//a").GetAttributeValue("title", string.Empty);
                string image = item.SelectSingleNode(".//img").GetAttributeValue("src", string.Empty);

                HtmlNode? paragraphItem = item.SelectSingleNode(".//div[contains(@class, 'gp-loop-text')]//p");
                string paragraph = paragraphItem == null ? "" : paragraphItem.InnerText;

                Debug.WriteLine(title + "\n" + paragraph + "\n\n");
            }
        }

        private void FlyoutMenu_Clicked(object sender, EventArgs e)
        {
            if (Application.Current.MainPage is FlyoutMenu flyoutPage)
            {
                flyoutPage.IsPresented = true;
            }
        }
    }
}
