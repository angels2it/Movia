using System;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Xamarin.Database;
using Firebase.Xamarin.Database.Query;
using Firebase.Xamarin.Database.Streaming;
using Movia.Mobile.Helpers;
using Movia.Mobile.Models;
using PropertyChanged;
using Rangstrup.Xam.Plugin.Maps;
using Rangstrup.Xam.Plugin.Maps.Events;
using Rangstrup.Xam.Plugin.Maps.Models;
using Rangstrup.Xam.Plugin.Mvvm.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Movia.Mobile.ViewModels
{
    [ImplementPropertyChanged]
    public class MapPageViewModel : ViewModelBase<PositionModel>
    {
        readonly FirebaseClient _client;
        private PositionModel _lastUpdatedPos;
        public CameraModel Camera { get; set; }
        public MapOptionsModel Options { get; set; }
        public bool IsFixedCenter { get; set; }
        public PositionModel MyPosition { get; set; }

        public MapPageViewModel(FirebaseClient client)
        {
            _client = client;
            Options = new MapOptionsModel();
        }

        public override void Init()
        {
            base.Init();
            IsFixedCenter = true;
            GetCurrentUserInfo().ConfigureAwait(false);
            GetUsers().ConfigureAwait(false);
            EventSubcribers();
        }

        private async Task GetUsers()
        {
            try
            {
                var users = await _client.Child("Users")
                 .OnceAsync<UserModel>();
                foreach (var user in users)
                {
                    if (user.Key == Settings.UserId)
                        continue;
                    UiUpdateUserPosition(user.Object);
                }
            }
            catch (Exception e)
            {
            }
            FirebaseSubcribers();
        }

        private async Task GetCurrentUserInfo()
        {
            try
            {
                var user = await _client.Child("Users")
                .Child(Settings.UserId)
                .OnceSingleAsync<UserModel>();
                Options = new MapOptionsModel()
                {
                    MyPositionOptions = new PositionOptionsModel()
                    {
                        Icon = user.Icon,
                        Text = "My location"
                    }
                };
                if (MyPosition != null)
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Camera = new CameraModel()
                        {
                            Zoom = 16,
                            Target = MyPosition.Position
                        };
                    });
            }
            catch (Exception e)
            {
            }
        }

        private void EventSubcribers()
        {
            MessagingCenter.Subscribe<CurrentPositionChangedEvent>(this, "CurrentPositionChangedEvent", OnCurrentPositionChanged);
        }

        private void OnCurrentPositionChanged(CurrentPositionChangedEvent obj)
        {
            if (obj.Position == null || !Settings.IsSendLocation)
                return;
            if (_lastUpdatedPos != null &&
                MapHelpers.GetDistance(_lastUpdatedPos.Lat, _lastUpdatedPos.Lng, obj.Position.Lat, obj.Position.Lng) * 1000 < 10)
            {
                return;
            }
            _lastUpdatedPos = obj.Position;
            NotifyMyLocationChanged(new UserPositionModel(obj.Position.Lat, obj.Position.Lng, DateTime.UtcNow)).ConfigureAwait(false);
        }

        private async Task NotifyMyLocationChanged(UserPositionModel position)
        {
            await _client.Child("Users")
                .Child(Settings.UserId)
                .Child("Position")
                .PatchAsync(position);
        }

        private void FirebaseSubcribers()
        {
            _client.Child("Users")
                .AsObservable<UserModel>()
                .Subscribe(OnUsersChanged);
        }

        private void OnUsersChanged(FirebaseEvent<UserModel> @event)
        {
            try
            {
                if (@event.Key == Settings.UserId)
                    return;
                Device.BeginInvokeOnMainThread(() => UiUpdateUserPosition(@event.Object));
            }
            catch (Exception e)
            {
            }
        }

        private void UiUpdateUserPosition(UserModel user)
        {
            var item = ListItems.FirstOrDefault(e => e.Index == user.Id);
            if (item == null)
            {
                ListItems.Add(new PositionModel(user.Id, user.Position.Latitude, user.Position.Longitude, user.Name, user.Icon));
                return;
            }
            item.UpdatePosition(user.Position.Latitude, user.Position.Longitude);
        }
    }
}
