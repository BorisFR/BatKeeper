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

}