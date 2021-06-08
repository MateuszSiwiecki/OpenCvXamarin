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

            imageView = view.FindViewById<Android.Widget.ImageView>(DocScanOpenCV.Droid.Resource.Id.cameratexture3);
            binding = new OpenCvSharp.Android.NativeBinding(Context, Activity, imageView);
            capture = binding.NewCapture(0);
            capture.FrameReady += Capture_FrameReady;
            capture.Start();
        }
        private volatile bool processing = false;
        private Task continousTask;
        private async void Capture_FrameReady(object sender, OpenCvSharp.Native.FrameArgs e)
        {
            var image = e.Mat;
            binding.ImShow("qwe", image, textureView1);
            binding.ImShow("qwe", image, textureView2);
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
