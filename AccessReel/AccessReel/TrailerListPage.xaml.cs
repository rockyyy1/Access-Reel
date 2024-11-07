using HtmlAgilityPack;
using System.Diagnostics;
using System.Security.AccessControl;

namespace AccessReel;

public partial class TrailerListPage : ContentPage
{
	public TrailerListPage()
	{
		InitializeComponent();

		List<Posts> trailers = new List<Posts>();
        var url = "https://accessreel.com/categories/trailers/";
        var web = new HtmlWeb();
        var document = web.Load(url);

        var containers = document.DocumentNode.SelectNodes("//div[contains(@class, 'gp-blog-wrapper')]");

        if (containers != null)
        {
            foreach (var container in containers)
            {
                var postNodes = container.SelectNodes(".//section[contains(@class, 'gp-post-item')]");

                if (postNodes != null)
                {
                    foreach (var node in postNodes)
                    {
                        // Extract the URL - note this will lead to .com/article/movie-title-trailer
                        var urlNode = node.SelectSingleNode(".//a");
                        var postUrl = urlNode.GetAttributeValue("href", string.Empty);

                        // Extract the title text
                        var titleNode = node.SelectSingleNode(".//h2[@class='gp-loop-title']/a");
                        var postTitle = titleNode?.InnerText.Trim();
                        postTitle = HtmlEntity.DeEntitize(postTitle);

                        // Extract the image source 
                        var metaImageNode = node.SelectSingleNode(".//div[@itemprop='image']/meta[@itemprop='url']");
                        var postImage = metaImageNode?.GetAttributeValue("content", string.Empty);

                        Posts t = new Review() { Title = postTitle, Url = postUrl, Image = postImage };
                        trailers.Add(t);
                        //Debug.WriteLine($"URL: {postUrl}");
                        //Debug.WriteLine($"Title: {postTitle}");
                        //Debug.WriteLine($"Image Src: {postImage}");
                    }
                }
            }
        }
            CVTrailers.ItemsSource = trailers;

    }

    private async void FilmTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label || sender is Image image)
        {
            // Access the DataContext of the Label
            var film = (Review)((VisualElement)sender).BindingContext;

            // Debug 
            Debug.WriteLine(film.Title);
            Debug.WriteLine(film.Url);

            //make a page - work in progress
            //FilmPage newFilm = new FilmPage(film); - Doesn't work properly 

            //await Navigation.PushAsync();

        }
    }

    //input: https://accessreel.com/article/saturday-night-trailer/
    //returns: https://accessreel.com/saturday-night
    // might use, might not we'll see
    static string ConvertUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return string.Empty;

        Uri uri = new Uri(url);
        string path = uri.AbsolutePath;

        // Remove the '/article/' part and the '-trailer/' suffix
        var title = path.Replace("/article/", "").Replace("-trailer/", "");

        // Rebuild the URL (only the domain and title)
        string newUrl = $"{uri.Scheme}://{uri.Host}/{title}";
        return newUrl;
    }


}