using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Plugin.Toasts;
using System.Linq.Expressions;
using Xamarin.Forms;
using System.Threading.Tasks;
using System.Threading;
using Plugin.BluetoothLE;

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
		public const string DEVICE_ID = "7CA11001-EC1B-49C7-ABE2-671597A51252";
		public const string SERVICE_ID = "876167c2-1572-44c4-93bc-f2c6ec50324f";
		public const string CHARACTERISTIC_ID = "00002601-0000-1000-8000-00805f9b34fb";

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
			try {
				notificator.Notify (ToastNotificationType.Error, "Error", text, TimeSpan.FromSeconds (5));
			} catch (Exception) { }
		}

		public static void DoNotificationInfo (string text)
		{
			try {
				notificator.Notify (ToastNotificationType.Info, "Info", text, TimeSpan.FromSeconds (5));
			} catch (Exception) { }
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
		public static event Trigger BleDeviceServicesLoaded;

		private static IDisposable scanForDevices;
		private static IDisposable connectToDevice;
		private static IDisposable connectionStatusToDevice;
		private static IDisposable scanForServices;
		private static IDisposable scanForCharacteristics;

		//private static IBluetoothLE theBleHardware;
		private static IAdapter theBleAdapter;
		private static bool bleSearchingForDevices = false;
		private static IDevice device;
		private static List<IGattService> services;
		private static IGattService theService;

		public static Dictionary<Guid, IDevice> allDevices = new Dictionary<Guid, IDevice> ();
		public static ObservableCollection<IDevice> allBleDevices = new ObservableCollection<IDevice> ();
		public static IDevice TheDevice;
		private static ConnectionStatus previousDeviceStatus = ConnectionStatus.Disconnected;
		public static Guid PreferedGuidDevice = Guid.Empty;
		private static bool foundOneBatKeeper = false;
		public static IGattCharacteristic TheCharacteristic;
		public static int BleAuthenticateCodeForDevice = 0;


		// BLE device search stuff
		// *****************************************************************


		public static void BleInit ()
		{
			if (theBleAdapter != null) return;
			theBleAdapter = CrossBleAdapter.Current;
			allBleDevices.Clear ();
			allDevices.Clear ();

			theBleAdapter.WhenStatusChanged ().Subscribe (status => {
				System.Diagnostics.Debug.WriteLine ($"* BLE init - WhenStatusChanged: {status}");
				switch (status) {
				case AdapterStatus.PoweredOn:
					foundOneBatKeeper = false;
					StartSearching ();
					break;
				case AdapterStatus.PoweredOff:
					// TODO: prévenir
					if (theBleAdapter.CanControlAdapterState ()) {
						theBleAdapter.SetAdapterState (true);
					} else {
						DoNotificationError (Translation.GetString ("ble.Off"));
					}
					break;
				case AdapterStatus.Resetting:
					// on attend
					DoNotificationError (Translation.GetString ("ble.Resetting"));
					break;
				case AdapterStatus.Unauthorized:
					// TODO: demander l'autorisation
					DoNotificationError (Translation.GetString ("ble.Unauthorized"));
					break;
				case AdapterStatus.Unknown:
					// init en cours, on attend
					DoNotificationError (Translation.GetString ("ble.Unknown"));
					break;
				case AdapterStatus.Unsupported:
					// TODO: prévenir qu'il n'y a pas de bluetooth
					break;
				}
			});

			theBleAdapter.WhenScanningStatusChanged ().Subscribe (scanning => {
				System.Diagnostics.Debug.WriteLine ($"* BLE init - WhenScanningStatusChanged: {scanning}");
				bleSearchingForDevices = scanning;
			});

			/*
			theBleAdapter.WhenDeviceStatusChanged ().Subscribe (device => {
				System.Diagnostics.Debug.WriteLine ($"* BLE init - WhenDeviceStatusChanged: {device}");
			});
			*/
			/*
			if (theBleHardware == null) {
				System.Diagnostics.Debug.WriteLine ("* BLE init");
				theBleHardware = CrossBleAdapter.Current; //  CrossBluetoothLE.Current;
				theBleHardware.StateChanged += (s, e) => {
					System.Diagnostics.Debug.WriteLine ($"* The bluetooth state changed to {e.NewState}");
					BleChangeState ();
					if (e.NewState == BluetoothState.Unavailable) {
						BleSearchForDevicesIsEnd ();
					}
				};
			}*/
			/*
			allBleDevices.Clear ();
			if (theBleAdapter == null) {
				System.Diagnostics.Debug.WriteLine ("* BLE adapter ok");
				theBleAdapter = CrossBluetoothLE.Current.Adapter;
				theBleAdapter.DeviceDiscovered += Adapter_DeviceDiscovered;
				theBleAdapter.DeviceConnected += Adapter_DeviceConnected;
				theBleAdapter.DeviceConnectionLost += Adapter_DeviceConnectionLost;
				theBleAdapter.DeviceDisconnected += Adapter_DeviceDisconnected;
				theBleAdapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
				theBleAdapter.DeviceAdvertised += Adapter_DeviceAdvertised;
			}
			SendChanged ("* Searching for devices...");
			foundOneBatKeeper = false;
			bleSearchingForDevices = true;
			theBleAdapter.StartScanningForDevicesAsync ();
			*/
		}

		private static void StartSearching ()
		{
			scanForDevices = theBleAdapter.Scan ().Subscribe (OnScanResult);
		}

		private static void OnScanResult (IScanResult scanResult)
		{
			if (allDevices.ContainsKey (scanResult.Device.Uuid)) return;
			System.Diagnostics.Debug.WriteLine ($"* OnScanResult: {scanResult.Device.Uuid} - {scanResult.Device.Name}");
			/*
			System.Diagnostics.Debug.WriteLine ($"* OnScanResult {scanResult.Device.Name}: {scanResult.AdvertisementData.LocalName} / {scanResult.AdvertisementData.IsConnectable}");
			if (scanResult.AdvertisementData.ServiceUuids != null)
				foreach (Guid g in scanResult.AdvertisementData.ServiceUuids) {
					System.Diagnostics.Debug.WriteLine ($"**** OnScanResult - {scanResult.Device.Name}: {g}");
				}
			*/
			allBleDevices.Add (scanResult.Device);
			allDevices.Add (scanResult.Device.Uuid, scanResult.Device);
		}

		public static void BleStopSearchingDevicesNow ()
		{
			scanForDevices?.Dispose ();
			//SendChanged ("* Stop the searching for devices...");
		}


		/*
				private static void SendChanged (string text)
				{
					//System.Diagnostics.Debug.WriteLine ($"* << {text} >>");
					if (BleChanged == null) return;
					BleChanged (text);
				}


				private static void BleChangeState ()
				{
					// no bluetooth device!
					if (theBleHardware.State == BluetoothState.Unknown) {
						DoNotificationError (Translation.GetString ("ble.Unknown"));
						return;
					}
					if (theBleHardware.State == BluetoothState.Unauthorized) {
						DoNotificationError (Translation.GetString ("ble.Unauthorized"));
						return;
					}
					if (theBleHardware.State == BluetoothState.Unavailable) {
						DoNotificationError (Translation.GetString ("ble.Unavailable"));
						return;
					}
					if (theBleHardware.State == BluetoothState.TurningOff) {
						DoNotificationError (Translation.GetString ("ble.TurningOff"));
						return;
					}
					if (theBleHardware.State == BluetoothState.TurningOn) {
						DoNotificationError (Translation.GetString ("ble.TurningOn"));
						return;
					}
					if (theBleHardware.State == BluetoothState.Off) {
						DoNotificationError (Translation.GetString ("ble.Off"));
						return;
					}
				}




				private static void Adapter_DeviceDiscovered (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
				{
					System.Diagnostics.Debug.WriteLine ($"* Device discovered {e.Device.Id}:  {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
					device = e.Device;
					if (e.Device.Name != null)
						allBleDevices.Add (new BleDevice () { Device = e.Device });
					if (e.Device.Name != null && e.Device.Name.StartsWith ("BatKeeper", StringComparison.CurrentCulture)) {
						foundOneBatKeeper = true;
						if (e.Device.Id.Equals (Helper.PreferedGuidDevice)) {
							Helper.TheDevice = new BleDevice () { Device = e.Device };
							Helper.BleStopSearchingDevicesNow ();
							Helper.GlobalState = GlobalState.ConnectToDevice;
							Helper.Navigation.RefreshMenu ();
							Helper.Navigation.NavigateTo (typeof (PageConnectToDevice));
						}

						//try {
						//	await adapter.ConnectToDeviceAsync (e.Device);
						//} catch (Exception err) {
						//	System.Diagnostics.Debug.WriteLine ($"Could not connect to {device.Id}:  {device.Name} / {e.Device.State} / {device.NativeDevice} => {err.Message}");
						//}

					}
					if (e.Device.Name != null)
						SendChanged ($"* Device found {device.Name}.");
					else
						SendChanged ("* Device found (null).");
				}

				private static void Adapter_DeviceAdvertised (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
				{
					//System.Diagnostics.Debug.WriteLine ($"Device DeviceAdvertised {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
				}


				private static void Adapter_ScanTimeoutElapsed (object sender, EventArgs e)
				{
					System.Diagnostics.Debug.WriteLine ("* Device ScanTimeoutElapsed");
					BleSearchForDevicesIsEnd ();
				}

				private static void BleSearchForDevicesIsEnd ()
				{
					bleSearchingForDevices = false;
					SendChanged ("* Search for devices is end.");
					if (allBleDevices.Count == 0 || !foundOneBatKeeper) {
						for (int i = 0; i < Helper.Random.Next (7); i++) {
							allBleDevices.Add (new BleDevice () { Device = new FakeDevice (false) });
						}
						allBleDevices.Add (new BleDevice () { Device = new FakeDevice (true) });
						for (int i = 0; i < Helper.Random.Next (7); i++)
							allBleDevices.Add (new BleDevice () { Device = new FakeDevice (false) });
					}
					if (BleSearchEnd != null)
						BleSearchEnd ();
				}
		*/

		// BLE connecting stuff
		// *****************************************************************


		public static void BleConnectToDevice ()
		{
			if (TheDevice.Status == ConnectionStatus.Connected) {
				System.Diagnostics.Debug.WriteLine ($"** ConnectToDevice - {TheDevice.Name} already connected!");
				return;
			}
			System.Diagnostics.Debug.WriteLine ($"** ConnectToDevice - trying to connect to {TheDevice.Name}...");
			previousDeviceStatus = ConnectionStatus.Disconnected;
			connectionStatusToDevice = TheDevice.WhenStatusChanged ().Subscribe (OnDeviceStatusChanged);

			connectToDevice = TheDevice.Connect ().Subscribe (OnDeviceConnect);
		}

		private static void OnDeviceConnect (object o)
		{
			System.Diagnostics.Debug.WriteLine ($"** ConnectToDevice - Connect: {o}");
		}

		private static void DoDisconnectDevice ()
		{
			System.Diagnostics.Debug.WriteLine ("** DoDisconnectDevice");
			TheDevice.CancelConnection ();
			scanForCharacteristics?.Dispose ();
			scanForServices?.Dispose ();
			connectionStatusToDevice?.Dispose ();
			connectToDevice?.Dispose ();
			System.Diagnostics.Debug.WriteLine ("** done");
		}

		private static void OnDeviceStatusChanged (ConnectionStatus status)
		{
			System.Diagnostics.Debug.WriteLine ($"** OnDeviceStatusChanged: {status}");
			if (previousDeviceStatus == ConnectionStatus.Connected && status == ConnectionStatus.Disconnected) {
				DoDisconnectDevice ();
			}
			if (previousDeviceStatus != status) {
				if (BleDeviceStateChange != null)
					BleDeviceStateChange ();
			}
			previousDeviceStatus = status;
		}

		public static void BleDisconnectFromDevice ()
		{
			if (TheDevice.Status == ConnectionStatus.Connected) {
				System.Diagnostics.Debug.WriteLine ($"** BleDisconnectFromDevice - trying to disconnect from {TheDevice.Name}...");
				DoDisconnectDevice ();
				//} else {
				//	System.Diagnostics.Debug.WriteLine ("** BleDisconnectFromDevice - NOT CONNECTED :(");
			}
		}


		public static void BleSearchForServices ()
		{
			System.Diagnostics.Debug.WriteLine ($"*** BleSearchForServices - trying to find {TheDevice.Name}'s services...");
			services = new List<IGattService> ();
			theService = null;
			scanForServices = TheDevice.WhenServiceDiscovered ().Subscribe (OnServiceDiscovered);
		}

		public static void OnServiceDiscovered (IGattService service)
		{
			System.Diagnostics.Debug.WriteLine ($"*** OnServiceDiscovered: {service.Uuid} - {service.Description}");
			services.Add (service);
			if (service.Uuid.ToString ().Equals (SERVICE_ID)) {
				System.Diagnostics.Debug.WriteLine ("*** Service found!");
				theService = service;
				SearchCaracteristics ();
			}
		}

		private static void SearchCaracteristics ()
		{
			scanForServices?.Dispose ();
			System.Diagnostics.Debug.WriteLine ($"**** SearchCaracteristics - trying to find {theService.Uuid}'s caracteristics...");
			TheCharacteristic = null;
			scanForCharacteristics = theService.WhenCharacteristicDiscovered ().Subscribe (OnCharacteristic);
		}

		private static void OnCharacteristic (IGattCharacteristic characteristic)
		{
			if (characteristic == null) {
				System.Diagnostics.Debug.WriteLine ("**** OnCharacteristic - problem");
			}
			if (characteristic.Uuid == null) {
				System.Diagnostics.Debug.WriteLine ($"**** OnCharacteristic - problem: {characteristic}");
			}
			if (characteristic.Description == null) {
				System.Diagnostics.Debug.WriteLine ($"**** OnCharacteristic - problem: {characteristic.Uuid}");
			} else {
				System.Diagnostics.Debug.WriteLine ($"**** OnCharacteristic: {characteristic.Uuid} - {characteristic.Description}");
			}
			if (characteristic.Uuid.ToString ().Equals (CHARACTERISTIC_ID)) {
				// auth stuff
				System.Diagnostics.Debug.WriteLine ("**** Characteristic found!");
				TheCharacteristic = characteristic;
			}
		}

		/*
				private static void Adapter_DeviceConnected (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
				{
					System.Diagnostics.Debug.WriteLine ($"** Device connected {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice} :)");
					if (BleDeviceStateChange != null)
						BleDeviceStateChange ();
				}

				private static void Adapter_DeviceConnectionLost (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
				{
					System.Diagnostics.Debug.WriteLine ($"** Device connection lost {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
					if (BleDeviceStateChange != null)
						BleDeviceStateChange ();
				}

				private static void Adapter_DeviceDisconnected (object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
				{
					System.Diagnostics.Debug.WriteLine ($"** Device disconnected {e.Device.Id}: {e.Device.Name} / {e.Device.State} / {e.Device.NativeDevice}");
					if (BleDeviceStateChange != null)
						BleDeviceStateChange ();
				}


				// BLE service stuff
				// *****************************************************************


				public static async Task BleSearchForServices ()
				{
					System.Diagnostics.Debug.WriteLine ("*** Searching services");
					if (Helper.TheDevice.Device.State != DeviceState.Connected) {
						System.Diagnostics.Debug.WriteLine (@"*** /!\ Not connect!");
						return;
					}
					System.Diagnostics.Debug.WriteLine (@"*** SearchBleServices - async...");
					//services = await device.GetServicesAsync ();
					IService service = await device.GetServiceAsync (Guid.Parse (SERVICE_ID));
					System.Diagnostics.Debug.WriteLine (@"*** SearchBleServices - got services!");
					services = new List<IService> ();
					services.Add (service);
					if (services == null) {
						if (TheDevice.State == DeviceState.Connected)
							theBleAdapter.DisconnectDeviceAsync (TheDevice.Device);
						System.Diagnostics.Debug.WriteLine (@"*** /!\ No service!");
						return;
					}
					System.Diagnostics.Debug.WriteLine ("*** our service is here?");
					foreach (IService s in services) {
						bool ourService = false;
						BleService bs = new BleService ();
						bs.Service = s;
						System.Diagnostics.Debug.WriteLine ($"*** Service {s.Id}: {s.Name} / {s.IsPrimary}");
						if (s.Id.ToString ().Equals (SERVICE_ID)) {
							// we found our Service :)
							ourService = true;
							System.Diagnostics.Debug.WriteLine ("*** Service found!");
						}
						var x = await s.GetCharacteristicsAsync ();
						foreach (ICharacteristic c in x) {
							BleCharacteristic bc = new BleCharacteristic ();
							bc.Characteristic = c;
							bs.Characteristics.Add (bc);
							System.Diagnostics.Debug.WriteLine ($"*** Characteristic {c.Id}: {c.Name} / {c.Properties}");

							//if (c.CanUpdate) {
							//	c.ValueUpdated += (sender2, e2) => {
							//		System.Diagnostics.Debug.WriteLine ($"Characteristic {c.Name} change: {e2.Characteristic.Value [0]}");
							//	};
							//	c.StartUpdates ();
							//} else {
							//	if (c.CanRead) {
							//		var r = await c.ReadAsync ();
							//		string st = System.Text.Encoding.UTF8.GetString (r, 0, r.Length);
							//		System.Diagnostics.Debug.WriteLine ($"Characteristic {c.Name} read: {st}");
							//
							//	}
							//}

							TheDevice.AllServices.Add (bs);
							if (ourService && c.Id.ToString ().Equals (CHARACTERISTIC_ID)) {
								// auth stuff
								System.Diagnostics.Debug.WriteLine ("*** Characteristic found!");
							}
						}
					}
					if (BleDeviceServicesLoaded != null)
						BleDeviceServicesLoaded ();
				}

				public static async Task<bool> WriteDataToBle (ICharacteristic c, Int32 value)
				{
					byte [] data = new byte [4];
					data [3] = (byte)(value >> 24);
					data [2] = (byte)(value >> 16);
					data [1] = (byte)(value >> 8);
					data [0] = (byte)(value);
					System.Diagnostics.Debug.WriteLine ($"*** Writing to Ble: {value}");
					return await c.WriteAsync (data);
				}

			}
			}
			*/

		public static void WriteDataToBle (IGattCharacteristic c, int value)
		{
			byte [] data = new byte [4];
			data [3] = (byte)(value >> 24);
			data [2] = (byte)(value >> 16);
			data [1] = (byte)(value >> 8);
			data [0] = (byte)(value);
			System.Diagnostics.Debug.WriteLine ($"**** Writing to Ble: {value}");
			if (c.CanWriteWithoutResponse ()) {
				System.Diagnostics.Debug.WriteLine ("**** Writing to Ble CanWriteWithoutResponse");
				c.WriteWithoutResponse (data);
			} else {
				if (c.CanWriteWithResponse ()) {
					System.Diagnostics.Debug.WriteLine ("**** Writing to Ble CanWriteWithResponse");
					c.Write (data);
				} else {
					if (c.CanWrite ()) {
						System.Diagnostics.Debug.WriteLine ("**** Writing to Ble CanWrite");
						c.Write (data);
					} else {
						System.Diagnostics.Debug.WriteLine ("**** Writing to Ble ????");
					}
				}
			}
		}

	}
}
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
