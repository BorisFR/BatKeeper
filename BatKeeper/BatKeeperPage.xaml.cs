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
			btOk.Clicked += BtOk_Clicked;
		}

		void BtOk_Clicked (object sender, EventArgs e)
		{
			Navigation.PushModalAsync (new RootPage ());
		}

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

		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			StartTimer ();
			if (Helper.Notificator == null)
				Helper.Notificator = DependencyService.Get<IToastNotificator> ();
		}

		protected override void OnDisappearing ()
		{
			StopTimer ();
			base.OnDisappearing ();
		}

	}
}