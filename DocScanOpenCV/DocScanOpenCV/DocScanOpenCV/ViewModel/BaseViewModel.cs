using OpenCvSharp;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace DocScanOpenCV.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public BaseViewModel()
        {
            BackCommand = new Command(async () => await PerformCommand(NavigationPage.PopAsync));
            TakePhotoToProcess = new Command(async () => await PerformCommand(ProcessImageAsync));
        }
        #region OpenCvAbstractions

        public List<Point2f> point2Fs = new List<Point2f>();

        // //(bl, tl, tr, br) 
        public Point2f[] srcPoints = new Point2f[]
        {
                new Point2f(0, 0),
                new Point2f(0, 0),
                new Point2f(0, 0),
                new Point2f(0, 0),
        };
        public abstract Mat Excec(Mat oryginalImage);

        public Mat CloneOryginalImageToProcess(Mat OriginalImage)
        {
            //clone image
            Mat modifiedImage = new Mat(OriginalImage.Rows, OriginalImage.Cols, OriginalImage.Type());
            OriginalImage.CopyTo(modifiedImage);


            //Step 1 Grayscale
            modifiedImage = modifiedImage.CvtColor(ColorConversionCodes.BGR2GRAY);


            //Step 2 Blur the image
            //modifiedImage = modifiedImage.GaussianBlur(new Size(5, 5), 0);
            modifiedImage = modifiedImage.MedianBlur(3);


            //Step 3 find edges (Canny and Dilate)
            modifiedImage = modifiedImage.Canny(75, 200);

            // dilate canny output to remove potential
            // holes between edge segments
            modifiedImage = modifiedImage.Dilate(null);

            return modifiedImage;
        }
        public Point[][] FindContours(Mat image, out Point[][] contours, out HierarchyIndex[] hierarchyIndexes)
        {
            //Step 4 Find Contour with 4 points (rectangle) with lagest area (find the doc edges)

            image.FindContours(out contours, out hierarchyIndexes, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            return contours;
        }
        public int FindEdges(Point[][] contours, HierarchyIndex[] hierarchyIndexes, out Point[] docEdgesPoints)
        {
            //find largest area with 4 points
            double largestarea = 0;
            var largestareacontourindex = 0;
            var contourIndex = 0;
            docEdgesPoints = null;

            foreach (var cont in contours)
            {
                var peri = Cv2.ArcLength(cont, true); //only take contour area that are closed shape no gap 
                var approx = Cv2.ApproxPolyDP(cont, 0.02 * peri, true);

                //TODO: we need to check and to not tranform if the contour size is larger or = to the picture size, 
                //or smaller than certain size means lagest contour detected is incorrect. then we output original image without transform
                if (approx.Length == 4 && Cv2.ContourArea(contours[contourIndex]) > largestarea)
                {
                    largestarea = Cv2.ContourArea(contours[contourIndex]);
                    largestareacontourindex = contourIndex;
                    docEdgesPoints = approx;
                }

                contourIndex = hierarchyIndexes[contourIndex].Next;
            }
            return largestareacontourindex;
        }
        public Mat DrawContour(Mat image, IEnumerable<IEnumerable<Point>> contours)
        {
            Cv2.DrawContours(image, contours, -1, Scalar.Red, 5);
            return image;
        }
        public Mat DrawContour(Mat image, IEnumerable<Point> countour) 
            => DrawContour(image, new List<IEnumerable<Point>> { countour });
        public Mat DrawContour(Mat image, Point[][] contours, int largestareacontourindex, IEnumerable<HierarchyIndex> hierarchyIndexes)
        {
            Cv2.DrawContours(
                   image,
                   contours,
                   largestareacontourindex,
                   color: Scalar.Yellow,
                   thickness: 7,
                   lineType: LineTypes.Link8,
                   hierarchy: hierarchyIndexes,
                   maxLevel: int.MaxValue);
            return image;
        }
        public Mat Transform(Mat OriginalImage, Point[][] contours, int largestareacontourindex, IEnumerable<HierarchyIndex> hierarchyIndexes, Point[] docEdgesPoints)
        {

            //Steps 4.1 find the max size of contour area (entire image) 
            //to be used to check if the largest contour area is the doc edges (ratio)
            var imageSize = OriginalImage.Size().Height * OriginalImage.Size().Width;

            // Steps 5: apply the four point transform to obtain a top-down
            // view of the original image
            Mat transformImage = null;
            if (Cv2.ContourArea(contours[largestareacontourindex]) < imageSize * 0.5)
            {
                //if largest contour smaller than 50% of the picture, assume document edges not found
                //proceed with simple filter 

                foreach (var item in docEdgesPoints)
                {
                    point2Fs.Add(new Point2f(item.X, item.Y));
                }
                transformImage = transform(OriginalImage, point2Fs);
                if (transformImage != null)
                {

                    //Step 6: grayscale it to give it that 'black and white' paper effect
                    transformImage = apply_doc_filters(transformImage);
                }
            }
            else
            {
                //doc closed edges detected, proceed tranformation

                //convert to point2f
                foreach (var item in docEdgesPoints)
                {
                    point2Fs.Add(new Point2f(item.X, item.Y));
                }
                transformImage = transform(OriginalImage, point2Fs);
                if (transformImage != null)
                {
                    //Step 6: grayscale it to give it that 'black and white' paper effect
                    transformImage = apply_doc_filters(transformImage);
                }

            }
            return transformImage;
        }

        public Mat transform(Mat OriginalImage, List<Point2f> pts)
        {
            Mat dst = null;
            try
            {
                if (pts.Count == 4)
                {
                    //need to sort the points to follow order (bl, tl, tr, br), findcontours will return random order
                    var sortedpts = DocScanOpenCV.Helper.Helper.Sort(pts);

                    // calc new image height & width
                    // compute the width of the new image, which will be the
                    // maximum distance
                    var widthA = sortedpts[2].X - sortedpts[1].X;
                    var widthB = sortedpts[3].X - sortedpts[0].X;
                    var maxWidth = Math.Max((int)widthA, (int)widthB);

                    var heightA = sortedpts[1].Y - sortedpts[0].Y;
                    var heightB = sortedpts[2].Y - sortedpts[3].Y;
                    var maxHeight = Math.Max((int)heightA, (int)heightB);

                    srcPoints = sortedpts.ToArray();

                    //new output image size
                    //(tl, tr, br, bl)
                    Point2f[] dstPoints = new Point2f[] {
                    new Point2f(0, 0),
                    new Point2f(0, maxHeight - 1),
                    new Point2f(maxWidth - 1, maxHeight - 1),
                    new Point2f(maxWidth - 1, 0),
                };


                    var matrix = Cv2.GetPerspectiveTransform(srcPoints, dstPoints);
                    dst = new Mat(new Size(maxWidth, maxHeight), MatType.CV_8UC3);
                    Cv2.WarpPerspective(OriginalImage, dst, matrix, dst.Size());
                    point2Fs.Clear();

                }
            }
            catch { }

            return dst;
        }
        public Mat apply_doc_filters(Mat image)
        {
            //if closed rectangle of the document cant be detected then we will not transform the image but just apply simple filter to make it look like scanned doc

            //apply grayscale
            //Step 6: grayscale it to give it that 'black and white' paper effect
            image = image.CvtColor(ColorConversionCodes.BGR2GRAY);
            //transformImage = transformImage.Threshold(127, 255, ThresholdTypes.Binary);
            //transformImage = transformImage.Dilate(null);
            image = image.AdaptiveThreshold(255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 17, 11);

            ////add a border to the image to act as border of the doc
            //modifiedImage = modifiedImage.CopyMakeBorder(5, 5, 5, 5, BorderTypes.Constant, value: Scalar.Black);

            return image;
        }
        #endregion OpenCvAbstractions


        #region BaseViewModelFundamentals
        public ICommand BackCommand { get; set; }
        public ICommand TakePhotoToProcess { get; set; }
        private bool processingImage;

        public bool ProcessingImage
        {
            get { return processingImage; }
            set => ChangeValue(ref processingImage, value);
        }


        protected static volatile bool CommandExecuting = false;
        public NavigationPage NavigationPage => Application.Current.MainPage as NavigationPage;

        protected void ChangeValue<T>(ref T changingProp, T newValue, [CallerMemberName] string propertyName = null)
        {
            changingProp = newValue;
            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool CanExecuteCommand() => !CommandExecuting;
        protected async Task PerformCommand(Func<Task> action)
        {
            if (CommandExecuting) return;
            CommandExecuting = true;
            await action.Invoke();
            CommandExecuting = false;
        }
        protected async Task PerformCommand(Func<object, Task> action, object objectToPass)
        {
            if (CommandExecuting) return;
            CommandExecuting = true;
            await action.Invoke(objectToPass);
            CommandExecuting = false;
        }
        protected async Task PerformCommand(Func<object, object, Task> action, object objectToPass1, object objectToPass2)
        {
            if (CommandExecuting) return;
            CommandExecuting = true;
            await action.Invoke(objectToPass1, objectToPass2);
            CommandExecuting = false;
        }
        #endregion BaseViewModelFundamentals
        #region MediaPicker
        public async Task ProcessImageAsync()
        {
            ProcessingImage = true;
            var file = await TakePhotoAsync();
            if (file != null)
            {
                try
                {
                    Mat OriginalImage = new Mat(file.Path, ImreadModes.AnyColor);
                    var image = Excec(OriginalImage);
                    if (image != null)
                    {

                    }
                }
                catch(Exception e)
                {

                }
            }
            ProcessingImage = false;
        }
        public async Task<MediaFile> TakePhotoAsync()
        {
            ProcessingImage = true;
            var status = await CheckAndRequestPermissionAsync(new Permissions.StorageRead());
            if (status != PermissionStatus.Granted)
            {
                // Notify user permission was denied
                return null;
            }
            var status1 = await CheckAndRequestPermissionAsync(new Permissions.StorageWrite());
            if (status1 != PermissionStatus.Granted)
            {
                // Notify user permission was denied
                return null;
            }
            await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                CompressionQuality = 100,
                PhotoSize = PhotoSize.Full
            });
            return file;
        }
        public async Task<PermissionStatus> CheckAndRequestPermissionAsync<T>(T permission)
          where T : BasePermission
        {
            var status = await permission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await permission.RequestAsync();
            }

            return status;
        }
        #endregion MediaPicker
    }
}
