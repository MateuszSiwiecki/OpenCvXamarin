using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
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

        public AutoFitTextureView textureView1;
        public AutoFitTextureView textureView2;

        public volatile Mat scannedImage;
        private volatile bool processingFirst = false;
        private volatile bool processingSecond = false;
        public volatile OpenCvSharp.Point[] foundedContours;
        private volatile List<OpenCvSharp.Point[]> allContours;
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

            binding = new OpenCvSharp.Android.NativeBinding(Context, Activity);
            capture = binding.NewCapture(0);
            capture.FrameReady += Capture_FrameReady;
            capture.Start();
        }
        private void Capture_FrameReady(object sender, OpenCvSharp.Native.FrameArgs e)
        {
            var image1 = e.Mat.Clone();
            var image2 = e.Mat.Clone();
            if (scannedImage != null && !scannedImage.IsDisposed)
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
                        var biggestContour = ImageProcessing.FindContours_BiggestContourInt(image1.Clone());
                        foundedContours = biggestContour;

                       // image1 = image1.CvtColor(ColorConversionCodes.GRAY2RGB);
                       // binding.ImShow("processing view", image1, textureView1, binding.locker1);
                    }
                    catch (System.Exception e)
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
                    try
                    {
                        if (foundedContours != null)
                        {
                            //image2.DrawContour(foundedContours);
                            image2 = image2.DrawContour(foundedContours);
                        }
                        binding.ImShow("normal view", image2, textureView2, binding.locker2);
                    }
                    catch (System.Exception e)
                    {

                    }
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
            if (scannedImage != null && !scannedImage.IsDisposed) scannedImage.Dispose();
            textureView1?.Dispose();
            textureView2?.Dispose();
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
