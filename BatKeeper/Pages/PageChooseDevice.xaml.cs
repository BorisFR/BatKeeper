using System;
using Plugin.Toasts;
using Xamarin.Forms;

namespace BatKeeper
{
	public partial class PageChooseDevice : ContentPage
	{
		public PageChooseDevice ()
		{
			InitializeComponent ();
			NavigationPage.SetHasNavigationBar (this, false);
			if (Helper.GlobalState != GlobalState.ChooseDevice) {
				Helper.GlobalState = GlobalState.ChooseDevice;
				Helper.Navigation.RefreshMenu ();
			}
			/*
			var tapGestureRecognizer = new TapGestureRecognizer ();
			tapGestureRecognizer.Tapped += (s, e) => {
				Navigation.PushModalAsync (new RootPage ());
			};
			imgSplash.GestureRecognizers.Add (tapGestureRecognizer);
			*/
			//btOk.Clicked += BtOk_Clicked;
			btRetry.Clicked += BtRetry_Clicked;
			Helper.BleChanged += Helper_BleChanged;
			Helper.BleSearchEnd += Helper_BleSearchEnd;
			listView.ItemsSource = Helper.DeviceList;
			listView.ItemSelected += ListView_ItemSelected;
			btRetry.IsEnabled = false;
		}

		void ListView_ItemSelected (object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null) return;
			Helper.TheDevice = (BleDevice)e.SelectedItem;
			DeviceIsChoosen ();
		}

		private void DeviceIsChoosen ()
		{
			timerIsRunning = false;
			Helper.BleChanged -= Helper_BleChanged;
			Helper.BleSearchEnd -= Helper_BleSearchEnd;
			//listView.ItemSelected -= ListView_ItemSelected;
			Helper.BleStopSearch ();
			Helper.GlobalState = GlobalState.ConnectToDevice;
			Helper.Navigation.RefreshMenu ();
			Helper.Navigation.NavigateTo (typeof (PageConnectToDevice));
			//Navigation.PushModalAsync (new RootPage (), true);
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
				try {
					lState.Text = status;
				} catch (Exception) { }
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
			if (!timerIsRunning) return false;
			if ((DateTime.Now - lastMessage).TotalSeconds < 1)
				return true;
			timerIsRunning = false;
			Device.BeginInvokeOnMainThread (() => {
				try {
					lState.Text = string.Empty;
				} catch (Exception) { }
			});
			return false;
		}

		/*
		private void BtOk_Clicked (object sender, EventArgs e)
		{
			Navigation.PushModalAsync (new RootPage ());
		}
		*/

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
			base.OnDisappearing ();
		}

	}
}