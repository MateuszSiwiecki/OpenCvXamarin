using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DocScanOpenCV.Droid.MicrosoftLens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: Xamarin.Forms.Dependency(typeof(DocScanOpenCV.Droid.MicrosoftOfficeCaller))]
namespace DocScanOpenCV.Droid
{
    public class MicrosoftOfficeCaller : IMicrosoftLens
    {
        Context content => Android.App.Application.Context;
        public static PdfReceiver receiver = new PdfReceiver();
        public void Open()
        {
            content.RegisterReceiver(receiver, new IntentFilter());
            //try
            //{
            //    var intent = content.PackageManager.GetLaunchIntentForPackage("com.microsoft.office.officelens");
            //    content.StartActivity(intent);
            //}
            //catch (Exception e)
            //{
            //    var geoUri = Android.Net.Uri.Parse("https://play.google.com/store/apps/details?id=com.microsoft.office.officelens");
            //    var mapIntent = new Intent(Intent.ActionView, geoUri);
            //    content.StartActivity(mapIntent);
            //}
        }
    }
}