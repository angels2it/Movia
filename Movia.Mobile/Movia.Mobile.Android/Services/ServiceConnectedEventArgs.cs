using System;
using Android.OS;

namespace Movia.Mobile.Droid.Services
{
	public class ServiceConnectedEventArgs : EventArgs
	{
		public IBinder Binder { get; set; }
	}
}