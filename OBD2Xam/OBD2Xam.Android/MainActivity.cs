using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;
using Android.Support.V4.App;
using Android.Support.V4.Content;

namespace OBD2Xam.Droid
{
    [Activity(Label = "OBD2Xam", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            checkPermissions();

            LoadApplication(new App());
        }

        private void checkPermissions()
        {
            try
            {
                // check / prompt for permissions
                // https://jeremylindsayni.wordpress.com/2018/12/16/how-to-detect-nearby-bluetooth-devices-with-net-and-xamarin-android/

                const int permissionsRequestCode = 1000;

                var permissions = new[]
                {
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.Bluetooth,
                    Manifest.Permission.Internet
                };

                bool promptUser = false;
                foreach (var permission in permissions)
                {
                    var permissionGranted = ContextCompat.CheckSelfPermission(this, permission);
                    if (permissionGranted == Permission.Denied)
                    {
                        promptUser = true;
                        break;
                    }
                }
                if (promptUser)
                {
                    ActivityCompat.RequestPermissions(this, permissions, permissionsRequestCode);
                }
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                System.Diagnostics.Debugger.Break();
            }
        }
    }
}