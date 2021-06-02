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
            var buffer = image.GetPlanes()[0].Buffer;
            byte[] bytes = new byte[buffer.Remaining()];
            buffer.Get(bytes);
            var bitmap = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);

            var canvas = owner.texture2.LockCanvas();
            canvas.DrawBitmap(bitmap, new Matrix(), new Paint());
            owner.texture2.UnlockCanvasAndPost(canvas);
        }
        // Saves a JPEG {@link Image} into the specified {@link File}.
        private class ImageSaver : Java.Lang.Object, IRunnable
        {
            // The JPEG image
            private Image mImage;

            // The file we save the image into.
            private File mFile;

            public ImageSaver(Image image, File file)
            {
                if (image == null)
                    throw new System.ArgumentNullException("image");
                if (file == null)
                    throw new System.ArgumentNullException("file");

                mImage = image;
                mFile = file;
            }

            public void Run()
            {
                ByteBuffer buffer = mImage.GetPlanes()[0].Buffer;
                byte[] bytes = new byte[buffer.Remaining()];
                buffer.Get(bytes);
                using (var output = new FileOutputStream(mFile))
                {
                    try
                    {
                        output.Write(bytes);
                    }
                    catch (IOException e)
                    {
                        e.PrintStackTrace();
                    }
                    finally
                    {
                        mImage.Close();
                    }
                }
            }
        }
    }
}