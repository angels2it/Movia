using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acr.UserDialogs;
using Autofac;
using Firebase.Xamarin.Database;
using Rangstrup.Xam.Plugin.Mvvm.Views;
using Xamarin.Forms;
using Movia.Mobile.Helpers;
using Movia.Mobile.Services;
using Movia.Mobile.ViewModels;
using Movia.Mobile.Views;
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
#if DEBUG
        public bool IsAuth => false;
#else
        public bool IsAuth => Settings.IsAuth;
#endif

        public override void ConfigApplication(IContainer container)
        {

        }

        public override void RegisterModule(ContainerBuilder builder)
        {

        }

        public override void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<AccountService>().AsImplementedInterfaces();
            builder.RegisterInstance(new FirebaseClient(Settings.FirebaseUrl)).SingleInstance();
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

        private void OnAuthFlow(IViewFactory viewFactory)
        {
            MainPage = viewFactory.Resolve<MapPageViewModel>();
        }
    }
}
