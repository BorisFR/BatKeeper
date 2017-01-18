using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BatKeeper
{
	public partial class HomePage : ContentPage
	{
		public HomePage ()
		{
			InitializeComponent ();
			NavigationPage.SetHasNavigationBar (this, false);
			Helper.BleDeviceStateChange += Helper_BleDeviceStateChange;
			btDisconnect.Clicked += BtDisconnect_Clicked;

			Helper.DoNotificationInfo ("Hello");
		}

		void Helper_BleDeviceStateChange ()
		{
			Helper.TheCharacteristic = null;
			Helper.BleDeviceStateChange -= Helper_BleDeviceStateChange;
			Helper.GlobalState = GlobalState.ChooseDevice;
			Helper.Navigation.RefreshMenu ();
			Helper.Navigation.NavigateTo (typeof (PageChooseDevice));
		}

		private void BtDisconnect_Clicked (object sender, EventArgs e)
		{
			Helper.BleDisconnectFromDevice ();
		}

	}
}