using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Data.Sqlite;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using PortableNuGetTest;

namespace iOSNuGetTest
{
	[Register("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		UIWindow window;
		MyViewController viewController;

		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			window = new UIWindow(UIScreen.MainScreen.Bounds);

			viewController = new MyViewController();
			window.RootViewController = viewController;

			window.MakeKeyAndVisible();

			var conn = new SqliteConnection(); // we need a reference in order to stop the linker

			var dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "test.db");
			NuGetTestClass.RunTests(dbPath);

			return true;
		}
	}
}

