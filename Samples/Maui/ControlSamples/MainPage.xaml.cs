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
            CatanControl.InitalizeBoard(width: 200, height: 200);
            ShapesControl.InitializeBoard(width: 200, height: 200);
            CheckersControl.InitalizeBoard(width: 200, height: 200);
            Platformer2DControl.InitializeSurface(width: 200, height: 200);
            TopDownPlatformerCotrol.InitializeSurface(width: 200, height: 200);
            Flyover3DControl.InitializeSurface(width: 200, height: 200);
        }
    }
}