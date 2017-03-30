using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Xamarin.Database;
using Firebase.Xamarin.Database.Query;
using Firebase.Xamarin.Database.Streaming;
using Movia.Mobile.Helpers;
using Movia.Mobile.Models;
using Movia.Mobile.Services;
using PropertyChanged;
using Rangstrup.Xam.Plugin.Maps;
using Rangstrup.Xam.Plugin.Maps.Events;
using Rangstrup.Xam.Plugin.Maps.Models;
using Rangstrup.Xam.Plugin.Mvvm.ViewModels;
using Xamarin.Forms;

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
        CancellationTokenSource _reloadTokenSource;
        private IDisposable _usersSubcribing;

        public MapPageViewModel(FirebaseClient client)
        {
            _client = client;
            Options = new MapOptionsModel();
        }

        public override void Init()
        {
            base.Init();
            try
            {
                _reloadTokenSource = new CancellationTokenSource();
                _usersSubcribing?.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
            IsFixedCenter = true;
            GetCurrentUserInfo().ConfigureAwait(false);
            GetUsers().ContinueWith(r =>
            {
                // start reload task
                StartReloadTask(_reloadTokenSource.Token).ConfigureAwait(false);
            }).ConfigureAwait(false);
            EventSubcribers();
        }

        public override void OnRelease()
        {
            CancelToken();
            base.OnRelease();
        }

        private void CancelToken()
        {
            try
            {
                _reloadTokenSource.Cancel();
                _reloadTokenSource.Dispose();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private async Task StartReloadTask(CancellationToken token)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), token);
                if (token.IsCancellationRequested)
                    return;
                await GetUserAndContinueReloadTask(token);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private async Task GetUserAndContinueReloadTask(CancellationToken token)
        {
            await GetUsers();
            // start reload task
            StartReloadTask(token).ConfigureAwait(false);
        }

        public void SendLocationToServerChanged(bool isSend)
        {
            Settings.IsSendLocation = isSend;
            if (isSend)
            {
                if (MyPosition != null)
                    NotifyMyLocationChanged(new UserPositionModel(MyPosition.Lat, MyPosition.Lng, DateTime.UtcNow)).ConfigureAwait(false);
                return;
            }
            DependencyService.Get<IFormsLocationService>().StopLocationService();
            // hide user position
            NotifyMyLocationChanged(new UserPositionModel(MyPosition.Lat, MyPosition.Lng, default(DateTime))).ConfigureAwait(false);
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
                    if (DateTime.UtcNow.Subtract(user.Object.Position.UpdatedAt).TotalMinutes > Settings.OnlineThreshold)
                        continue;
                    UiUpdateUserPosition(user.Object);
                }
            }
            catch (Exception)
            {
                // ignored
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
                Options = new MapOptionsModel
                {
                    MyPositionOptions = new PositionOptionsModel
                    {
                        Icon = user.Icon,
                        Text = "My location"
                    }
                };
                if (MyPosition != null)
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Camera = new CameraModel
                        {
                            Zoom = 16,
                            Target = MyPosition.Position
                        };
                    });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void EventSubcribers()
        {
            MessagingCenter.Subscribe<CurrentPositionChangedEvent>(this, "CurrentPositionChangedEvent", OnCurrentPositionChanged);
        }
        private DateTime _lastTimeSyncPosition = default(DateTime);
        private void OnCurrentPositionChanged(CurrentPositionChangedEvent obj)
        {
            if (obj.Position == null || !Settings.IsSendLocation)
                return;
            if (_lastUpdatedPos != null &&
                MapHelpers.GetDistance(_lastUpdatedPos.Lat, _lastUpdatedPos.Lng, obj.Position.Lat, obj.Position.Lng) * 1000 < 10
                && _lastTimeSyncPosition != default(DateTime) && DateTime.Now.Subtract(_lastTimeSyncPosition).TotalMinutes < Settings.UpdatePositionInverval)
            {
                return;
            }
            _lastUpdatedPos = obj.Position;
            _lastTimeSyncPosition = DateTime.Now;
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
            _usersSubcribing = _client.Child("Users")
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
            catch (Exception)
            {
                // ignored
            }
        }

        private void UiUpdateUserPosition(UserModel user)
        {
            if (user?.Position == null)
                return;
            bool isShowOnMap = DateTime.UtcNow.Subtract(user.Position.UpdatedAt).TotalMinutes <= Settings.OnlineThreshold;
            var item = ListItems.FirstOrDefault(e => e.Index == user.Id);
            if (item == null)
            {
                if (isShowOnMap)
                    ListItems.Add(new PositionModel(user.Id, user.Position.Latitude, user.Position.Longitude, user.Name, user.Icon));
                return;
            }
            if (isShowOnMap)
                item.UpdatePosition(user.Position.Latitude, user.Position.Longitude);
            else
                ListItems.Remove(item);
        }
    }
}
