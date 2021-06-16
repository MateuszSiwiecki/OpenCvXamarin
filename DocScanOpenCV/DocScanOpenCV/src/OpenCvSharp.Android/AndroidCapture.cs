using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Debug = System.Diagnostics.Debug;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Hardware = Android.Hardware;
using Graphics = Android.Graphics;
using OpenCvSharp;
using OpenCvSharp.Native;
using Android.Graphics;
using static MoreLinq.Extensions.MinByExtension;

#pragma warning disable CS0618

namespace OpenCvSharp.Android
{
    public class AndroidCapture : Capture
    {
        private double fps;
        public override double FPS => fps;
        public bool MultiThread { get; set; } = true;

        public override bool IsOpened => Camera != null;

        public Hardware.Camera Camera;
        bool cameraOn = false;
        int width;
        int height;
        int cameraIndex;
        int documentSize;
        Graphics.ImageFormatType cameraType;
        Graphics.SurfaceTexture Texture;

        object capturedBufferLocker = new object();

        long frameCount = 0;
        long lastFrame = -1;

        public override event EventHandler<FrameArgs> FrameReady;

        public AndroidCapture(int index, int documentSize)
        {
            cameraIndex = index;
            this.documentSize = documentSize;
        }
        public AndroidCapture(int index) : this(index, 2)
        {
        }

        public AndroidCapture(string filepath)
        {
            throw new NotImplementedException();
        }

        #region CaptureProc

        private int initializeAttempt;

        protected override void OnStart()
        {
            try
            {
                if (Camera == null)
                {
                    Camera = Hardware.Camera.Open(cameraIndex);
                }
                else
                {
                    try
                    {
                        Camera.StartPreview();
                    }
                    catch (Exception e)
                    {
                        Camera = Hardware.Camera.Open(cameraIndex);
                    }
                }

                if (Texture == null)
                    Texture = new Graphics.SurfaceTexture(0);

                CameraPreviewCallback callback = new CameraPreviewCallback();
                callback.PreviewUpdated += Callback_PreviewUpdated;

                Hardware.Camera.Size foundedSize = null;
                Hardware.Camera.Parameters parameter = Camera.GetParameters();
                List<Hardware.Camera.Size> supportSize = parameter.SupportedPreviewSizes.OrderByDescending(x => x.Width).ToList();

                var width = Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width;

                if (documentSize == 2) foundedSize = supportSize.MinBy(x => Math.Abs(700000 - (x.Width * x.Height))).First();
                else if (documentSize == 1) foundedSize = supportSize.MinBy(x => Math.Abs(300000 - (x.Width * x.Height))).First();
                else if (documentSize == 0) foundedSize = supportSize.MinBy(x => Math.Abs(150000 - (x.Width * x.Height))).First();

                parameter.SetPreviewSize(foundedSize.Width, foundedSize.Height);
                CvLogger.Log(this, $"SET Camera Size: W{foundedSize.Height},H{foundedSize.Width}");

                string[] supportedFocusMode = parameter.SupportedFocusModes.ToArray();
                if (supportedFocusMode.Contains(Hardware.Camera.Parameters.FocusModeContinuousVideo))
                {
                    parameter.FocusMode = Hardware.Camera.Parameters.FocusModeContinuousVideo;
                }
                else if (supportedFocusMode.Contains(Hardware.Camera.Parameters.FocusModeContinuousPicture))
                {
                    parameter.FocusMode = Hardware.Camera.Parameters.FocusModeContinuousPicture;
                }
                parameter.ColorEffect = Hardware.Camera.Parameters.EffectNone;

                this.width = parameter.PreviewSize.Width;
                height = parameter.PreviewSize.Height;
                fps = parameter.PreviewFrameRate;
                cameraType = parameter.PreviewFormat;

                CvLogger.Log(this, string.Format("Camera is creating W{0} H{1} FPS{2}", width, height, fps));
                Camera.SetParameters(parameter);

                Camera.SetPreviewCallback(callback);
                Camera.SetPreviewTexture(Texture);
                Camera.StartPreview();

                cameraOn = true;
            }
            catch (Exception ex)
            {
                CvLogger.Log(this, "Camera Init Failed.\n" + ex.ToString());

                Dispose();

                throw new ArgumentException("Camera Exception", ex);
            }
        }

        protected override void OnStop()
        {
            if (Camera != null)
            {
                Camera.StopPreview();
                Camera.SetPreviewCallback(null);
                Camera.SetPreviewTexture(null);
            }
            cameraOn = false;
        }

        private void Callback_PreviewUpdated(object sender, PreviewUpdatedEventArgs e)
        {
            CvProfiler.End("Captured");
            CvProfiler.Start("Captured");
            CvProfiler.Count("CapturedFPS");

            if (FrameReady == null)
                return;

            frameCount++;
            if (MultiThread)
            {
                if (e.Buffer != null && LimitedTaskScheduler.QueuedTaskCount < LimitedTaskScheduler.MaxTaskCount)
                    LimitedTaskScheduler.Factory.StartNew(() => CaptureCvtProc(e.Buffer, frameCount, LimitedTaskScheduler.QueuedTaskCount));
            }
            else CaptureCvtProc(e.Buffer, 0, 0);

            CvProfiler.Capture("TaskCount", LimitedTaskScheduler.QueuedTaskCount);
        }

        private void CaptureCvtProc(byte[] Buffer, long frameIndex, int threadindex)
        { 
            CvProfiler.Start("CaptureCvt" + threadindex);
            Mat mat = null;

            CvProfiler.Start("CaptureCvt.CvtColor" + threadindex);
            switch (cameraType)
            {
                case Graphics.ImageFormatType.Nv16:
                    mat = new Mat((int)Math.Round(height * 1.5), width, MatType.CV_8UC1, Buffer);
                    Cv2.CvtColor(mat, mat, ColorConversionCodes.YUV2BGR_NV12);
                    break;
                case Graphics.ImageFormatType.Nv21:
                    mat = new Mat((int)Math.Round(height * 1.5), width, MatType.CV_8UC1, Buffer);
                    Cv2.CvtColor(mat, mat, ColorConversionCodes.YUV2BGR_NV21);
                    break;
                case Graphics.ImageFormatType.Rgb565:
                    mat = new Mat(width, height, MatType.CV_16UC1, Buffer);
                    Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR5652BGR);
                    break;
                case Graphics.ImageFormatType.Yuv420888:
                default:
                    throw new NotImplementedException("Unknown Camera Format");
            }
            CvProfiler.End("CaptureCvt.CvtColor" + threadindex);

            CvProfiler.Start("CaptureCvt.Tp" + threadindex);
            Cv2.Transpose(mat, mat);
            CvProfiler.End("CaptureCvt.Tp" + threadindex);

            CvProfiler.Start("CaptureCvt.Flip" + threadindex);
            if (cameraIndex == 1)
                Cv2.Flip(mat, mat, FlipMode.XY);
            else
                Cv2.Flip(mat, mat, FlipMode.Y);
            CvProfiler.End("CaptureCvt.Flip" + threadindex);

            CvProfiler.End("CaptureCvt" + threadindex);
             
            var args = new FrameArgs(mat, 'y');
            args.MatDispose = false;

            if (MultiThread)
            {
                lock (capturedBufferLocker)
                {
                    if (lastFrame > frameIndex)
                    {
                        if (mat != null)
                            mat.Dispose();
                        mat = null;
                        CvProfiler.Count("CaptureSkipped");
                        return;
                    }

                    lastFrame = frameIndex;
                    FrameReady?.Invoke(this, args);
                }
            }
            else FrameReady?.Invoke(this, args); 

            if (args.MatDispose)
            {
                mat.Release();
                mat.Dispose();
                mat = null;
            }

            if (args.Break)
            {
                Dispose();
                Stop();
                return;
            }
        }

        #endregion CaptureProc

        public override void Dispose()
        {
            if (Camera != null)
            {
                Stop();
                Camera.Release();
                Camera.Dispose();
                Camera = null;
            }

            if (Texture != null)
            {
                Texture.Release();
                Texture.Dispose();
                Texture = null;
            }
        }
    }
}
#pragma warning restore CS0618