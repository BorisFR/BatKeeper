using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BatKeeper
{
	[ContentProperty ("Source")]
	public class ImageResourceExtension : IMarkupExtension
	{

		public string Source { get; set; }

		public object ProvideValue (IServiceProvider serviceProvider)
		{
			if (Source == null)
				return null;
			try {
				var imageSource = ImageSource.FromResource ("BatKeeper.Images." + Source);
				if (imageSource == null)
					System.Diagnostics.Debug.WriteLine ("ImageResourceExtension*** ProvideValue null :" + Source);
				return imageSource;
			} catch (Exception err) {
				System.Diagnostics.Debug.WriteLine ("ImageResourceExtension*** ProvideValue :" + err.Message);
				return null;
			}
		}
	}
}