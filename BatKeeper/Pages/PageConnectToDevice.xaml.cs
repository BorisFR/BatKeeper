using System;
using System.Collections.Generic;
using Plugin.BLE.Abstractions;
using Xamarin.Forms;

namespace BatKeeper
{
	public partial class PageConnectToDevice : ContentPage
	{
		public PageConnectToDevice ()
		{
			InitializeComponent ();
			NavigationPage.SetHasNavigationBar (this, false);

			lDevice.Text = $"Device {Helper.TheDevice.Name}";
			ShowText ("Connecting...");
			btCancel.Clicked += BtCancel_Clicked;
			Helper.BleDeviceStateChange += Helper_BleDeviceStateChange;
			Helper.BleDeviceServicesLoaded += Helper_BleDeviceServicesLoaded;
			Helper.ConnectToDevice ();
		}

		protected override void OnDisappearing ()
		{
			Disconnect ();
			base.OnDisappearing ();
		}

		void ShowText (string status)
		{
			Device.BeginInvokeOnMainThread (() => {
				lState.Text = status;
			});
		}

		private void Disconnect ()
		{
			Helper.BleDeviceServicesLoaded -= Helper_BleDeviceServicesLoaded;
			Helper.BleDeviceStateChange -= Helper_BleDeviceStateChange;
			Helper.DisconnectFromDevice ();
		}

		private void GoBackChooseDevice ()
		{
			Disconnect ();
			Helper.GlobalState = GlobalState.ChooseDevice;
			Helper.Navigation.RefreshMenu ();
			Helper.Navigation.NavigateTo (typeof (PageChooseDevice));
		}

		void BtCancel_Clicked (object sender, EventArgs e)
		{
			GoBackChooseDevice ();
		}

		void Helper_BleDeviceStateChange ()
		{
			ShowText ($"State: {Helper.TheDevice.State}");
			if (Helper.TheDevice.State == DeviceState.Disconnected) {
				Helper.DoNotificationInfo ("Device disconnect.");
				GoBackChooseDevice ();
				return;
			}
			if (Helper.TheDevice.State == DeviceState.Connected) {
				Helper.SearchBleServices ();
			}
		}

		void Helper_BleDeviceServicesLoaded ()
		{
			bool found = false;
			ShowText ("Services ready");
			foreach (BleService bs in Helper.TheDevice.AllServices) {
				if (bs.Service.Id.ToString ().Equals ("876167c2-1572-44c4-93bc-f2c6ec50324f")) {
					// on a trouvé notre service :)
					found = true;
				}
			}
			if (!found) {
				GoBackChooseDevice ();
				Helper.DoNotificationError ("This is not a BatKeeper device. Please choose the right device.");
			} else {
				ShowText ("This is a good device.");
			}
		}

	}
}