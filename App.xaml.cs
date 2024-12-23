
using banditoth.MAUI.MVVM.Interfaces;
using DartsApp.ViewModels;

namespace DartsApp
{
    public partial class App : Application
    {
        public App(MainViewModel vm, INavigator navigator)
        {
            InitializeComponent();

            MainPage = new NavigationPage(navigator.GetInstance<MainViewModel>());
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            const int newWidth = 450;
            const int newHeight = 850;

            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                window.Width = newWidth;
                window.Height = newHeight;

                window.X = 0;
                window.Y = 0;
            }
            return window;
        }
    }
}
