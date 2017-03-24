using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Firebase.Xamarin.Database;
using Firebase.Xamarin.Database.Query;
using Movia.Mobile.Helpers;
using Movia.Mobile.Models;
using Movia.Mobile.Services;
using PropertyChanged;
using Rangstrup.Xam.Plugin.Analytics;
using Rangstrup.Xam.Plugin.Mvvm;
using Rangstrup.Xam.Plugin.Mvvm.ViewModels;
using Rangstrup.Xam.Plugin.Mvvm.Views;
using Xamarin.Forms;

namespace Movia.Mobile.ViewModels
{
    [ImplementPropertyChanged]
    public class LoginPageViewModel : ViewModelBase
    {
        private readonly INavigator _navigator;
        private readonly IUserDialogs _dialogs;
        private readonly IAccountService _accountService;
        private readonly FirebaseClient _client;
        private readonly IViewFactory _viewFactory;
        public string Username { get; set; }
        public string Password { get; set; }
        public CommandTrackable Start { get; set; }

        public LoginPageViewModel(INavigator navigator, IUserDialogs dialogs, IAccountService accountService, FirebaseClient client, IViewFactory viewFactory)
        {
            _navigator = navigator;
            _dialogs = dialogs;
            _accountService = accountService;
            _client = client;
            _viewFactory = viewFactory;
            InitCommands();
        }

        public override void Init()
        {
            base.Init();
            ResetInput();
#if DEBUG
            Username = "U1";
            Password = "Pass1";
#endif
        }

        private void InitCommands()
        {
            Start = new CommandTrackable("Start", OnStartExecute);
        }

        private void OnStartExecute()
        {
            if (UserAndPassNotValid())
            {
                OnLoginError();
                return;
            }
            UiLoading();
            LoginTask().ConfigureAwait(false);
        }

        private void UiLoading()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _dialogs.ShowLoading();
            });
        }

        private async Task LoginTask()
        {
            LoginModelResult result = await _accountService.Login(new LoginModel()
            {
                Username = Username,
                Password = Password
            });
            if (!result.Ok)
            {
                UiHideLoading();
                OnLoginError();
                return;
            }
            await OnLoginSuccess(result);
        }

        private void UiHideLoading()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _dialogs.HideLoading();
            });
        }

        private void OnLoginError()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ResetInput();
                _dialogs.Alert("Forkert brugernavn eller adgangskode");
            });
        }

        private void ResetInput()
        {
            Username = Password = string.Empty;
        }

        private async Task OnLoginSuccess(LoginModelResult result)
        {
            var user = _accountService.GetUserById(result.Id);
            if (user == null)
            {
                _dialogs.Alert("Error");
                return;
            }
            Settings.IsAuth = true;
            Settings.UserId = user.Id;
            try
            {
                await _client.Child("Users")
                .Child(Settings.UserId)
                .PutAsync(new UserModel()
                {
                    Id = result.Id,
                    Name = user.Username,
                    Icon = user.Icon
                });
            }
            catch (Exception e)
            {
                
            }
            UiHideLoading();
            App.Current.OnAuthFlow(_viewFactory);
        }

        private bool UserAndPassNotValid()
        {
            return string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password);
        }
    }
}