using System.Globalization;

namespace AccessReel;

public partial class ListPage : ContentPage
{
	public ListPage()
	{
		InitializeComponent();
        //Test Data
        Review a = new Review { Author = "Test Author", Date = DateTime.Today, Description="Article Body", ReviewScore="10", Title="Article title"};
        Review b = new Review { Author = "Test Author", Date = DateTime.Today, Description = "Article Body", ReviewScore = "3", Title = "Article title" };
        List<Review> l = new List<Review>();
        l.Add(a);
        l.Add(b);
        CVArticles.ItemsSource = l;
    }
}



//This changes the postion of the label within the circle for the review score
public class ReviewScoreToAbsLayoutConverter : IValueConverter
{
    public Object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if(value is string text)
        {
            if (text.Length == 2)
            {
                return new Rect(0.22, 0.25, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            }
            else
            {
                return new Rect(0.28, 0.25, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
            }
        }
        return new Rect(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize);
        
    }

    public Object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
