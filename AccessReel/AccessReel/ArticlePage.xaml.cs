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
        //var ArticleBody = bodyText.Zip(bodyImages, (t, i) => new { text = t, image = i});
        //ImageSource[] bodyImages;

        /*foreach(var ti in ArticleBody)
        {
            Label label = new Label();
            label.FormattedText = ti.text;
            ArticleLayout.Add(label);

            //Replace with Image
            Label label2 = new Label { Text = ti.image };
            ArticleLayout.Add(label2);

            Image image = new Image();
            image.Source = ti.image;
            ArticleLayout.Add(image);
        }*/
        for(int i = 0;  i < bodyText.Length; i++)
        {
            if (bodyImages[i] != null)
            {
                Image image = new Image();
                image.Source = bodyImages[i];
                ArticleLayout.Add(image);
            }
            if (bodyText[i] != null)
            {
                Label label = new Label();
                //label.FormattedText = ti.text;
                ArticleLayout.Add(label);
            }
        }
    }
}