using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Firebase.Xamarin.Database;
using Firebase.Xamarin.Database.Query;
using Movia.Mobile.Helpers;
using Movia.Mobile.Models;
using Rangstrup.Xam.Plugin.Maps;

namespace Movia.Mobile.Droid.Services
{
    [Service]
    public class LocationService : Service, ILocationListener
    {
        public event EventHandler<LocationChangedEventArgs> LocationChanged = delegate { };
        public event EventHandler<ProviderDisabledEventArgs> ProviderDisabled = delegate { };
        public event EventHandler<ProviderEnabledEventArgs> ProviderEnabled = delegate { };
        public event EventHandler<StatusChangedEventArgs> StatusChanged = delegate { };

        public string UserId
        {
            get
            {
                using (var sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext))
                {
                    return sharedPreferences.GetString(Settings.UserIdKey, Convert.ToString(string.Empty));
                }
            }
        }

        // Set our location manager as the system location service
        protected readonly LocationManager LocMgr = Application.Context.GetSystemService("location") as LocationManager;

        readonly string logTag = "LocationService";
        IBinder binder;

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Debug(logTag, "OnCreate called in the Location Service");
        }

        // This gets called when StartService is called in our App class
        [Obsolete("deprecated in base class")]
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug(logTag, "LocationService started");
            StartLocationUpdates();
            return StartCommandResult.Sticky;
        }

        // This gets called once, the first time any client bind to the Service
        // and returns an instance of the LocationServiceBinder. All future clients will
        // reuse the same instance of the binder
        public override IBinder OnBind(Intent intent)
        {
            Log.Debug(logTag, "Client now bound to service");
            binder = new LocationServiceBinder(this);
            return binder;
        }

        // Handle location updates from the location manager
        public void StartLocationUpdates()
        {
            //we can set different location criteria based on requirements for our app -
            //for example, we might want to preserve power, or get extreme accuracy
            var locationCriteria = new Criteria();
            locationCriteria.Accuracy = Accuracy.NoRequirement;
            locationCriteria.PowerRequirement = Power.NoRequirement;

            // get provider: GPS, Network, etc.
            var locationProvider = LocMgr.GetBestProvider(locationCriteria, true);
            Log.Debug(logTag, $"You are about to get location updates via {locationProvider}");

            // Get an initial fix on location
            LocMgr.RequestLocationUpdates(locationProvider, 2000, 20, this);

            Log.Debug(logTag, "Now sending location updates");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Debug(logTag, "Service has been terminated");

            // Stop getting updates from the location manager:
            LocMgr.RemoveUpdates(this);
        }

        #region ILocationListener implementation
        // ILocationListener is a way for the Service to subscribe for updates
        // from the System location Service

        public void OnLocationChanged(Android.Locations.Location location)
        {
            LocationChanged(this, new LocationChangedEventArgs(location));
            Log.Debug(logTag, "Location changed");
            UpdateUserLocation(new UserPositionModel(location.Latitude, location.Longitude, DateTime.UtcNow));
        }
        UserPositionModel _previousPos;
        private DateTime _lastTimeSyncPosition = default(DateTime);
        private FirebaseClient _client = new FirebaseClient("https://oh-my-beer.firebaseio.com/movia");
        private async Task UpdateUserLocation(UserPositionModel position)
        {
            if (_previousPos != null &&
                MapHelpers.GetDistance(_previousPos.Latitude, _previousPos.Longitude, position.Latitude, position.Longitude) * 1000 < 10
                && _lastTimeSyncPosition != default(DateTime) && DateTime.Now.Subtract(_lastTimeSyncPosition).TotalMinutes < Settings.UpdatePositionInverval)
            {
                return;
            }
            _previousPos = position;
            _lastTimeSyncPosition  = DateTime.Now;
            try
            {
                Log.Debug(logTag, UserId);
                Log.Debug(logTag, "Position", position);
                await _client.Child("Users")
                    .Child(UserId)
                    .Child("Position")
                    .PatchAsync(position);
                Log.Debug(logTag, "Save position successfully");
            }
            catch (Exception e)
            {
                Log.Debug(logTag, "Save position error");
                Log.Debug(logTag, e.Message);
                Log.Debug(logTag, e.StackTrace);
                Log.Debug(logTag, e.Message, e);
            }
        }

        public void OnProviderDisabled(string provider)
        {
            this.ProviderDisabled(this, new ProviderDisabledEventArgs(provider));
        }

        public void OnProviderEnabled(string provider)
        {
            this.ProviderEnabled(this, new ProviderEnabledEventArgs(provider));
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            this.StatusChanged(this, new StatusChangedEventArgs(provider, status, extras));
        }

        #endregion

    }
}

