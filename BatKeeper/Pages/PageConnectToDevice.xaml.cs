using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.BluetoothLE;
using Xamarin.Forms;

namespace BatKeeper
{
	public partial class PageConnectToDevice : ContentPage
	{
		IDisposable onNotif;
		IDisposable onRead;
		IDisposable onSub;

		public PageConnectToDevice ()
		{
			InitializeComponent ();
			NavigationPage.SetHasNavigationBar (this, false);

			try {
				lDevice.Text = $"Device {Helper.TheDevice.Name}";
				ShowText ("Connecting...");
				btCancel.Clicked += BtCancel_Clicked;
				Helper.BleDeviceStateChange += Helper_BleDeviceStateChange;
				//Helper.BleDeviceServicesLoaded += Helper_BleDeviceServicesLoaded;
				btOk.Clicked += BtOk_Clicked;
				btOk.IsEnabled = false;
				eCode.TextChanged += ECode_TextChanged;
				Helper.BleConnectToDevice ();
			} catch (Exception) {

			}

		}

		protected override void OnDisappearing ()
		{
			//if (Helper.TheCharacteristic == null)
			//	Disconnect ();
			//if (onNotif != null)
			onSub?.Dispose ();
			onRead?.Dispose ();
			onNotif?.Dispose ();
			Helper.BleDeviceStateChange -= Helper_BleDeviceStateChange;
			base.OnDisappearing ();
		}

		void ShowText (string status)
		{
			//System.Diagnostics.Debug.WriteLine ($"ShowText: {status}");
			Device.BeginInvokeOnMainThread (() => {
				lState.Text = status;
			});
		}

		private async Task Disconnect ()
		{
			/*
			if (Helper.TheCharacteristic != null) {
				await Helper.TheCharacteristic.Characteristic.StopUpdatesAsync ();
				//Helper.TheCharacteristic.Characteristic.StopUpdates ();
				Helper.TheCharacteristic.Characteristic.ValueUpdated -= GotAnswer;
				Helper.TheCharacteristic = null;
			}
			Helper.BleDeviceServicesLoaded -= Helper_BleDeviceServicesLoaded;
			Helper.BleDeviceStateChange -= Helper_BleDeviceStateChange;
			*/
			Helper.BleDisconnectFromDevice ();
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
				Helper.BleAuthenticateCodeForDevice = Convert.ToInt32 (eCode.Text.Trim ());
				DoSendCodeAuth ();
			} catch (Exception) {
				btOk.IsEnabled = true;
			}
		}

		void ECode_TextChanged (object sender, TextChangedEventArgs e)
		{
			try {
				if (eCode.Text.Trim ().Length == 3) {
					try {
						int code = Convert.ToInt32 (eCode.Text.Trim ());
						btOk.IsEnabled = true;
					} catch (Exception) {
						btOk.IsEnabled = false;
					}
				}
			} catch (Exception) { }
		}

		void Helper_BleDeviceStateChange ()
		{
			ShowText ($"State: {Helper.TheDevice.Status}");
			if (Helper.TheDevice.Status == ConnectionStatus.Disconnected) {
				System.Diagnostics.Debug.WriteLine ("> Disconnected, going back to devices choice");
				Helper.DoNotificationInfo ("Device disconnect.");
				GoBackChooseDevice ();
				return;
			}
			if (Helper.TheDevice.Status == ConnectionStatus.Connected) {
				System.Diagnostics.Debug.WriteLine ("> Connected, searching services");
				Helper.BleSearchForServices ();
				return;
			}
			System.Diagnostics.Debug.WriteLine ($"> Don't know what to do with this state: {Helper.TheDevice.Status}");
		}


		private void DoSendCodeAuth ()
		{
			if (Helper.TheCharacteristic == null) {
				System.Diagnostics.Debug.WriteLine (@"> /!\ DoSendCodeAuth TheCharacteristic is null :(");
				return;
			}
			System.Diagnostics.Debug.WriteLine ("> Add to event update");
			onSub = Helper.TheCharacteristic.SubscribeToNotifications ().Subscribe (OnNotification);
			//onNotif = Helper.TheCharacteristic.WhenNotificationReceived ().Subscribe (OnNotification);

			onRead = Helper.TheCharacteristic.WhenReadOrNotify (TimeSpan.FromMilliseconds (500)).Subscribe (OnNotification);
			onNotif = Helper.TheCharacteristic.WhenNotificationReceived ().Subscribe (OnNotification);

			Helper.WriteDataToBle (Helper.TheCharacteristic, Helper.BleAuthenticateCodeForDevice);
			//await Helper.TheCharacteristic.Characteristic.StartUpdatesAsync ();

			//if (await Helper.WriteDataToBle (Helper.TheCharacteristic.Characteristic, Helper.BleAuthenticateCodeForDevice)) {
			// good auth !!!
			//	System.Diagnostics.Debug.WriteLine ("> Auth sent ok");
			//Helper.SettingsSave<int> ("BleCode", code);
			//} else {
			//	System.Diagnostics.Debug.WriteLine ("> Auth sent error");
			//}
		}

		private void OnNotification (CharacteristicResult result)
		{
			if (result.Characteristic == null) {
				System.Diagnostics.Debug.WriteLine ($">OnNotification - problem: {result}");
			}
			if (result.Characteristic.Uuid == null) {
				System.Diagnostics.Debug.WriteLine ($">OnNotification - problem: {result.Characteristic}");
			}
			System.Diagnostics.Debug.WriteLine ($">OnNotification: {result.Characteristic.Uuid}");
			if (result.Data != null) {
				foreach (int i in result.Data) {
					System.Diagnostics.Debug.WriteLine ($">Data: {i}");
				}
			}
		}

		/*
				private void Helper_BleDeviceServicesLoaded ()
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
										System.Diagnostics.Debug.WriteLine ("> Characteristic found");
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
						Helper.BleAuthenticateCodeForDevice = (int)(Helper.SettingsRead<int> ("BleCode", 0));
						if (Helper.BleAuthenticateCodeForDevice > 0) {
							DoSendCodeAuth ();
						}
					}
				}

				private async void DoSendCodeAuth ()
				{

					//if (Helper.TheCharacteristic == null) {
					//	foreach (BleService bs in Helper.TheDevice.AllServices) {
					//		if (bs.Service.Id.ToString ().Equals (Helper.SERVICE_ID)) {
					//			// on a trouvé notre service :)
					//			foreach (BleCharacteristic bc in bs.Characteristics) {
					//				if (bc.Characteristic.Id.ToString ().Equals (Helper.CHARACTERISTIC_ID)) {
					//				}
					//			}
					//		}
					//	}
					//} else {

					System.Diagnostics.Debug.WriteLine ("> Add to event update");
					if (Helper.TheCharacteristic == null) {
						System.Diagnostics.Debug.WriteLine (@"> /!\ DoSendCodeAuth TheCharacteristic is null :(");
						return;
					}
					if (Helper.TheCharacteristic.Characteristic == null) {
						System.Diagnostics.Debug.WriteLine (@"> /!\ DoSendCodeAuth TheCharacteristic.Characteristic is null :(");
						return;
					}
					Helper.TheCharacteristic.Characteristic.ValueUpdated += GotAnswer;
					await Helper.TheCharacteristic.Characteristic.StartUpdatesAsync ();
					//Helper.TheCharacteristic.Characteristic.StartUpdates ();

					if (await Helper.WriteDataToBle (Helper.TheCharacteristic.Characteristic, Helper.BleAuthenticateCodeForDevice)) {
						// good auth !!!
						System.Diagnostics.Debug.WriteLine ("> Auth sent ok");
						//Helper.SettingsSave<int> ("BleCode", code);
					} else {
						System.Diagnostics.Debug.WriteLine ("> Auth sent error");
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
						Helper.SettingsSave<int> ("BleCode", Helper.BleAuthenticateCodeForDevice);
						System.Diagnostics.Debug.WriteLine ("> Auth ok!");

						Helper.BleDeviceServicesLoaded -= Helper_BleDeviceServicesLoaded;
						Helper.BleDeviceStateChange -= Helper_BleDeviceStateChange;
						Helper.GlobalState = GlobalState.DeviceOk;
						Helper.Navigation.RefreshMenu ();
						Helper.Navigation.NavigateTo (typeof (HomePage));

					}
					if (res [0] == 2) {
						// auth is bad
						System.Diagnostics.Debug.WriteLine ("> Auth not ok");
					}
				}
				*/

	}
}