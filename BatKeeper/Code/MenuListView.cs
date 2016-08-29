using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace BatKeeper
{
	public class MenuListView : ListView
	{
		List<MenuItemCustom> data = new MenuListData ();

		public MenuListView ()
		{
			ItemsSource = data;
			VerticalOptions = LayoutOptions.FillAndExpand;
			BackgroundColor = Color.Transparent;
			SeparatorVisibility = SeparatorVisibility.None;

			var cell = new DataTemplate (typeof (MenuCell));
			cell.SetBinding (MenuCell.TextProperty, "Title");
			cell.SetBinding (MenuCell.ImageSourceProperty, "IconSource");

			ItemTemplate = cell;

		}

		public void RefreshData ()
		{
			ItemsSource = null;
			data = new MenuListData ();
			ItemsSource = data;
		}

		public void AddMenu (MenuItemCustom item)
		{
			data.Add (item);
			ItemsSource = null;
			ItemsSource = data;
		}

	}
}