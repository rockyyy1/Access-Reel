using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace AccessReel;

public partial class ListPage : ContentPage
{
    public List<Review> reviewList { get; set; }
    public List<Posts> postList { get; set; }

    public ListPage() //Used for xaml to prevent error, might be removed later
    {
        InitializeComponent();

        var TapGesture = new TapGestureRecognizer();
        TapGesture.Tapped += OnPageTapped;

        Content.GestureRecognizers.Add(TapGesture);
    }

    public ListPage(string pageType = "")
	{
		InitializeComponent();
        //Test Data
        if(pageType == "Reviews" || pageType == "Films")
        {
            Title = pageType;
            //LblPageTitle.Text = pageType;
            reviewList = new List<Review>();
            Review a = new Review { Author = "Test Author", Date = DateTime.Today, Description = "Film Description", ReviewScore = "10", Title = "Film title" };
            Review b = new Review { Author = "Test Author", Date = DateTime.Today, Description = "Film Description", ReviewScore = "3", Title = "Film title" };
            reviewList.Add(a);
            reviewList.Add(b);
            CVArticles.ItemTemplate = DTMovieArticle;
            CVArticles.ItemsSource = reviewList;
        }
        else
        {
            Title = pageType;
            //LblPageTitle.Text = pageType;
            postList = new List<Posts>();
            Posts c = new Posts { Author = "Test Author", Date = DateTime.Today, Description = "Article Body", Title = "Article title" };
            Posts d = new Posts { Author = "Test Author", Date = DateTime.Today, Description = "Article Body", Title = "Article title" };
            postList.Add(c);
            postList.Add(d);
            CVArticles.ItemTemplate = DTArticle;
            CVArticles.ItemsSource = postList;
        }
    }

    private void BtnFlyoutMenu_Clicked(object sender, EventArgs e)
    {
        if(Application.Current.MainPage is FlyoutMenu flyoutPage)
        {
            flyoutPage.IsPresented = true;
        }        
    }

    private void OnPageTapped(object sender, EventArgs e)
    {
        /*if (Application.Current.MainPage is FlyoutMenu flyoutPage)
        {
            flyoutPage.IsPresented = false;
        }*/
        Debug.WriteLine("Tapped");
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

