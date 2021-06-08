using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using Java.Lang;
using Java.Util.Concurrent;
using Xamarin.Forms.Platform.Android;
using DocScanOpenCV.CameraRenderer;
using Android.Media;
using DocScanOpenCV.Utils;
using OpenCvSharp.Cuda;
using OpenCvSharp;

namespace CustomRenderer.Droid
{
    public class CameraFragment : Fragment, TextureView.ISurfaceTextureListener
    {
        OpenCvSharp.Android.NativeBinding binding;
        OpenCvSharp.Native.Capture capture;
        public Android.Widget.ImageView imageView;
        public AutoFitTextureView textureView1;
        public AutoFitTextureView textureView2;
        public CameraPreview Element { get; set; }

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
            textureView1.SetOpaque(false);
            textureView2.SetOpaque(false);

            imageView = view.FindViewById<Android.Widget.ImageView>(DocScanOpenCV.Droid.Resource.Id.cameratexture3);
            binding = new OpenCvSharp.Android.NativeBinding(Context, Activity, imageView);
            capture = binding.NewCapture(0);
            capture.FrameReady += Capture_FrameReady;
            capture.Start();
        }
        private volatile bool processingFirst = false;
        private volatile bool processingSecond = false;
        private volatile OpenCvSharp.Point[] foundedContours;
        private volatile List<OpenCvSharp.Point[]> allContours;
        public volatile Mat scannedImage;
        private void Capture_FrameReady(object sender, OpenCvSharp.Native.FrameArgs e)
        {
            var image1 = e.Mat.Clone();
            var image2 = e.Mat.Clone();
            if(scannedImage != null && !scannedImage.IsDisposed)
                scannedImage.Dispose();
            scannedImage = e.Mat.Clone();

            if (!processingFirst)
            {
                processingFirst = true;
                Task.Run(() =>
                {
                    try
                    {
                        image1 = ImageProcessing.ProccessToGrayContuour(image1);
                        foundedContours = ImageProcessing.FindContours_BiggestContourInt(image1.Clone());
                        // allContours = ImageProcessing.FindContours_SortedContours(image1.Clone());
                        image1 = image1.CvtColor(ColorConversionCodes.GRAY2RGB);
                        binding.ImShow("processing view", image1, textureView1, binding.locker1);
                    }
                    catch(System.Exception e)
                    {

                    }
                    processingFirst = false;
                });
            }
            if (!processingSecond)
            {
                processingSecond = true;
                Task.Run(() =>
                {
                    //if (allContours != null)
                    //{
                    //    image2 = ImageProcessing.DrawContour(image2, new List<OpenCvSharp.Point[]>(allContours));
                    //}
                    if (foundedContours != null)
                    {
                        image2 = ImageProcessing.DrawContour(image2, foundedContours);
                    }
                    binding.ImShow("normal view", image2, textureView2, binding.locker2);
                    processingSecond = false;
                });
            }
        }
        public override void OnPause()
        {
            capture.Stop();
            base.OnPause();            
        }

        public override async void OnResume()
        {
            base.OnResume();

            capture.Start();
        }

        protected override void Dispose(bool disposing)
        {
            capture.Stop();
            capture.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region TextureView.ISurfaceTextureListener

        async void TextureView.ISurfaceTextureListener.OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {        
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
