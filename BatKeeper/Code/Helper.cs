using System;
using Plugin.Toasts;

namespace BatKeeper
{
	public class Helper
	{

		internal static RootPage Navigation;
		internal static MenuPage MenuPage;
		internal static IToastNotificator Notificator;

		public static void DoNotificationInfo (string text)
		{
			Helper.Notificator.Notify (ToastNotificationType.Info, "Info", text, TimeSpan.FromSeconds (5));
		}

	}
}