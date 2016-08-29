using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BatKeeper
{
	public class MenuListData : List<MenuItemCustom>
	{
		public MenuListData ()
		{
			ImageSource HomeSource = ImageSource.FromResource ("BatKeeper.Images.icon_menu.png");
			this.Add (new MenuItemCustom () {
				Title = Translation.GetString ("MenuAccueil"),
				IconSource = HomeSource,
				TargetType = typeof (HomePage)
			});

		}
	}
}