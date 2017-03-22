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
        public string Username { get; set; }
        public string Password { get; set; }
        public CommandTrackable Start { get; set; }

        public LoginPageViewModel(INavigator navigator, IUserDialogs dialogs, IAccountService accountService, FirebaseClient client)
        {
            _navigator = navigator;
            _dialogs = dialogs;
            _accountService = accountService;
            _client = client;
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
            LoginTask().ConfigureAwait(false);
        }

        private async Task LoginTask()
        {
            if (UserAndPassNotValid())
            {
                OnLoginError();
                return;
            }
            LoginModelResult result = await _accountService.Login(new LoginModel()
            {
                Username = Username,
                Password = Password
            });
            if (!result.Ok)
            {
                OnLoginError();
                return;
            }
            await OnLoginSuccess(result);
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
            await _navigator.PushModalAsync<MapPageViewModel>();
        }

        private bool UserAndPassNotValid()
        {
            return string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password);
        }
    }
}