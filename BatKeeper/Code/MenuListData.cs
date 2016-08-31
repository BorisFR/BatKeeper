using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BatKeeper
{
	public class MenuListData : List<MenuItemCustom>
	{
		public MenuListData ()
		{
			LoadMenu ();

		}

		private void LoadMenu ()
		{
			if (Helper.GlobalState == GlobalState.ChooseDevice || Helper.GlobalState == GlobalState.ConnectToDevice) {
				ImageSource ChooseDeviceSource = ImageSource.FromResource ("BatKeeper.Images.icon_menu.png");
				this.Add (new MenuItemCustom () {
					Title = Translation.GetString ("ChooseDevice"),
					IconSource = ChooseDeviceSource,
					TargetType = typeof (PageChooseDevice)
				});
			}
			if (Helper.GlobalState == GlobalState.ConnectToDevice) {
				ImageSource ConnectToDeviceSource = ImageSource.FromResource ("BatKeeper.Images.icon_menu.png");
				this.Add (new MenuItemCustom () {
					Title = Translation.GetString ("ConnectToDeviceSource"),
					IconSource = ConnectToDeviceSource,
					TargetType = typeof (PageConnectToDevice)
				});
			}
			if (Helper.GlobalState == GlobalState.DeviceOk) {
				ImageSource HomeSource = ImageSource.FromResource ("BatKeeper.Images.icon_menu.png");
				this.Add (new MenuItemCustom () {
					Title = Translation.GetString ("MenuAccueil"),
					IconSource = HomeSource,
					TargetType = typeof (HomePage)
				});
			}
		}
	}
}