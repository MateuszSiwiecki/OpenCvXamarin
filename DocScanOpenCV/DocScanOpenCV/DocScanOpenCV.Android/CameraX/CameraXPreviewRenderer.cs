using Android;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Camera.Core;
using AndroidX.Camera.Lifecycle;
using AndroidX.Camera.View;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Java.IO;
using Java.Lang;
using Java.Util;
using Java.Util.Concurrent;
using System.Linq;
using DocScanOpenCV.Droid;
using Android.Content;

[assembly: Xamarin.Forms.ExportRenderer(typeof(DocScanOpenCV.CameraRenderer.CameraXPreview), typeof(CameraXPreviewRenderer))]
namespace DocScanOpenCV.Droid
{
    class CameraXPreviewRenderer : FrameLayout
    {
        ImageCapture imageCapture;
        File outputDirectory;
        IExecutorService cameraExecutor;

        PreviewView viewFinder;

        public CameraXPreviewRenderer(Context context) : base(context)
        {
             this.viewFinder = this.FindViewById<PreviewView>(Resource.Id.viewFinder);
             var camera_capture_button = this.FindViewById<Button>(Resource.Id.camera_capture_button);
        }

    }
}