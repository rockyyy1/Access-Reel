using System.Diagnostics;

namespace AccessReel;
/// <summary>
/// This page is for displaying individual articles e.g. news article
/// </summary>
public partial class ArticlePage : ContentPage
{
	public ArticlePage()
	{
		InitializeComponent();
	}
    
    public ArticlePage(string webpage = "")
    {
        InitializeComponent();
        //Test Data
        FormattedString[] bodyText;
        bodyText = ["Test1", "Test2", "Test3", "Test4"];
        string[] bodyImages = ["Image1", "Image2"]; //replace with ImageSource
        //ImageSource[] bodyImages;

        //Creates labels and images to form the full body of the article
        for(int i = 0;  i < bodyText.Length; i++)
        {
            if (i < bodyImages.Length)
            {
                Image image = new Image();
                image.Source = bodyImages[i];
                ArticleLayout.Add(image);
            }
            if (i < bodyText.Length)
            {
                Label label = new Label();
                label.FormattedText = bodyText[i];
                ArticleLayout.Add(label);
            }
        }
    }
}