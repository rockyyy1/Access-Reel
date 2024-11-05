namespace AccessReel;

public partial class TrailerListPage : ContentPage
{
	public TrailerListPage()
	{
		InitializeComponent();

		List<Posts> posts = new List<Posts>();
		Posts a = new Posts() { Title = "Film Title"};
		posts.Add(a);
		posts.Add(a);
		posts.Add(a);
		posts.Add(a);
		posts.Add(a);
		posts.Add(a);
		posts.Add(a);
		posts.Add(a);
		posts.Add(a);
		posts.Add(a);
		posts.Add(a);
		CVTrailers.ItemsSource = posts;
	}
}