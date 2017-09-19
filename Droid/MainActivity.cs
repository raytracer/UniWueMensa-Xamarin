using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Nfc;
using Android.Nfc.Tech;


namespace uniwuemensa.Droid
{
	[Activity(Label = "uniwuemensa.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);

			LoadApplication(new App());
		}

		protected override void OnResume()
		{
			base.OnResume();
			var mAdapter = NfcAdapter.GetDefaultAdapter(this);
			var i = new Intent(this, Java.Lang.Class.FromType(typeof(MainActivity)));

			i.AddFlags(ActivityFlags.SingleTop);

			var mPendingIntent = PendingIntent.GetActivity(this, 0, i, 0);
			var detectedTag = new IntentFilter(NfcAdapter.ActionTagDiscovered);
			var filters = new IntentFilter[] { detectedTag };

			var techLists = new string[][] { new string[] { "android.nfc.tech.IsoDep" } };

			mAdapter.EnableForegroundDispatch(this, mPendingIntent, filters, techLists);
		}


		protected override void OnNewIntent(Intent intent)
		{
			base.OnNewIntent(intent);
	

			var tag = (Tag)intent.GetParcelableExtra(NfcAdapter.ExtraTag);

			if (tag != null)
			{
				var desfire = IsoDep.Get(tag);

				desfire.Connect();

				var readCommand = new byte[] { 0x6C, 0x01 };
				var selectCommand = new byte[] { 0x5A, 0x5F, 0x84, 0x15 };

				desfire.Transceive(selectCommand);

				var result = BitConverter.ToInt32(desfire.Transceive(readCommand), 1);

				desfire.Close();
				var alert = new AlertDialog.Builder(this);
				alert.SetTitle("Guthaben");
				alert.SetMessage(String.Format("{0:0.00} €", ((double) result) / 1000.0d));
				alert.SetPositiveButton("Ok", new EventHandler<DialogClickEventArgs>((sender, args) => { }));
				var dialog = alert.Create();
				dialog.Show();
			}
		}
	}
}
