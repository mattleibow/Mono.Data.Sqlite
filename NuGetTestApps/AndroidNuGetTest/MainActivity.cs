using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using PortableNuGetTest;

namespace AndroidNuGetTest
{
	[Activity(Label = "AndroidNuGetTest", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		int count = 1;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			// Get our button from the layout resource,
			// and attach an event to it
			Button button = FindViewById<Button>(Resource.Id.MyButton);

			button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

			var dbPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "test.db");
			NuGetTestClass.RunTests(dbPath);
		}
	}
}

