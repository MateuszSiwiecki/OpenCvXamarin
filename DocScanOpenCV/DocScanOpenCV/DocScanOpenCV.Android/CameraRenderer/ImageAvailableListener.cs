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
        public ImageAvailableListener(CameraFragment fragment, File file)
        {
            if (fragment == null)
                throw new System.ArgumentNullException("fragment");
            if (file == null)
                throw new System.ArgumentNullException("file");

            owner = fragment;
            this.file = file;
        }

        private readonly File file;
        private readonly CameraFragment owner;

        //public File File { get; private set; }
        //public Camera2BasicFragment Owner { get; private set; }

        public void OnImageAvailable(ImageReader reader)
        {
            var image = reader.AcquireLatestImage();
            if (image == null) return;
            var planes = image.GetPlanes();
            
            var buffer = planes[0].Buffer;
            buffer.Rewind();
            byte[] bytes = new byte[buffer.Capacity()];
            buffer.Get(bytes);


            //var output = ProcessImage(bytes);
            var bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
            //output.Dispose();
            if (bitmap == null)
            {
                image.Close();
                return;
            }
            
            var canvas = owner.texture2.LockCanvas();
            var matrix = new Matrix();
            var width = canvas.Width;
            var heigth = canvas.Height;
           // matrix.SetRectToRect(new RectF(0, 0, width, heigth), new RectF(0, 0, width, heigth), Matrix.ScaleToFit.Fill);
            matrix.PostRotate(90, width / 2, heigth / 2);
            canvas.DrawBitmap(bitmap, matrix, new Paint());
            
            owner.texture2.UnlockCanvasAndPost(canvas);
            owner.texture2.SetOpaque(true);
            Paint mpaintTexture = new Paint();
            owner.texture2.SetLayerType(LayerType.Hardware, mpaintTexture);
            owner.texture2.SetLayerPaint(mpaintTexture);
            owner.texture2.SetOpaque(false);
            image.Close();

        }

        private Mat ProcessImage(byte[] bytes)
        {
            Mat matImage = Mat.FromImageData(bytes);
            matImage = matImage.CvtColor(ColorConversionCodes.BGR2GRAY);
            return matImage;
        }
    }
}