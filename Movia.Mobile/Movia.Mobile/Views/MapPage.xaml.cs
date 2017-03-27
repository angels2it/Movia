using Movia.Mobile.Helpers;
using Movia.Mobile.Services;
using Movia.Mobile.ViewModels;
using Rangstrup.Xam.Plugin.Mvvm.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Movia.Mobile.Views
{
    public partial class MapPage : ViewBase
    {
        public MapPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Switch.IsToggled = Settings.IsSendLocation;
        }

        private void Switch_OnToggled(object sender, ToggledEventArgs e)
        {
            var model = Model as MapPageViewModel;
            model?.SendLocationToServerChanged(e.Value);
        }
    }
}
