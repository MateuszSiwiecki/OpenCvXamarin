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
using Xamarin.Forms.Platform.Android;
using Android.Views;
using System.ComponentModel;

[assembly: Xamarin.Forms.ExportRenderer(typeof(DocScanOpenCV.CameraRenderer.CameraXPreview), typeof(CameraXPreviewRenderer))]
namespace DocScanOpenCV.Droid
{
    class CameraXPreviewRenderer : FrameLayout, IVisualElementRenderer, IViewRenderer
    {
        ImageCapture imageCapture;
        File outputDirectory;
        IExecutorService cameraExecutor;
        VisualElementTracker visualElementTracker;
        Xamarin.Forms.Platform.Android.FastRenderers.VisualElementRenderer visualElementRenderer;

        PreviewView viewFinder;

        public CameraXPreviewRenderer(Context context) : base(context)
        {
            visualElementRenderer = new Xamarin.Forms.Platform.Android.FastRenderers.VisualElementRenderer(this);

            //this.viewFinder = this.FindViewById<PreviewView>(Resource.Id.viewFinder);
             //var camera_capture_button = this.FindViewById<Button>(Resource.Id.camera_capture_button);

            //StartCamera(context);
        }


        public void StartCamera(Context context)
        {
            var cameraProviderFuture = ProcessCameraProvider.GetInstance(context);

            cameraProviderFuture.AddListener(new Runnable(() =>
            {
                // Used to bind the lifecycle of cameras to the lifecycle owner
                var cameraProvider = (ProcessCameraProvider)cameraProviderFuture.Get();

                // Preview
                var preview = new Preview.Builder().Build();
                preview.SetSurfaceProvider(viewFinder.SurfaceProvider);
                
                // Take Photo
                this.imageCapture = new ImageCapture.Builder().Build();

                // Frame by frame analyze
                //var imageAnalyzer = new ImageAnalysis.Builder().Build();
                //imageAnalyzer.SetAnalyzer(cameraExecutor, new LuminosityAnalyzer(luma =>
                //    Log.Debug(TAG, $"Average luminosity: {luma}")
                //    ));

                // Select back camera as a default, or front camera otherwise
                CameraSelector cameraSelector = null;
                if (cameraProvider.HasCamera(CameraSelector.DefaultBackCamera) == true)
                    cameraSelector = CameraSelector.DefaultBackCamera;
                else if (cameraProvider.HasCamera(CameraSelector.DefaultFrontCamera) == true)
                    cameraSelector = CameraSelector.DefaultFrontCamera;
                else
                    throw new System.Exception("Camera not found");

                try
                {
                    // Unbind use cases before rebinding
                    cameraProvider.UnbindAll();

                    // Bind use cases to camera
                    cameraProvider.BindToLifecycle((AndroidX.Lifecycle.ILifecycleOwner)context, cameraSelector, preview, imageCapture);
                }
                catch (Exception exc)
                {
                }

            }), ContextCompat.GetMainExecutor(context)); //GetMainExecutor: returns an Executor that runs on the main thread.
        }
        Xamarin.Forms.VisualElement IVisualElementRenderer.Element => throw new System.NotImplementedException();

        VisualElementTracker IVisualElementRenderer.Tracker => throw new System.NotImplementedException();

        ViewGroup IVisualElementRenderer.ViewGroup => throw new System.NotImplementedException();

        View IVisualElementRenderer.View => throw new System.NotImplementedException();

        event System.EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
        {
            add
            {
                throw new System.NotImplementedException();
            }

            remove
            {
                throw new System.NotImplementedException();
            }
        }

        event System.EventHandler<PropertyChangedEventArgs> IVisualElementRenderer.ElementPropertyChanged
        {
            add
            {
                throw new System.NotImplementedException();
            }

            remove
            {
                throw new System.NotImplementedException();
            }
        }

        Xamarin.Forms.SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            Measure(widthConstraint, heightConstraint);
            Xamarin.Forms.SizeRequest result 
                = new Xamarin.Forms.SizeRequest(new Xamarin.Forms.Size(MeasuredWidth, MeasuredHeight), new Xamarin.Forms.Size(Context.ToPixels(20), Context.ToPixels(20)));
            return result;
        }

        void IViewRenderer.MeasureExactly()
        {
            throw new System.NotImplementedException();
        }

        void IVisualElementRenderer.SetElement(Xamarin.Forms.VisualElement element)
        {
            throw new System.NotImplementedException();
        }

        void IVisualElementRenderer.SetLabelFor(int? id)
        {
            throw new System.NotImplementedException();
        }

        void IVisualElementRenderer.UpdateLayout()
        {
            throw new System.NotImplementedException();
        }
    }
}