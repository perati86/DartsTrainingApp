using banditoth.MAUI.MVVM.Entities;
using DartsApp.ViewModels;

namespace DartsApp
{
    public partial class MainPage : BaseView
    {
        public MainPage(MainViewModel vm)
        {
            InitializeComponent();

            BindingContext = vm;
        }
    }

}
