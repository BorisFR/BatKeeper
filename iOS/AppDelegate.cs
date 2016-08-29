﻿using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using Plugin.Toasts;
using UIKit;
using Xamarin.Forms;

namespace BatKeeper.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init ();
			DependencyService.Register<ToastNotificatorImplementation> ();
			ToastNotificatorImplementation.Init ();

			Plugin.Iconize.Iconize.With (new Plugin.Iconize.Fonts.FontAwesomeModule ());
			FormsPlugin.Iconize.iOS.IconControls.Init ();
			LoadApplication (new App ());

			return base.FinishedLaunching (app, options);
		}
	}
}
