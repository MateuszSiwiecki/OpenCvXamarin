using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Media;
using Java.IO;
using Java.Lang;
using Java.Nio;
using CustomRenderer.Droid;
using Android.Graphics;
using OpenCvSharp;

namespace CustomRenderer.Droid
{
    public class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        OpenCvSharp.Android.NativeBinding binding;
        OpenCvSharp.Android.AndroidCapture capture;
        public ImageAvailableListener(CameraFragment fragment, File file)
        {
        }

        private void Capture_FrameReady(object sender, OpenCvSharp.Native.FrameArgs e)
        {
            binding.ImShow("qwe", e.Mat);
            e.Mat.Dispose();
        }

        private readonly File file;
        private readonly CameraFragment owner;

        //public File File { get; private set; }
        //public Camera2BasicFragment Owner { get; private set; }

        public void OnImageAvailable(ImageReader reader)
        {
        }

        private Mat ProcessImage(byte[] bytes)
        {
            Mat matImage = Mat.FromImageData(bytes);
            matImage = matImage.CvtColor(ColorConversionCodes.BGR2GRAY);
            //matImage = DocScanOpenCV.Utils.ImageProcessing.Rotate(matImage, -90);
            return matImage;
        }
    }
}