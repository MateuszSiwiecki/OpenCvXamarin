using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocScanOpenCV.Droid.MicrosoftLens
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter( new[] { Android.Content.Intent.ActionCameraButton})]
    public class PdfReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            // Do stuff here.
            Toast.MakeText(context, "123", ToastLength.Short);
        }
    }
}