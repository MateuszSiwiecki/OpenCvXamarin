
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using DocScanOpenCV.Droid;
using System;
using Xamarin.Essentials;

[assembly: Xamarin.Forms.Dependency(typeof(MicrosoftLens))]
namespace DocScanOpenCV.Droid
{
    public class MicrosoftLens : Activity, IMicrosoftLens
    {
        public static readonly int PickImageId = 1000;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
        }
        public async void Open()
        {
            try
            {


                var intent = Android.App.Application.Context.PackageManager.GetLaunchIntentForPackage("com.microsoft.office.officelens");
                intent.SetAction(Intent.ActionMain);
                intent.SetFlags(ActivityFlags.NewTask);
                MainActivity.Instance.StartActivityForResult(intent, PickImageId);
                
            }
            catch (Exception e)
            {
                Toast.MakeText(Android.App.Application.Context, "It seems, you have not downloaded Msofficelens android app", ToastLength.Long).Show();
                var geoUri = Android.Net.Uri.Parse("https://play.google.com/store/apps/details?id=com.microsoft.office.officelens");
                var mapIntent = new Intent(Intent.ActionView, geoUri);
                mapIntent.SetFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(mapIntent);
            }
        }
    }
}