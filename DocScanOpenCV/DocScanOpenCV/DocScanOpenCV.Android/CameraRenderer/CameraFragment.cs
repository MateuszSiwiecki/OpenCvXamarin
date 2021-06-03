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

namespace CustomRenderer.Droid
{
    public class CameraFragment : Fragment, TextureView.ISurfaceTextureListener
    {
        ImageReader mImageReader;
        ImageAvailableListener mOnImageAvailableListener;
        public CameraCaptureListener mCaptureCallback;
        public int mState = STATE_PREVIEW;

        // Camera state: Showing camera preview.
        public const int STATE_PREVIEW = 0;
        // Camera state: Waiting for the focus to be locked.
        public const int STATE_WAITING_LOCK = 1;
        // Camera state: Waiting for the exposure to be precapture state.
        public const int STATE_WAITING_PRECAPTURE = 2;
        //Camera state: Waiting for the exposure state to be something other than precapture.
        public const int STATE_WAITING_NON_PRECAPTURE = 3;
        // Camera state: Picture was taken.
        public const int STATE_PICTURE_TAKEN = 4;

        public CameraDevice device;
        public CaptureRequest.Builder sessionBuilder;
        public CaptureRequest mPreviewRequest;
        public CameraCaptureSession session;
        public CameraTemplate cameraTemplate;
        CameraManager manager;

        bool cameraPermissionsGranted;
        bool busy;
        bool repeatingIsRunning;
        int sensorOrientation;
        string cameraId;
        LensFacing cameraType;

        Android.Util.Size previewSize;

        public HandlerThread backgroundThread;
        public Handler backgroundHandler = null;

        Java.Util.Concurrent.Semaphore captureSessionOpenCloseLock = new Java.Util.Concurrent.Semaphore(1);

        public CustomRenderer.Droid.AutoFitTextureView texture1;
        public CustomRenderer.Droid.AutoFitTextureView texture2;

        TaskCompletionSource<CameraDevice> initTaskSource;
        TaskCompletionSource<bool> permissionsRequested;

        CameraManager Manager => manager ??= (CameraManager)Context.GetSystemService(Context.CameraService);

        bool IsBusy
        {
            get => device == null || busy;
            set
            {
                busy = value;
            }
        }

        bool Available;

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

        public override Android.Views.View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) => inflater.Inflate(DocScanOpenCV.Droid.Resource.Layout.CameraFragment, null);

        public override void OnViewCreated(Android.Views.View view, Bundle savedInstanceState)
        {
            texture1 = view.FindViewById<CustomRenderer.Droid.AutoFitTextureView>(DocScanOpenCV.Droid.Resource.Id.cameratexture1);
            texture2 = view.FindViewById<CustomRenderer.Droid.AutoFitTextureView>(DocScanOpenCV.Droid.Resource.Id.cameratexture2); 
            mCaptureCallback = new CameraCaptureListener(this);
            mOnImageAvailableListener = new ImageAvailableListener(this, new 
                Java.IO.File(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "img.png")));
        }

        public override void OnPause()
        {
            CloseSession();
            StopBackgroundThread();
            base.OnPause();            
        }

        public override async void OnResume()
        {
            base.OnResume();

            StartBackgroundThread();
            if (texture1 is null || texture2 is null)
            {
                return;
            }
            if (texture1.IsAvailable)
            {
                View?.SetBackgroundColor(Color.Transparent);
                cameraTemplate = CameraTemplate.Preview;

                await RetrieveCameraDevice(force: true);
            }
            else
            {
                texture1.SurfaceTextureListener = this;
                texture2.SurfaceTextureListener = this;
            }
        }

        protected override void Dispose(bool disposing)
        {
            CloseDevice();
            base.Dispose(disposing);
        }

        #endregion

        #region Public methods

        public async Task RetrieveCameraDevice(bool force = false)
        {
            if (Context == null || (!force && initTaskSource != null))
            {
                return;
            }

            if (device != null)
            {
                CloseDevice();
            }

            await RequestCameraPermissions();

            if (!captureSessionOpenCloseLock.TryAcquire(2500, TimeUnit.Milliseconds))
            {
                throw new RuntimeException("Timeout waiting to lock camera opening.");
            }

            IsBusy = true;
            cameraId = GetCameraId();

            if (string.IsNullOrEmpty(cameraId))
            {
                IsBusy = false;
                captureSessionOpenCloseLock.Release();
                Console.WriteLine("No camera found");
            }
            else
            {
                try
                {
                    CameraCharacteristics characteristics = Manager.GetCameraCharacteristics(cameraId);
                    
                    StreamConfigurationMap map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);

                    previewSize = new Android.Util.Size(1920, 1080);
                    //previewSize = ChooseOptimalSize(map.GetOutputSizes(Class.FromType(typeof(SurfaceTexture))),
                        //texture1.Width, texture1.Height, GetMaxSize(map.GetOutputSizes((int)ImageFormatType.Jpeg)));
                    sensorOrientation = (int)characteristics.Get(CameraCharacteristics.SensorOrientation);
                    cameraType = (LensFacing)(int)characteristics.Get(CameraCharacteristics.LensFacing);


                    mImageReader = ImageReader.NewInstance(previewSize.Width, previewSize.Height, ImageFormatType.Jpeg, 2);
                    mImageReader.SetOnImageAvailableListener(mOnImageAvailableListener, backgroundHandler);


                    if (Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape)
                    {
                        texture1.SetAspectRatio(previewSize.Width, previewSize.Height);
                        texture2.SetAspectRatio(previewSize.Width, previewSize.Height);
                    }
                    else
                    {
                        texture1.SetAspectRatio(previewSize.Height, previewSize.Width);
                        texture2.SetAspectRatio(previewSize.Height, previewSize.Width);
                    }

                    initTaskSource = new TaskCompletionSource<CameraDevice>();
                    Manager.OpenCamera(cameraId, new CameraStateListener
                    {
                        OnOpenedAction = device => initTaskSource?.TrySetResult(device),
                        OnDisconnectedAction = device =>
                        {
                            initTaskSource?.TrySetResult(null);
                            CloseDevice(device);
                        },
                        OnErrorAction = (device, error) =>
                        {
                            initTaskSource?.TrySetResult(device);
                            Console.WriteLine($"Camera device error: {error}");
                            CloseDevice(device);
                        },
                        OnClosedAction = device =>
                        {
                            initTaskSource?.TrySetResult(null);
                            CloseDevice(device);
                        }
                    }, backgroundHandler);

                    captureSessionOpenCloseLock.Release();
                    device = await initTaskSource.Task;
                    initTaskSource = null;
                    if (device != null)
                    {
                        await PrepareSession();
                    }
                }
                catch (Java.Lang.Exception ex)
                {
                    Console.WriteLine("Failed to open camera.", ex);
                    Available = false;
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        public void UpdateRepeatingRequest()
        {
            if (session == null || sessionBuilder == null) return; 

            IsBusy = true;
            try
            {
                if (repeatingIsRunning) session.StopRepeating(); 

                sessionBuilder.Set(CaptureRequest.ControlMode, (int)ControlMode.Auto);
                sessionBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.On);
                session.SetRepeatingRequest(sessionBuilder.Build(), listener: null, backgroundHandler);
                repeatingIsRunning = true;
            }
            catch (Java.Lang.Exception error)
            {
                Console.WriteLine("Update preview exception.", error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        void StartBackgroundThread()
        {
            backgroundThread = new HandlerThread("CameraBackground");
            backgroundThread.Start();
            backgroundHandler = new Handler(backgroundThread.Looper);
        }

        void StopBackgroundThread()
        {
            if (backgroundThread == null) return; 

            backgroundThread.QuitSafely();
            try
            {
                backgroundThread.Join();
                backgroundThread = null;
                backgroundHandler = null;
            }
            catch (InterruptedException ex)
            {
                Console.WriteLine("Error stopping background thread.", ex);
            }
        }        

        Android.Util.Size GetMaxSize(Android.Util.Size[] imageSizes)
        {
            Android.Util.Size maxSize = null;
            long maxPixels = 0;
            for (int i = 0; i < imageSizes.Length; i++)
            {
                long currentPixels = imageSizes[i].Width * imageSizes[i].Height;
                if (currentPixels > maxPixels)
                {
                    maxSize = imageSizes[i];
                    maxPixels = currentPixels;
                }
            }
            return maxSize;
        }

        Android.Util.Size ChooseOptimalSize(Android.Util.Size[] choices, int width, int height, Android.Util.Size aspectRatio)
        {
            List<Android.Util.Size> bigEnough = new List<Android.Util.Size>();
            int w = aspectRatio.Width;
            int h = aspectRatio.Height;

            foreach (Android.Util.Size option in choices)
            {
                if (option.Height == option.Width * h / w && option.Width >= width && option.Height >= height)
                {
                    bigEnough.Add(option);
                }
            }

            if (bigEnough.Count > 0)
            {
                int minArea = bigEnough.Min(s => s.Width * s.Height);
                return bigEnough.First(s => s.Width * s.Height == minArea);
            }
            else
            {
                Console.WriteLine("Couldn't find any suitable preview size.");
                return choices[0];
            }
        }

        string GetCameraId()
        {
            string[] cameraIdList = Manager.GetCameraIdList();
            if (cameraIdList.Length == 0)
            {
                return null;
            }

            string FilterCameraByLens(LensFacing lensFacing)
            {
                foreach (string id in cameraIdList)
                {
                    CameraCharacteristics characteristics = Manager.GetCameraCharacteristics(id);
                    if (lensFacing == (LensFacing)(int)characteristics.Get(CameraCharacteristics.LensFacing))
                    {
                        return id;
                    }
                }
                return null;
            }

            return (Element.Camera == CameraOptions.Front) ? FilterCameraByLens(LensFacing.Front) : FilterCameraByLens(LensFacing.Back);
        }

        async Task PrepareSession()
        {
            IsBusy = true;
            try
            {
                CloseSession();
                sessionBuilder = device.CreateCaptureRequest(cameraTemplate);
                
                List<Surface> surfaces = new List<Surface>();
                if (texture1.IsAvailable && previewSize != null)
                {
                    var texture1 = this.texture1.SurfaceTexture;


                    texture1.SetDefaultBufferSize(previewSize.Width, previewSize.Height);
                    texture2.SurfaceTexture.SetDefaultBufferSize(previewSize.Width, previewSize.Height);
                    Surface previewSurface1 = new Surface(texture1);
                    surfaces.Add(previewSurface1);
                    surfaces.Add(mImageReader.Surface);
                    sessionBuilder.AddTarget(previewSurface1);
                    sessionBuilder.AddTarget(mImageReader.Surface);
                }

                TaskCompletionSource<CameraCaptureSession> tcs = new TaskCompletionSource<CameraCaptureSession>();
                device.CreateCaptureSession(surfaces, new CameraCaptureStateListener
                {
                    OnConfigureFailedAction = captureSession =>
                    {
                        tcs.SetResult(null);
                        Console.WriteLine("Failed to create capture session.");
                    },
                    OnConfiguredAction = captureSession => tcs.SetResult(captureSession)
                }, null);

                session = await tcs.Task;
                if (session != null)
                {
                    UpdateRepeatingRequest();
                }
            }
            catch (Java.Lang.Exception ex)
            {
                Available = false;
                Console.WriteLine("Capture error.", ex);
            }
            finally
            {
                Available = session != null;
                IsBusy = false;
            }
        }

        void CloseSession()
        {
            repeatingIsRunning = false;
            if (session == null)
            {
                return;
            }

            try
            {
                session.StopRepeating();
                session.AbortCaptures();
                session.Close();
                session.Dispose();
                session = null;
            }
            catch (CameraAccessException ex)
            {
                Console.WriteLine("Camera access error.", ex);
            }
            catch (Java.Lang.Exception ex)
            {
                Console.WriteLine("Error closing device.", ex);
            }
        }

        void CloseDevice(CameraDevice inputDevice)
        {
            if (inputDevice == device)
            {
                CloseDevice();
            }
        }

        void CloseDevice()
        {
            CloseSession();

            try
            {
                if (sessionBuilder != null)
                {
                    sessionBuilder.Dispose();
                    sessionBuilder = null;
                }
                if (device != null)
                {
                    device.Close();
                    device = null;
                }
                if (mImageReader != null)
                {
                    mImageReader.Close();
                    mImageReader = null;
                }
            }
            catch (Java.Lang.Exception error)
            {
                Console.WriteLine("Error closing device.", error);
            }
        }

        void ConfigureTransform(int viewWidth, int viewHeight)
        {
            if (texture1 == null || previewSize == null || previewSize.Width == 0 || previewSize.Height == 0)
            {
                return;
            }

            var matrix = new Matrix();
            var viewRect = new RectF(0, 0, viewWidth, viewHeight);
            var bufferRect = new RectF(0, 0, previewSize.Height, previewSize.Width);
            var centerX = viewRect.CenterX();
            var centerY = viewRect.CenterY();
            bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
            matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);
            matrix.PostRotate(GetCaptureOrientation(), centerX, centerY);
            texture1.SetTransform(matrix);
            texture2.SetTransform(matrix);
        }

        int GetCaptureOrientation()
        {
            int frontOffset = cameraType == LensFacing.Front ? 90 : -90;
            return (360 + sensorOrientation - GetDisplayRotationDegrees() + frontOffset) % 360;
        }

        int GetDisplayRotationDegrees() =>
            GetDisplayRotation() switch
            {
                SurfaceOrientation.Rotation90 => 90,
                SurfaceOrientation.Rotation180 => 180,
                SurfaceOrientation.Rotation270 => 270,
                _ => 0
            };

        SurfaceOrientation GetDisplayRotation() => Android.App.Application.Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>().DefaultDisplay.Rotation;
        public void RunPrecaptureSequence()
        {
            try
            {
                // This is how to tell the camera to trigger.
                stillCaptureBuilder.Set(CaptureRequest.ControlAePrecaptureTrigger, (int)ControlAEPrecaptureTrigger.Start);
                // Tell #mCaptureCallback to wait for the precapture sequence to be set.
                mState = STATE_WAITING_PRECAPTURE;
                session.Capture(stillCaptureBuilder.Build(), mCaptureCallback, backgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }
        private CaptureRequest.Builder stillCaptureBuilder;
        public void CaptureStillPicture()
        {
            try
            {
                var activity = Activity;
                if (null == activity || null == device)
                {
                    return;
                }
                // This is the CaptureRequest.Builder that we use to take a picture.
                if (stillCaptureBuilder == null)
                    stillCaptureBuilder = device.CreateCaptureRequest(CameraTemplate.StillCapture);

                stillCaptureBuilder.AddTarget(mImageReader.Surface);
                // Use the same AE and AF modes as the preview.
                stillCaptureBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousPicture);

                // Orientation
                int rotation = (int)activity.WindowManager.DefaultDisplay.Rotation;
               // stillCaptureBuilder.Set(CaptureRequest.JpegOrientation, 90);

                //session.StopRepeating();
                session.Capture(stillCaptureBuilder.Build(), new CameraCaptureStillPictureSessionCallback(this), null);
                
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        public void UnlockFocus()
        {
            try
            {
                // Reset the auto-focus trigger
                sessionBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Cancel);
                session.Capture(sessionBuilder.Build(), mCaptureCallback,
                        backgroundHandler);
                // After this, the camera will go back to the normal state of preview.
                mState = STATE_PREVIEW;
                session.SetRepeatingRequest(sessionBuilder.Build(), mCaptureCallback,
                        backgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }
        #region Permissions

        async Task RequestCameraPermissions()
        {
            if (permissionsRequested != null)
            {
                await permissionsRequested.Task;
            }

            List<string> permissionsToRequest = new List<string>();
            cameraPermissionsGranted = ContextCompat.CheckSelfPermission(Context, Manifest.Permission.Camera) == Permission.Granted;
            if (!cameraPermissionsGranted)
            {
                permissionsToRequest.Add(Manifest.Permission.Camera);
            }

            if (permissionsToRequest.Count > 0)
            {
                permissionsRequested = new TaskCompletionSource<bool>();
                RequestPermissions(permissionsToRequest.ToArray(), requestCode: 1);
                await permissionsRequested.Task;
                permissionsRequested = null;
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode != 1)
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                return;
            }

            for (int i=0; i < permissions.Length; i++)
            {
                if (permissions[i] == Manifest.Permission.Camera)
                {
                    cameraPermissionsGranted = grantResults[i] == Permission.Granted;
                    if (!cameraPermissionsGranted)
                    {
                        Console.WriteLine("No permission to use the camera.");
                    }
                }
            }
            permissionsRequested?.TrySetResult(true);
        }

        #endregion

        #region TextureView.ISurfaceTextureListener

        async void TextureView.ISurfaceTextureListener.OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            
            View?.SetBackgroundColor(Element.BackgroundColor.ToAndroid());
            cameraTemplate = CameraTemplate.Preview;
            await RetrieveCameraDevice();           
        }

        bool TextureView.ISurfaceTextureListener.OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            CloseDevice();
            return true;
        }

        void TextureView.ISurfaceTextureListener.OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height) => ConfigureTransform(width, height);

        void TextureView.ISurfaceTextureListener.OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }

        #endregion
    }
}
