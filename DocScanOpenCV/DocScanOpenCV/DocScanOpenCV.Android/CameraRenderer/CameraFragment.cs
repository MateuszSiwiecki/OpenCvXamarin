using System;
using System.Collections.Generic;
using Dg = System.Diagnostics.Debug;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using DocScanOpenCV.CameraRenderer;
using DocScanOpenCV.Utils;
using OpenCvSharp;

namespace CustomRenderer.Droid
{
    public class CameraFragment : Fragment, TextureView.ISurfaceTextureListener
    {
        OpenCvSharp.Android.NativeBinding binding;
        OpenCvSharp.Native.Capture capture;
        private OpenCvSharp.Android.AndroidCapture ActiveCapture => (OpenCvSharp.Android.AndroidCapture)capture;

        public AutoFitTextureView textureView1;
        public AutoFitTextureView textureView2;
        public ImageView textureView3;

        public volatile byte[] scannedImage;
        private volatile bool processingFirst = false;
        private volatile bool processingSecond = false;
        public volatile OpenCvSharp.Point[] foundedContours;
        public volatile OpenCvSharp.Point[] foundedContoursOryginal;
        private volatile List<OpenCvSharp.Point[]> allContours;
        public SurfaceTexture surface;
        public CameraPreview Element { get; set; }
        public static int height = 0;
        public static int width = 0;

        #region Constructors

        public CameraFragment()
        {
        }

        public CameraFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        #endregion

        #region Overrides

        public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) =>
            inflater.Inflate(DocScanOpenCV.Droid.Resource.Layout.CameraFragment, null);

        public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState)
        {
            textureView1 = view.FindViewById<AutoFitTextureView>(DocScanOpenCV.Droid.Resource.Id.cameratexture1);
            textureView2 = view.FindViewById<AutoFitTextureView>(DocScanOpenCV.Droid.Resource.Id.cameratexture2);
            textureView3 = view.FindViewById<ImageView>(DocScanOpenCV.Droid.Resource.Id.cameratexture3);
            textureView1.SetOpaque(false);
            textureView2.SetOpaque(false);

            textureView1.SurfaceTextureListener = this;

            binding = new OpenCvSharp.Android.NativeBinding(Context, Activity);
            capture = binding.NewCapture(0, 2);
            capture.FrameReady += Capture_FrameReady;
            capture.Start();
        }
        private void Capture_FrameReady(object sender, OpenCvSharp.Native.FrameArgs e)
        {
            //return;
            var image1 = e.Mat;
            var first = false;
            var second = false;

            scannedImage = image1.ToBytes();
            var size = image1.Size();
            image1 = image1.Resize(new Size(width, height));



            if (!processingFirst)
            {
                processingFirst = true;
                Task.Run(async () =>
                {
                    try
                    {
                        var biggestContour = await image1.FindContours_MultiChannel();
                        foundedContours = biggestContour;
                        foundedContoursOryginal = biggestContour
                            .Select(x => new OpenCvSharp.Point(x.X * size.Width / width, x.Y * size.Height / height))
                            .ToArray();
                    }
                    catch (System.Exception e)
                    {

                    }
                    first = true;
                    if (first && second) image1.Dispose();
                    processingFirst = false;
                });
            }
            if (!processingSecond)
            {
                processingSecond = true;
                Task.Run(() =>
                {
                    if (foundedContours == null)
                    {
                        processingSecond = false;
                        return;
                    }
                    var workingImage = image1.Clone();
                    try
                    {
                        workingImage = workingImage.DrawTransparentContour(foundedContours);
                        binding.ImShow("normal view", workingImage, textureView2, binding.locker2);
                    }
                    catch (System.Exception e)
                    {

                    }
                    workingImage.Dispose();
                    second = true;
                    if (first && second) image1.Dispose();
                    processingSecond = false;
                });
            }
        }
        public override void OnPause()
        {
            Dg.WriteLine("OnPause START");
            try
            {
                capture?.Stop();

            }
            catch (Exception e)
            {

            }
            base.OnPause();
            Dg.WriteLine("OnPause END");
        }

        public override async void OnResume()
        {
            Dg.WriteLine("On resume START");
            base.OnResume();

            try
            {
                capture?.Start();
                if (surface != null)
                {

                }
                ActiveCapture?.Camera?.SetPreviewTexture(surface);
                ActiveCapture?.Camera?.SetDisplayOrientation(90);
            }
            catch (Exception e)
            {

            }
            Dg.WriteLine("On resume END");
        }

        protected override void Dispose(bool disposing)
        {
            Dg.WriteLine("Dispose START");

            try
            {
                capture?.Stop();
                capture?.Dispose();
                textureView1?.Dispose();
                textureView2?.Dispose();
            }
            catch (Exception e)
            {

            }
            base.Dispose(disposing);
            Dg.WriteLine("Dispose END");
        }

        #endregion

        #region TextureView.ISurfaceTextureListener

        async void TextureView.ISurfaceTextureListener.OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            this.surface = surface;
            var widthDisp = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width;
            textureView1.LayoutParameters = new Android.Widget.FrameLayout.LayoutParams((int)(widthDisp), (int)(widthDisp * 4 / 3));
            ActiveCapture?.Camera?.SetPreviewTexture(surface);
            ActiveCapture?.Camera?.SetDisplayOrientation(90);
        }

        bool TextureView.ISurfaceTextureListener.OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            capture.Stop();
            return true;
        }

        void TextureView.ISurfaceTextureListener.OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        { }

        void TextureView.ISurfaceTextureListener.OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }

        #endregion
    }

}
