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
			btOk.Clicked += BtOk_Clicked;
			btOk.IsEnabled = false;
			eCode.TextChanged += ECode_TextChanged;
			Helper.ConnectToDevice ();
		}

		protected override void OnDisappearing ()
		{
			if (Helper.TheCharacteristic == null)
				Disconnect ();
			base.OnDisappearing ();
		}

		void ShowText (string status)
		{
			Device.BeginInvokeOnMainThread (() => {
				lState.Text = status;
			});
		}

		private async void Disconnect ()
		{
			if (Helper.TheCharacteristic != null) {
				await Helper.TheCharacteristic.Characteristic.StopUpdatesAsync ();
				//Helper.TheCharacteristic.Characteristic.StopUpdates ();
				Helper.TheCharacteristic.Characteristic.ValueUpdated -= GotAnswer;
				Helper.TheCharacteristic = null;
			}
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

		void BtOk_Clicked (object sender, EventArgs e)
		{
			btOk.IsEnabled = false;
			try {
				Helper.BleAuth = Convert.ToInt32 (eCode.Text.Trim ());
				DoSendCodeAuth ();
			} catch (Exception err) {
				btOk.IsEnabled = true;
			}
		}

		void ECode_TextChanged (object sender, TextChangedEventArgs e)
		{
			if (eCode.Text.Trim ().Length == 3) {
				try {
					int code = Convert.ToInt32 (eCode.Text.Trim ());
					btOk.IsEnabled = true;
				} catch (Exception err) {
					btOk.IsEnabled = false;
				}
			}
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

		private async void Helper_BleDeviceServicesLoaded ()
		{
			bool found = false;
			ShowText ("Services ready");
			foreach (BleService bs in Helper.TheDevice.AllServices) {
				if (bs.Service.Id.ToString ().Equals (Helper.SERVICE_ID)) {
					// on a trouvé notre service :)
					foreach (BleCharacteristic bc in bs.Characteristics) {
						if (bc.Characteristic.Id.ToString ().Equals (Helper.CHARACTERISTIC_ID)) {
							// on a notre characteristic :)
							if (Helper.TheCharacteristic == null) {
								found = true;
								Helper.TheCharacteristic = bc;
							}
						}
					}
				}
			}
			if (!found) {
				GoBackChooseDevice ();
				Helper.DoNotificationError ("This is not a BatKeeper device. Please choose the right device.");
			} else {
				ShowText ("This is a good device.");
				//Helper.SettingsSave<int> ("BleCode", 981);
				Helper.BleAuth = (int)(Helper.SettingsRead<int> ("BleCode", 0));
				if (Helper.BleAuth > 0) {
					DoSendCodeAuth ();
				}
			}
		}

		private async void DoSendCodeAuth ()
		{
			/*
			if (Helper.TheCharacteristic == null) {
				foreach (BleService bs in Helper.TheDevice.AllServices) {
					if (bs.Service.Id.ToString ().Equals (Helper.SERVICE_ID)) {
						// on a trouvé notre service :)
						foreach (BleCharacteristic bc in bs.Characteristics) {
							if (bc.Characteristic.Id.ToString ().Equals (Helper.CHARACTERISTIC_ID)) {
							}
						}
					}
				}
			} else {
			*/
			System.Diagnostics.Debug.WriteLine ("Add to event update");
			Helper.TheCharacteristic.Characteristic.ValueUpdated += GotAnswer;
			await Helper.TheCharacteristic.Characteristic.StartUpdatesAsync ();
			//Helper.TheCharacteristic.Characteristic.StartUpdates ();

			if (await Helper.WriteDataToBle (Helper.TheCharacteristic.Characteristic, Helper.BleAuth)) {
				// good auth !!!
				System.Diagnostics.Debug.WriteLine ("Auth sent");
				//Helper.SettingsSave<int> ("BleCode", code);
			} else {
				System.Diagnostics.Debug.WriteLine ("Auth sent error");
			}
			//}
		}

		private async void GotAnswer (object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
		{
			await Helper.TheCharacteristic.Characteristic.StopUpdatesAsync ();
			//Helper.TheCharacteristic.Characteristic.StopUpdates ();
			Helper.TheCharacteristic.Characteristic.ValueUpdated -= GotAnswer;
			byte [] res = new byte [4];
			res = await e.Characteristic.ReadAsync ();
			if (res [0] == 1) {
				// auth is ok
				Helper.SettingsSave<int> ("BleCode", Helper.BleAuth);
				System.Diagnostics.Debug.WriteLine ("Auth ok!");

				Helper.BleDeviceServicesLoaded -= Helper_BleDeviceServicesLoaded;
				Helper.BleDeviceStateChange -= Helper_BleDeviceStateChange;
				Helper.GlobalState = GlobalState.DeviceOk;
				Helper.Navigation.RefreshMenu ();
				Helper.Navigation.NavigateTo (typeof (HomePage));

			}
			if (res [0] == 2) {
				// auth is bad
				System.Diagnostics.Debug.WriteLine ("Auth not ok");
			}
		}


	}
}