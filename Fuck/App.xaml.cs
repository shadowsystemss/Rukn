[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Fuck
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;
        public App(IServiceProvider services)
        {
            _services = services;
            InitializeComponent();
        }

        public void SetPage<T>() where T : Page => MainPage = _services.GetRequiredService<T>();
        public void SetPage<T>(T page) where T : Page => MainPage = page;

        protected override Window CreateWindow(IActivationState? activationState)
        {
            if (this.MainPage is null)
                return base.CreateWindow(activationState);

            Window window = base.CreateWindow(activationState);

            window.Width = 400;
            window.Height = 650;

            return window;
        }
    }
}
