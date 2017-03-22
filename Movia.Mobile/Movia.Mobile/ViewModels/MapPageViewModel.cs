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
        private PositionModel _previousPos;
        public CameraModel Camera { get; set; }
        public MapOptionsModel Options { get; set; }
        public bool IsFixedCenter { get; set; }

        public MapPageViewModel(FirebaseClient client)
        {
            _client = client;
            Options = new MapOptionsModel();
        }

        public override void Init()
        {
            base.Init();
            GetCurrentUserInfo().ConfigureAwait(false);
            EventSubcribers();
            FirebaseSubcribers();
        }

        private async Task GetCurrentUserInfo()
        {
            try
            {
                var user = await _client.Child("Users")
                .Child(Settings.UserId)
                .OnceSingleAsync<UserModel>();
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
            if (obj.Position == null)
                return;
            if (_previousPos != null &&
                MapHelpers.GetDistance(_previousPos.Lat, _previousPos.Lng, obj.Position.Lat, obj.Position.Lng) * 1000 < 10)
            {
                _previousPos = obj.Position;
                return;
            }
            _previousPos = obj.Position;
            NotifyMyLocationChanged(new Position(obj.Position.Lat, obj.Position.Lng)).ConfigureAwait(false);
        }

        private async Task NotifyMyLocationChanged(Position position)
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
                ListItems.Add(new PositionModel(user.Id, user.Position, user.Name, user.Icon));
                return;
            }
            item.UpdatePosition(user.Position.Latitude, user.Position.Longitude);
        }
    }
}
