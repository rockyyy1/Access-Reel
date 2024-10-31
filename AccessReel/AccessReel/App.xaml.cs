namespace AccessReel
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            
            MainPage = new FlyoutMenu();//new AppShell();
        }
    }
}
