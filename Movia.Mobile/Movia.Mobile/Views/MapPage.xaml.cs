using Movia.Mobile.Helpers;
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
            Settings.IsSendLocation = e.Value;
        }
    }
}
