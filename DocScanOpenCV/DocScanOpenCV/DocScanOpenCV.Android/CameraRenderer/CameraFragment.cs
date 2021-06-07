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

namespace CustomRenderer.Droid
{
    public class CameraFragment : Fragment, TextureView.ISurfaceTextureListener
    {
        OpenCvSharp.Android.NativeBinding binding;
        OpenCvSharp.Android.AndroidCapture capture;
        public Android.Widget.ImageView texture;
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
            texture = view.FindViewById<Android.Widget.ImageView>(DocScanOpenCV.Droid.Resource.Id.cameratexture3);
            binding = new OpenCvSharp.Android.NativeBinding(Context, Activity, texture);
            capture = binding.NewCapture(0) as OpenCvSharp.Android.AndroidCapture;
            capture.FrameReady += Capture_FrameReady;
            capture.Start();
        }
        private volatile bool processing = false;
        private Task continousTask;
        private async void Capture_FrameReady(object sender, OpenCvSharp.Native.FrameArgs e)
        {
            return;
            //if (processing) return;
            //processing = true;
            var image = e.Mat;
            binding.ImShow("qwe", image);
            Action<Task> del = (tsk) =>
            {
                image = ImageProcessing.PreviewProcess(image);
                binding.ImShow("qwe", image);
            };
            continousTask ??= Task.Run(() => del);

            continousTask.ContinueWith(del);
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
