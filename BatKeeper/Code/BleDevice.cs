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

		public IDevice Device;

		public bool IsEnable {
			get {
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
	}

	public class FakeService : IService
	{
		int number;

		public FakeService (int serviceNumber)
		{
			number = serviceNumber;
			switch (serviceNumber) {
			case 0:
				break;
			}
		}

		public Guid Id {
			get {
				switch (number) {
				case 0:
					return Guid.Parse ("");
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
					return "Service name";
				}
				return String.Empty;
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
					return Guid.Parse ("7CA11001-EC1B-49C7-ABE2-671597A51252");
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