using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.Toasts;
using System.Linq.Expressions;
using Xamarin.Forms;
using Plugin.BLE.Abstractions;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.EventArgs;
using System.Threading;

namespace BatKeeper
{
	public delegate void Trigger ();
	public delegate void Changed (string status);

	public enum GlobalState
	{
		ChooseDevice,
		ConnectToDevice,
		DeviceOk
	}

	public static class Helper
	{
		public static GlobalState GlobalState = GlobalState.ChooseDevice;

		internal static RootPage Navigation;
		internal static PageMenu MenuPage;

		public static Random Random = new Random (DateTime.Now.Millisecond);

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

		// Settings stuff
		// *****************************************************************
		public static void SettingsSave<T> (string key, T value)
		{
			Plugin.Settings.CrossSettings.Current.AddOrUpdateValue<T> (key, value);
		}

		public static T SettingsRead<T> (string key, T defaultValue)
		{
			return Plugin.Settings.CrossSettings.Current.GetValueOrDefault<T> (key, defaultValue);
		}

		// Blutooth Low Energy stuff
		// *****************************************************************
		public static event Changed BleChanged;
		public static event Trigger BleSearchEnd;
		public static event Trigger BleDeviceStateChange;

		private static IBluetoothLE ble;
		private static IAdapter adapter;
		private static IDevice device;
		private static IList<IService> services;
		private static bool bleOk = false;

		public static ObservableCollection<BleDevice> DeviceList = new ObservableCollection<BleDevice> ();
		public static BleDevice TheDevice;
		public static Guid PreferedGuidDevice = Guid.Empty;
		private static bool foundOneBatKeeper = false;


		// BLE device search stuff
		// *****************************************************************


		private static void SendChanged (string text)
		{
			if (BleChanged == null) return;
			BleChanged (text);
		}

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
			DeviceList.Clear ();
			if (ble.State == BluetoothState.Unavailable) {
				BleEnd ();
				return;
			}
			ble.StateChanged += (s, e) => {
				System.Diagnostics.Debug.WriteLine ($"The bluetooth state changed to {e.NewState}");
				BleChangeState ();
				if (e.NewState == BluetoothState.Unavailable) {
					BleEnd ();
				}
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
			foundOneBatKeeper = false;
			adapter.StartScanningForDevicesAsync ();
		}

		public static void BleStopSearch ()
		{
			if (ble == null) return;
			if (adapter == null) return;
			adapter.StopScanningForDevicesAsync ();
		}

		private static async void Adapter_DeviceDiscovered (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"Device discovered {e.Device.Id}:  {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
			device = e.Device;
			if (e.Device.Name != null)
				DeviceList.Add (new BleDevice () { Device = e.Device });
			if (e.Device.Name != null && e.Device.Name.StartsWith ("BatKeeper", StringComparison.CurrentCulture)) {
				foundOneBatKeeper = true;
				try {
					//await adapter.ConnectToDeviceAsync (e.Device);
				} catch (Exception err) {
					System.Diagnostics.Debug.WriteLine ($"Could not connect to {device.Id}:  {device.Name} / {e.Device.State} / {device.NativeDevice} => {err.Message}");
				}
			}
			if (e.Device.Name != null)
				SendChanged ($"Device found {device.Name}.");
			else
				SendChanged ("Device found (null).");
		}

		private static void Adapter_DeviceAdvertised (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"Device DeviceAdvertised {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
		}


		private static void Adapter_ScanTimeoutElapsed (object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ("Device ScanTimeoutElapsed");
			BleEnd ();
		}

		private static void BleEnd ()
		{
			SendChanged ("Searching end.");
			if (DeviceList.Count == 0 || !foundOneBatKeeper) {
				for (int i = 0; i < Helper.Random.Next (7); i++) {
					DeviceList.Add (new BleDevice () { Device = new FakeDevice (false) });
				}
				DeviceList.Add (new BleDevice () { Device = new FakeDevice (true) });
				for (int i = 0; i < Helper.Random.Next (7); i++)
					DeviceList.Add (new BleDevice () { Device = new FakeDevice (false) });
			}
			if (BleSearchEnd != null)
				BleSearchEnd ();
		}


		// BLE connecting stuff
		// *****************************************************************


		public async static void ConnectToDevice ()
		{
			if (Helper.TheDevice.Device.State == DeviceState.Connected)
				return;
			adapter.ConnectToDeviceAsync (Helper.TheDevice.Device);
		}

		private static async void Adapter_DeviceConnected (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"Device connected {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
			if (BleDeviceStateChange != null)
				BleDeviceStateChange ();
		}

		private static void Adapter_DeviceConnectionLost (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"Device connection lost {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
			if (BleDeviceStateChange != null)
				BleDeviceStateChange ();
		}

		private static void Adapter_DeviceDisconnected (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine ($"Device disconnected {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
			if (BleDeviceStateChange != null)
				BleDeviceStateChange ();
		}


		// BLE service stuff
		// *****************************************************************


		public static async void SearchBleServices ()
		{
			if (Helper.TheDevice.Device.State != DeviceState.Connected)
				return;
			services = await device.GetServicesAsync ();
			if (services == null) {
				if (TheDevice.State == DeviceState.Connected)
					adapter.DisconnectDeviceAsync (TheDevice.Device);
				return;
			}
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

	}
}