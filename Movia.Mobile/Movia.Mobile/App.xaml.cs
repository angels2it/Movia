using Acr.UserDialogs;
using Autofac;
using Firebase.Xamarin.Database;
using Rangstrup.Xam.Plugin.Mvvm.Views;
using Xamarin.Forms;
using Movia.Mobile.Helpers;
using Movia.Mobile.Services;
using Movia.Mobile.ViewModels;
using Movia.Mobile.Views;
using Plugin.VersionTracking;
using Rangstrup.Xam.Plugin.Mvvm.Autofac;

namespace Movia.Mobile
{
    public partial class App : Rangstrup.Xam.Plugin.Mvvm.App
    {
        public App()
        {
            InitializeComponent();
            new Bootstrapper(this).Run();
        }
        public new static App Current => (App)Application.Current;

        public bool IsAuth
        {
            get
            {
                if (CrossVersionTracking.Current.IsFirstLaunchForBuild) return false;
                return Settings.IsAuth;
            }
        }

        private FirebaseClient _client;
        public override void ConfigApplication(IContainer container)
        {

        }

        public override void RegisterModule(ContainerBuilder builder)
        {

        }

        public override void RegisterServices(ContainerBuilder builder)
        {
            var fl = DependencyService.Get<IFormsLocationService>();
            builder.RegisterInstance(fl);
            builder.RegisterType<AccountService>().AsImplementedInterfaces();

            _client = new FirebaseClient(Settings.FirebaseUrl);
            builder.RegisterInstance(_client).SingleInstance();
            builder.RegisterInstance(UserDialogs.Instance);
        }

        public override void RegisterViews(IViewFactory viewFactory)
        {
            viewFactory.Register<LoginPageViewModel, LoginPage>();
            viewFactory.Register<MapPageViewModel, MapPage>();
        }

        public override void StartApp(IViewFactory viewFactory)
        {
            if (IsAuth)
                OnAuthFlow(viewFactory);
            else
                OnUnAuthFlow(viewFactory);
        }

        private void OnUnAuthFlow(IViewFactory viewFactory)
        {
            MainPage = viewFactory.Resolve<LoginPageViewModel>();
        }

        public void OnAuthFlow(IViewFactory viewFactory)
        {
            MainPage = viewFactory.Resolve<MapPageViewModel>();
        }
    }
}
