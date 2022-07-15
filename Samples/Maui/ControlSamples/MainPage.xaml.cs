namespace ControlSamples
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);


            // init
            CatanControl.InitalizeBoard(width: 500, height: 500);
        }
    }
}