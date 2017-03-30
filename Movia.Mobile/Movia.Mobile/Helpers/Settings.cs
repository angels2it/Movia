using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Movia.Mobile.Helpers
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        #region Setting Constants

        private const string IsAuthKey = "isAuth_key";
        public const string UserIdKey = "userId_key";
        private const string IsSendLocationKey = "isSendLocation_key";

        #endregion


        public static bool IsAuth
        {
            get
            {
                return AppSettings.GetValueOrDefault(IsAuthKey, false);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IsAuthKey, value);
            }
        }

        public const string FirebaseUrl = "https://oh-my-beer.firebaseio.com/movia";
        public static string UserId
        {
            get
            {
                return AppSettings.GetValueOrDefault(UserIdKey, string.Empty);
            }
            set
            {
                AppSettings.AddOrUpdateValue(UserIdKey, value);
            }
        }

        public static bool IsSendLocation
        {
            get
            {
                return AppSettings.GetValueOrDefault(IsSendLocationKey, true);
            }
            set
            {
                AppSettings.AddOrUpdateValue(IsSendLocationKey, value);
            }
        }

        public static double UpdatePositionInverval = 3;

        public const double OnlineThreshold = 3;
    }
}