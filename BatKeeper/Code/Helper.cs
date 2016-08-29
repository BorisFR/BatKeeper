using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.Toasts;
using System.Linq.Expressions;
using Xamarin.Forms;

namespace BatKeeper
{
	public delegate void Changed (string status);

	public static class Helper
	{
		public static event Changed BleChanged;

		private static void SendChanged (string text)
		{
			if (BleChanged == null) return;
			BleChanged (text);
		}

		internal static RootPage Navigation;
		internal static MenuPage MenuPage;

		// Toast stuff
		// *****************************************************************
		private static IToastNotificator notificator;

		public static void NotificatorInit ()
		{
			if (notificator == null) {
				System.Diagnostics.Debug.WriteLine ("Notificator init");
				notificator = DependencyService.Get<IToastNotificator> ();
			}
		}

		public static void DoNotificationError (string text)
		{
			notificator.Notify (ToastNotificationType.Error, "Error", text, TimeSpan.FromSeconds (5));
		}

		public static void DoNotificationInfo (string text)
		{
			notificator.Notify (ToastNotificationType.Info, "Info", text, TimeSpan.FromSeconds (5));
		}

		// Blutooth Low Energy stuff
		// *****************************************************************
		private static IBluetoothLE ble;
		private static IAdapter adapter;
		private static IDevice device;
		private static IList<IService> services;
		private static ObservableCollection<IDevice> deviceList = new ObservableCollection<IDevice> ();
		private static bool bleOk = false;

		public static bool BleOk ()
		{
			return bleOk;
		}

		private static void BleChangeState ()
		{
			// no bluetooth device!
			if (ble.State == BluetoothState.Unknown) {
				DoNotificationError (Translation.GetString ("ble.Unknown"));
				return;
			}
			if (ble.State == BluetoothState.Unauthorized) {
				DoNotificationError (Translation.GetString ("ble.Unauthorized"));
				return;
			}
			if (ble.State == BluetoothState.Unavailable) {
				DoNotificationError (Translation.GetString ("ble.Unavailable"));
				return;
			}
			if (ble.State == BluetoothState.TurningOff) {
				DoNotificationError (Translation.GetString ("ble.TurningOff"));
				return;
			}
			if (ble.State == BluetoothState.TurningOn) {
				DoNotificationError (Translation.GetString ("ble.TurningOn"));
				return;
			}
			if (ble.State == BluetoothState.Off) {
				DoNotificationError (Translation.GetString ("ble.Off"));
				return;
			}
		}

		public async static void BleInit ()
		{
			System.Diagnostics.Debug.WriteLine ("BLE init");
			if (ble == null) {
				ble = CrossBluetoothLE.Current;
			}
			ble.StateChanged += (s, e) => {
				System.Diagnostics.Debug.WriteLine ($"The bluetooth state changed to {e.NewState}");
				BleChangeState ();
			};
			//BleChangeState ();
			/*
			if (!ble.IsAvailable) {
				DoNotificationError (Translation.GetString ("ble.NotAvailable"));
				return;
			}
			if (!ble.IsOn) {
				DoNotificationError (Translation.GetString ("ble.NotOn"));
				return;
			}
			*/
			bleOk = true;
			if (adapter == null) {
				adapter = CrossBluetoothLE.Current.Adapter;
				adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
				adapter.DeviceConnected += Adapter_DeviceConnected;
				adapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
				adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
				adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
				adapter.DeviceAdvertised += Adapter_DeviceAdvertised;
			}
			SendChanged ("Searching devices");
			adapter.StartScanningForDevicesAsync ();
		}


		private static async void Adapter_DeviceDiscovered (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"Device discovered {e.Device.Id}:  {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
			device = e.Device;
			deviceList.Add (e.Device);
			if (e.Device.Name.Equals ("BatKeeper")) {
				try {
					await adapter.ConnectToDeviceAsync (e.Device);
				} catch (Exception err) {
					System.Diagnostics.Debug.WriteLine ($"Could not connect to {device.Id}:  {device.Name} / {e.Device.State} / {device.NativeDevice} => {err.Message}");
				}
			}
			SendChanged ($"Device found {device.Name}.");
		}

		private static void Adapter_DeviceAdvertised (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"Device DeviceAdvertised {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
		}

		private static async void Adapter_DeviceConnected (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"Device connected {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
			services = await device.GetServicesAsync ();
			foreach (IService s in services) {
				System.Diagnostics.Debug.WriteLine ($"Service {s.Id}: {s.Name} / {s.IsPrimary}");
				var x = await s.GetCharacteristicsAsync ();
				foreach (ICharacteristic c in x) {
					System.Diagnostics.Debug.WriteLine ($"Characteristic {c.Id}: {c.Name} / {c.Properties}");
					if (c.CanUpdate) {
						c.ValueUpdated += (sender2, e2) => {
							System.Diagnostics.Debug.WriteLine ($"Characteristic {c.Name} change: {e2.Characteristic.Value [0]}");
						};
						c.StartUpdates ();
					} else {
						if (c.CanRead) {
							var r = await c.ReadAsync ();
							string st = System.Text.Encoding.UTF8.GetString (r, 0, r.Length);
							System.Diagnostics.Debug.WriteLine ($"Characteristic {c.Name} read: {st}");
							/*
							StringBuilder sb = new StringBuilder ();
							sb.Append ("Characteristic ");
							sb.Append (c.Name);
							sb.Append (":");
							for (int z = 0; z < r.Length; z++) {
								sb.Append (Convert.ToString (r [z]));
							}
							Debug.WriteLine (sb.ToString ());
							*/
						}
					}
				}
			}
		}

		private static void Adapter_DeviceConnectionLost (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"Device connection lost {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");

		}

		private static void Adapter_DeviceDisconnected (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"Device disconnected {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");

		}

		private static void Adapter_ScanTimeoutElapsed (object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ("Device ScanTimeoutElapsed");
			SendChanged ("Searching end.");
		}
	}
}