using System;
using Plugin.Toasts;
using Xamarin.Forms;

namespace BatKeeper
{
	public partial class BatKeeperPage : ContentPage
	{
		public BatKeeperPage ()
		{
			InitializeComponent ();
			var tapGestureRecognizer = new TapGestureRecognizer ();
			tapGestureRecognizer.Tapped += (s, e) => {
				Navigation.PushModalAsync (new RootPage ());
			};
			imgSplash.GestureRecognizers.Add (tapGestureRecognizer);
			//btOk.Clicked += BtOk_Clicked;
			btRetry.Clicked += BtRetry_Clicked;
			Helper.BleChanged += Helper_BleChanged;
			Helper.BleSearchEnd += Helper_BleSearchEnd;
			listView.ItemsSource = Helper.DeviceList;
			btRetry.IsEnabled = false;
		}

		void Helper_BleSearchEnd ()
		{
			btRetry.IsEnabled = true;
		}

		DateTime lastMessage = DateTime.Now;
		bool timerIsRunning = false;

		void Helper_BleChanged (string status)
		{
			Device.BeginInvokeOnMainThread (() => {
				lState.Text = status;
			});
			StartTimer ();
		}

		private void StartTimer ()
		{
			if (!timerIsRunning) {
				timerIsRunning = true;
				lastMessage = DateTime.Now;
				Device.StartTimer (new TimeSpan (0, 0, 0, 1), ClearMessage);
			}
		}

		private bool ClearMessage ()
		{
			if ((DateTime.Now - lastMessage).TotalSeconds < 1)
				return true;
			timerIsRunning = false;
			Device.BeginInvokeOnMainThread (() => {
				lState.Text = string.Empty;
			});
			return false;
		}

		private void BtOk_Clicked (object sender, EventArgs e)
		{
			Navigation.PushModalAsync (new RootPage ());
		}

		private void BtRetry_Clicked (object sender, EventArgs e)
		{
			btRetry.IsEnabled = false;
			Helper.BleInit ();
		}

		/*
		private bool doAnimation = false;
		private int animationState = 0;

		private bool DoAnimation ()
		{
			switch (animationState) {
			case 0:
				touch1.IsVisible = true;
				touch2.IsVisible = false;
				animationState++;
				break;
			case 1:
				touch1.IsVisible = false;
				touch2.IsVisible = true;
				animationState = 0;
				break;
			default:
				animationState = 0;
				break;
			}
			return doAnimation;
		}

		private void StartTimer ()
		{
			doAnimation = true;
			Device.StartTimer (new TimeSpan (0, 0, 0, 0, 300), DoAnimation);
		}

		private void StopTimer ()
		{
			doAnimation = false;
		}
		*/

		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			//StartTimer ();
			Helper.NotificatorInit ();
			Helper.BleInit ();
		}

		protected override void OnDisappearing ()
		{
			//StopTimer ();
			Helper.BleChanged -= Helper_BleChanged;
			base.OnDisappearing ();
		}

	}
}