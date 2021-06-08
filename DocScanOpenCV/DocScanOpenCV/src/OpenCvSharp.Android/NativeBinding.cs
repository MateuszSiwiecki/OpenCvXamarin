﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using OpenCvSharp.Native;
using Java.Nio;

namespace OpenCvSharp.Android
{
    public class NativeBinding : NativeBindings
    {
        public static NativeBinding K => (NativeBinding)Kernal;

        public static void Init(Context context, Activity activity, ImageView imShowTarget = null)
        {
            Kernal = new NativeBinding(context, activity, imShowTarget);
        }

        public Context AppContext;
        public Activity MainActivity;
        public ImageView ImShowTarget;

        public NativeBinding(Context context, Activity activity, ImageView imShowTarget = null)
        {
            AppContext = context;
            MainActivity = activity;
            ImShowTarget = imShowTarget;
        }

        Bitmap imShowBitmap;
        byte[] imShowBuffer;
        object imShowLocker = new object();
        public override void ImShow(string name, Mat m) => ImShow(name, m, ImShowTarget, imShowLocker);
        public void ImShow(string name, Mat m, ImageView tagetView) => ImShow(name, m, tagetView, new object());
        private void ImShow(string name, Mat m, ImageView tagetView, object lockObject)
        {
            if (tagetView != null)
            {
                lock (lockObject)
                {
                    CvProfiler.Start($"imshow {name}");
                    if (imShowBitmap == null)
                    {
                        imShowBitmap = Bitmap.CreateBitmap(m.Width, m.Height, Bitmap.Config.Argb8888);
                    }
                    else if (imShowBitmap.Width != m.Width && imShowBitmap.Height != m.Height)
                    {
                        //imShowBitmap.Recycle();
                        //imShowBitmap.Dispose();
                        imShowBitmap = Bitmap.CreateBitmap(m.Width, m.Height, Bitmap.Config.Argb8888);
                    }

                    using (Mat mat = new Mat())
                    {
                        Cv2.CvtColor(m, mat, ColorConversionCodes.BGR2RGBA);

                        var bufLen = mat.Channel * mat.Total();
                        if (imShowBuffer == null || imShowBuffer.Length != bufLen)
                        {
                            imShowBuffer = new byte[bufLen];
                        }
                        mat.GetArray(0, 0, imShowBuffer);

                        using (var raw = ByteBuffer.Wrap(imShowBuffer))
                        {
                            imShowBitmap.CopyPixelsFromBuffer(raw);
                        }

                        MainActivity.RunOnUiThread(() =>
                        {
                            tagetView.SetImageBitmap(imShowBitmap);
                        });
                    }
                    CvProfiler.End($"imshow {name}");
                }
            }
        }
        public void ImShow(string name, Mat m, TextureView tagetView) => ImShow(name, m, tagetView, new object());
        private void ImShow(string name, Mat m, TextureView tagetView, object lockObject)
        {
            if (tagetView != null)
            {
                lock (lockObject)
                {
                    CvProfiler.Start($"imshow {name}");
                    if (imShowBitmap == null)
                    {
                        imShowBitmap = Bitmap.CreateBitmap(m.Width, m.Height, Bitmap.Config.Argb8888);
                    }
                    else if (imShowBitmap.Width != m.Width && imShowBitmap.Height != m.Height)
                    {
                        //imShowBitmap.Recycle();
                        //imShowBitmap.Dispose();
                        imShowBitmap = Bitmap.CreateBitmap(m.Width, m.Height, Bitmap.Config.Argb8888);
                    }

                    using (Mat mat = new Mat())
                    {
                        Cv2.CvtColor(m, mat, ColorConversionCodes.BGR2RGBA);

                        var bufLen = mat.Channel * mat.Total();
                        if (imShowBuffer == null || imShowBuffer.Length != bufLen)
                        {
                            imShowBuffer = new byte[bufLen];
                        }
                        mat.GetArray(0, 0, imShowBuffer);

                        using (var raw = ByteBuffer.Wrap(imShowBuffer))
                        {
                            imShowBitmap.CopyPixelsFromBuffer(raw);
                        }

                        MainActivity.RunOnUiThread(() =>
                        {
                            var canvas = tagetView.LockCanvas();
                            canvas.DrawBitmap(imShowBitmap, new Matrix(), new Paint());
                            tagetView.UnlockCanvasAndPost(canvas);
                        });
                    }
                    CvProfiler.End($"imshow {name}");
                }
            }
        }

        char keyPending = (char)255;
        public void SendKey(char key)
        {
            keyPending = key;
        }

        public override int WaitKey(int sleep = 0)
        {
            var ret = keyPending;

            Sleep(sleep);

            keyPending = (char)255;
            return ret;
        }

        public override void Sleep(int sleep = 0)
        {
            if (sleep <= 0)
            {
                while (true)
                {
                    if (keyPending != 255)
                    {
                        Sleep(1);
                        break;
                    }
                }
            }
            else
            {
                System.Threading.Thread.Sleep(sleep);
            }
        }

        public override Capture NewCapture(int index)
        {
            return new AndroidCapture(index);
        }

        public override Capture NewCapture(string file)
        {
            return new AndroidCapture(file);
        }

        public override void NamedWindow(string winname, WindowMode flags)
        {

        }

        public override void DestroyAllWindows()
        {

        }
    }
}