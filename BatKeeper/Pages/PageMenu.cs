using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace BatKeeper
{
	public partial class PageMenu : ContentPage
	{
		public ListView Menu { get; set; }

		public PageMenu ()
		{
			Title = "Back";
			//BackgroundColor = GlobalDesign.Menu_BackgroundColor;
			Menu = new MenuListView ();
			var menuLabel = new ContentView {
				Padding = new Thickness (10, 0, 0, 5),
				Content = new Label {
					//TextColor = GlobalDesign.Menu_TextColor,
					Text = ""
				}
			};

			var layout = new StackLayout {
				Spacing = 0,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			layout.Children.Add (menuLabel);
			layout.Children.Add (Menu);
			Content = layout;
			//InitializeComponent ();
		}

		public void RefreshMenu ()
		{
			Device.BeginInvokeOnMainThread (() => {
				((MenuListView)Menu).RefreshData ();
			});
		}

	}
}