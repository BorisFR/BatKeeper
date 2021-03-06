﻿using System;
using Xamarin.Forms;

namespace BatKeeper
{
	public class RootPage : MasterDetailPage
	{
		PageMenu menuPage;

		public RootPage ()
		{
			Helper.Navigation = this;
			menuPage = new PageMenu ();
			menuPage.Menu.ItemSelected += (sender, e) => NavigateTo (e.SelectedItem as MenuItemCustom);
			Master = menuPage;
			Detail = new NavigationPage (new PageChooseDevice ());
			Helper.MenuPage = menuPage;
		}

		public void RefreshMenu ()
		{
			menuPage.RefreshMenu ();
		}

		public void NavigateTo (MenuItemCustom menu)
		{
			if (menu == null)
				return;
			Page displayPage = null;
			if (menu.TargetType == null) return;
			displayPage = (Page)Activator.CreateInstance (menu.TargetType);

			Detail = new NavigationPage (displayPage);
			menuPage.Menu.SelectedItem = null;
			IsPresented = false;
		}


		public void NavigateTo (Type TargetType)
		{
			Page displayPage = null;
			displayPage = (Page)Activator.CreateInstance (TargetType);

			Detail = new NavigationPage (displayPage);
			displayPage = null;
			menuPage.Menu.SelectedItem = null;
			IsPresented = false;
		}

	}
}