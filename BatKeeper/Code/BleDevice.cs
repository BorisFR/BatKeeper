/*
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

	public class BleDevice
	{
		private IList<BleService> allServices = new List<BleService> ();

		public IDevice Device;

		public bool IsEnable {
			get {
				return true;
				if (!(Device.State == DeviceState.Disconnected))
					return false;
				if (Device.Name == null)
					return false;
				if (Device.Name.StartsWith ("BatKeeper", StringComparison.CurrentCulture))
					return true;
				return false;
			}
		}

		public string Name { get { if (Device.Name == null) return string.Empty; return Device.Name; } }

		public DeviceState State { get { return Device.State; } }

		public Guid Id { get { return Device.Id; } }

		public Color StateBackgroundColor {
			get {
				if (IsEnable) {
					if (Device.Id.Equals (Helper.PreferedGuidDevice))
						return Color.FromRgb (100, 255, 100);
					return Color.White;
				}
				return Color.FromRgb (220, 220, 220);
			}
		}

		public IList<BleService> AllServices {
			get { return allServices; }
			set { allServices = value; }
		}
	}

	public class BleCharacteristic
	{
		public ICharacteristic Characteristic;
	}

	public class BleService
	{
		public IService Service;
		private IList<BleCharacteristic> characteristics = new List<BleCharacteristic> ();

		public IList<BleCharacteristic> Characteristics {
			get { return characteristics; }
			set { characteristics = value; }
		}
	}

	public class FakeService : IService
	{
		int number;

		public FakeService (int serviceNumber)
		{
			number = serviceNumber;
		}

		public Guid Id {
			get {
				// 00001530-1212-efde-1523-785feabcd123 DFU ?
				// 6e400001-b5a3-f393-e0a9-e50e24dcca9e UART ?
				switch (number) {
				case 0:
					return Guid.Parse ("0000180a-0000-1000-8000-00805f9b34fb");
				case 1:
					return Guid.Parse ("0000180f-0000-1000-8000-00805f9b34fb");
				case 2:
					return Guid.Parse ("876167c2-1572-44c4-93bc-f2c6ec50324f");
				}
				return Guid.Empty;
			}
		}

		public bool IsPrimary {
			get {
				return true;
			}
		}

		public string Name {
			get {
				switch (number) {
				case 0:
					return "Device Information";
				case 1:
					return "Battery Service";
				case 2:
					return "BatKeeper Infos";
				}
				return String.Empty;
			}
		}

		IDevice IService.Device {
			get {
				throw new NotImplementedException ();
			}
		}

		Guid IService.Id {
			get {
				throw new NotImplementedException ();
			}
		}

		bool IService.IsPrimary {
			get {
				throw new NotImplementedException ();
			}
		}

		string IService.Name {
			get {
				throw new NotImplementedException ();
			}
		}

		public Task<ICharacteristic> GetCharacteristicAsync (Guid id)
		{
			return null;
		}

		public Task<IEnumerable<ICharacteristic>> GetCharacteristicsAsync ()
		{
			return null;
		}

		Task<ICharacteristic> IService.GetCharacteristicAsync (Guid id)
		{
			throw new NotImplementedException ();
		}

		Task<IList<ICharacteristic>> IService.GetCharacteristicsAsync ()
		{
			throw new NotImplementedException ();
		}

	}

	public class FakeDevice : IDevice
	{
		bool isPrefered = false;
		Guid id = Guid.Empty;
		int rand = Helper.Random.Next (1000);

		List<IService> list = new List<IService> ();

		public FakeDevice (bool prefered)
		{
			isPrefered = prefered;
			list.Add (new FakeService (0));
			list.Add (new FakeService (1));
			list.Add (new FakeService (2));
		}

		public IList<AdvertisementRecord> AdvertisementRecords {
			get {
				return null;
			}
		}

		public Guid Id {
			get {
				if (isPrefered)
					return Guid.Parse (Helper.DEVICE_ID);
				if (id == Guid.Empty)
					id = Guid.NewGuid ();
				return id;
			}
		}

		public string Name {
			get {
				return $"BatKeeper Fake {rand}";
			}
		}

		public object NativeDevice {
			get {
				return null;
			}
		}

		public int Rssi {
			get {
				return 0;
			}
		}

		public DeviceState State {
			get {
				return DeviceState.Disconnected;
			}
		}

		public void Dispose ()
		{
			return;
		}

		public Task<IService> GetServiceAsync (Guid id)
		{
			return new Task<IService> (() => { return null; });
		}

		public Task<IList<IService>> GetServicesAsync ()
		{
			return new Task<IList<IService>> (() => { return null; });
		}

		public Task<bool> UpdateRssiAsync ()
		{
			return new Task<bool> (() => { return false; });
		}
	}

}
*/