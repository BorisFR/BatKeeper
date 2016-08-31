using Xamarin.Forms;
using System.Diagnostics;
using System;

namespace BatKeeper
{
	public partial class App : Application
	{
		public App ()
		{
			Debug.WriteLine ("Create App");
			InitializeComponent ();

			//if (((Guid)Helper.SettingsRead ("PreferedDeviceId", Guid.Empty)).Equals (Guid.Empty))
			//	Helper.SettingsSave ("PreferedDeviceId", Guid.Parse ("7CA11001-EC1B-49C7-ABE2-671597A51252"));
			Helper.PreferedGuidDevice = (Guid)Helper.SettingsRead ("PreferedDeviceId", Guid.Empty);
			Debug.WriteLine ($"Prefered device ID: {Helper.PreferedGuidDevice}");

			MainPage = new RootPage ();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
			Debug.WriteLine ("OnStart");
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
			Debug.WriteLine ("OnSleep");
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
			Debug.WriteLine ("OnResume");
		}
	}
}