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
			Helper.ConnectToDevice ();
		}

		protected override void OnDisappearing ()
		{
			base.OnDisappearing ();
		}

		void ShowText (string status)
		{
			Device.BeginInvokeOnMainThread (() => {
				lState.Text = status;
			});
		}

		private void GoBackChooseDevice ()
		{
			Helper.DisconnectFromDevice ();
			Helper.BleDeviceStateChange -= Helper_BleDeviceStateChange;
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
	}
}