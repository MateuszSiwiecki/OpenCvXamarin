using DocScanOpenCV.Utils;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using static Xamarin.Essentials.Permissions;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;
using OpenCvSharp;
using DocScanOpenCV.Helper;
using OpenCvSharp.Native;
using OpenCvSharp.Android;

namespace DocScanOpenCV
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        static List<Point2f> point2Fs = new List<Point2f>();

        // //(bl, tl, tr, br) 
        static Point2f[] srcPoints = new Point2f[] {
                new Point2f(0, 0),
                new Point2f(0, 0),
                new Point2f(0, 0),
                new Point2f(0, 0),
            };
        private AndroidCapture ewq;
        public MainPage()
        {
            InitializeComponent();

        }

        private async void Button_Clicked(object sender, EventArgs e)
        {

            return;
            box.IsVisible = true;
            stackloading.IsVisible = true;
            loading.IsVisible = true;
            loading.IsRunning = true;
            var status = await CheckAndRequestPermissionAsync(new Permissions.StorageRead());
            if (status != PermissionStatus.Granted)
            {
                // Notify user permission was denied
                return;
            }
            var status1 = await CheckAndRequestPermissionAsync(new Permissions.StorageWrite());
            if (status1 != PermissionStatus.Granted)
            {
                // Notify user permission was denied
                return;
            }
            await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                CompressionQuality = 75,
                PhotoSize = PhotoSize.Large
            });
            if (file != null)
            {
                Mat OriginalImage = new Mat(file.Path, ImreadModes.AnyColor);
                execute(OriginalImage);
                //var result = ImageProcessing.ProcessImage(OriginalImage);
                //Save(result);
            }
            else
            {
                box.IsVisible = false;
                loading.IsVisible = false;
                loading.IsRunning = false;
                stackloading.IsVisible = false;

            }
        }
        public void execute(Mat OriginalImage)
        {

            //clone image
            Mat modifiedImage = new Mat(OriginalImage.Rows, OriginalImage.Cols, OriginalImage.Type());
            OriginalImage.CopyTo(modifiedImage);


            //Step 1 Grayscale
            modifiedImage = modifiedImage.CvtColor(ColorConversionCodes.BGR2GRAY);
            modifiedImage = modifiedImage.Threshold(127, 255, ThresholdTypes.Binary);


            //Step 2 Blur the image
            //modifiedImage = modifiedImage.GaussianBlur(new Size(5, 5), 0);
            modifiedImage = modifiedImage.MedianBlur(3);


            //Step 3 find edges (Canny and Dilate)
            modifiedImage = modifiedImage.Canny(75, 200);

            // dilate canny output to remove potential
            // holes between edge segments
            modifiedImage = modifiedImage.Dilate(null);


            //Step 4 Find Contour with 4 points (rectangle) with lagest area (find the doc edges)

            HierarchyIndex[] hierarchyIndexes;
            Point[][] contours;
            modifiedImage.FindContours(out contours, out hierarchyIndexes, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            //find largest area with 4 points
            double largestarea = 0;
            var largestareacontourindex = 0;
            var contourIndex = 0;
            Point[] docEdgesPoints = null;

            //debug purpose, uncomment to see all contours captured by openCV
            //debug_showallcontours(OriginalImage, hierarchyIndexes, contours);


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

            //draw contour (debug purpose)
            Mat EdgingImage = new Mat(OriginalImage.Rows, OriginalImage.Cols, OriginalImage.Type());
            OriginalImage.CopyTo(EdgingImage);
            Cv2.DrawContours(
                   EdgingImage,
                   contours,
                   largestareacontourindex,
                   color: Scalar.Yellow,
                   thickness: 3,
                   lineType: LineTypes.Link8,
                   hierarchy: hierarchyIndexes,
                   maxLevel: int.MaxValue);

            //var contouredImage = OriginalImage.Clone();
            //Cv2.DrawContours(contouredImage, contours, -1, Scalar.Red, 5);
            //Save(EdgingImage);
            //return;

            //Steps 4.1 find the max size of contour area (entire image) 
            //to be used to check if the largest contour area is the doc edges (ratio)
            var imageSize = OriginalImage.Size().Height * OriginalImage.Size().Width;

            // Steps 5: apply the four point transform to obtain a top-down
            // view of the original image
            Mat transformImage = null;
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
            //if (Cv2.ContourArea(contours[largestareacontourindex]) < imageSize * 0.5)
            //{
            //    //if largest contour smaller than 50% of the picture, assume document edges not found
            //    //proceed with simple filter 

            //    transformImage = apply_doc_filters(OriginalImage);
            //}
            //else
            //{
            //    //doc closed edges detected, proceed tranformation

            //    //convert to point2f
            //    foreach (var item in docEdgesPoints)
            //    {
            //        point2Fs.Add(new Point2f(item.X, item.Y));
            //    }
            //    transformImage = transform(OriginalImage, point2Fs);
            //    if (transformImage != null)
            //    {

            //        //Step 6: grayscale it to give it that 'black and white' paper effect
            //        transformImage = apply_doc_filters(transformImage);
            //    }

            //}

            if (transformImage != null) Save(transformImage);



        }
        private void Save(Mat image)
        {
            var ms = image.ToMemoryStream();
            myimg.Source = ImageSource.FromStream(() => new MemoryStream(ms.ToArray()));
            box.IsVisible = false;
            stackloading.IsVisible = false;
            loading.IsVisible = false;
            loading.IsRunning = false;
        }
        private static Mat apply_doc_filters(Mat image)
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
        private static Mat transform(Mat OriginalImage, List<Point2f> pts)
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
                    Point2f[] dstPoints = new Point2f[]
                    {
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
        unsafe public static void convertYUV420_NV21toRGB565(byte* yuvIn, Int16* rgbOut, int width, int height, bool monochrome)
        {
            int size = width * height;
            int offset = size;
            int u, v, y1, y2, y3, y4;

            for (int i = 0, k = 0; i < size; i += 2, k += 2)
            {
                y1 = yuvIn[i];
                y2 = yuvIn[i + 1];
                y3 = yuvIn[width + i];
                y4 = yuvIn[width + i + 1];

                u = yuvIn[offset + k];
                v = yuvIn[offset + k + 1];
                u = u - 128;
                v = v - 128;

                if (monochrome)
                {
                    convertYUVtoRGB565Monochrome(y1, u, v, rgbOut, i);
                    convertYUVtoRGB565Monochrome(y2, u, v, rgbOut, (i + 1));
                    convertYUVtoRGB565Monochrome(y3, u, v, rgbOut, (width + i));
                    convertYUVtoRGB565Monochrome(y4, u, v, rgbOut, (width + i + 1));
                }
                else
                {
                    convertYUVtoRGB565(y1, u, v, rgbOut, i);
                    convertYUVtoRGB565(y2, u, v, rgbOut, (i + 1));
                    convertYUVtoRGB565(y3, u, v, rgbOut, (width + i));
                    convertYUVtoRGB565(y4, u, v, rgbOut, (width + i + 1));
                }

                if (i != 0 && (i + 2) % width == 0)
                    i += width;
            }
        }

        unsafe private static void convertYUVtoRGB565Monochrome(int y, int u, int v, Int16* rgbOut, int index)
        {
            rgbOut[index] = (short)(((y & 0xf8) << 8) |
                          ((y & 0xfc) << 3) |
                          ((y >> 3) & 0x1f));
        }

        unsafe private static void convertYUVtoRGB565(int y, int u, int v, Int16* rgbOut, int index)
        {
            int r = y + (int)1.402f * v;
            int g = y - (int)(0.344f * u + 0.714f * v);
            int b = y + (int)1.772f * u;
            r = r > 255 ? 255 : r < 0 ? 0 : r;
            g = g > 255 ? 255 : g < 0 ? 0 : g;
            b = b > 255 ? 255 : b < 0 ? 0 : b;
            rgbOut[index] = (short)(((b & 0xf8) << 8) |
                          ((g & 0xfc) << 3) |
                          ((r >> 3) & 0x1f));
        }
    }
}

