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

        public volatile byte[] scannedImage;
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

            var width = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width;
            image1 = image1.Resize(new Size(width, width * 4 / 3));

            Task.Run(() => scannedImage = image1.ToBytes());
            

            if (!processingFirst)
            {
                processingFirst = true;
                Task.Run(async () =>
                {
                    var workingImage = image1.Clone();
                    try
                    {
                        //workingImage = ImageProcessing.ProccessToGrayContuour(workingImage);
                        var biggestContour = await ImageProcessing.FindContours_MultiChannel(image1);
                        foundedContours = biggestContour;

                        workingImage = workingImage.DrawContour(biggestContour);
                        binding.ImShow("processing view", workingImage, textureView1, binding.locker1);
                    }
                    catch (System.Exception e)
                    {

                    }
                    workingImage.Release();
                    workingImage.Dispose();
                    processingFirst = false;
                });
            }
            if (!processingSecond)
            {
                processingSecond = true;
                Task.Run(() =>
                {
                    return;
                    try
                    {
                        var workingImage = image1;
                        if (foundedContours != null)
                        {
                            //image2.DrawContour(foundedContours);
                            workingImage = workingImage.DrawContour(foundedContours);
                        }
                        binding.ImShow("normal view", workingImage, textureView2, binding.locker2);
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
