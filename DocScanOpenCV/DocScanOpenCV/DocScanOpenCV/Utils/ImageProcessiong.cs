using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocScanOpenCV.Utils
{
    public static class ImageProcessing
    {
        //public static void Initialize() => Cv2.Ini
        //public static string ProcessImage(string path)
        //{
        //    var processed = ProcessImage(new Mat(path));
        //    SaveImage(path, processed);
        //    processed.Dispose();
        //    return path;
        //}
        public static Stream ProcessImage(string path)
        {
            var processed = ProcessImage(new Mat(path));
            return processed.ToMemoryStream();
        }
        public static Stream ProcessImage(Stream stream)
        {
            var processed = ProcessImage(Mat.FromStream(stream, ImreadModes.Color));
            return processed.ToMemoryStream();
        }

        public static Mat ProcessImage(this Mat image)
        {
            var grayImage = ProccessToGrayContuour(image.Clone());
            var contoursOfDocument = FindContours_BiggestContourFloat(grayImage);
            grayImage.Dispose();
            var transformedImage = Transform(image, contoursOfDocument);
            return transformedImage;
            //return ProcessToPaperView(transformedImage, 255, 255);
        }
        public static Mat ProcessToPaperView(this Mat image) => ProcessToPaperView(image, 0, 0);
        public static Mat ProcessToPaperView(this Mat image, int thresh, int maxval)
        {
            var color = new Mat();
            var bilateralFilter = new Mat();
            var threshold = new Mat();
            Cv2.CvtColor(image.Clone(), color, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(color.Clone(), threshold, thresh, maxval, ThresholdTypes.Binary);
            Cv2.BilateralFilter(threshold.Clone(), bilateralFilter, 20, 20, 10);

            var output = (threshold).Clone();
            color.Dispose();
            bilateralFilter.Dispose();
            threshold.Dispose();

            return output;
        }
        public static Mat ProccessToGrayContuour(this Mat image)
        {
            image = image.CvtColor(ColorConversionCodes.BGR2GRAY);
            image = image.MedianBlur(3);
            image = image.Canny(75, 200);
            image = image.Dilate(null);
            return image;
        }

        public static Mat PreviewProcess(this Mat image)
        {
            var processingImage = image.Clone();
            processingImage = ProccessToGrayContuour(processingImage);
            var contour = FindContours_BiggestContourInt(processingImage);
            processingImage.Dispose();
            return DrawContour(image, contour);
        }

        public static async Task<Mat> ProccessToGrayContuourAsync(this Mat image)
        {
            return await Task.Run(() =>
            {
                lock (proccessToGrayContuourAsyncLockObject)
                {
                    image = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                    image = image.MedianBlur(3);
                    image = image.Canny(75, 200);
                    image = image.Dilate(null);
                    return image;
                }
            });
        }
        private static readonly object proccessToGrayContuourAsyncLockObject = new object();
        public static List<Point[]> FindContours_SortedContours(this Mat image)
        {
            Cv2.FindContours(image, out var foundedContour, out var hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            var sortedContour = foundedContour.OrderByDescending(ContourArea).ToArray();

            List<Point[]> result = new List<Point[]>();
            foreach (var contour in sortedContour)
            {
                var peri = Cv2.ArcLength(contour, true);

                var approx = Cv2.ApproxPolyDP(contour.AsEnumerable(), 0.015 * peri, true);

                result.Add(approx);
            }
            return result;
        }
        public static Point2f[] FindContours_BiggestContourFloat(this Mat image)
        {

            Cv2.FindContours(image, out var foundedContours, out var hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            var sortedContours = foundedContours.OrderByDescending(ContourArea);
            var contourOfDocument = sortedContours.First();
            var peri = Cv2.ArcLength(contourOfDocument, true);

            var approx = Cv2.ApproxPolyDP(contourOfDocument.AsEnumerable(), 0.015 * peri, true);

            var temp = new List<Point2f>();
            var output = new List<Point2f>();
            for (int i = 0; i < approx.Length; i++) temp.Add(approx[i]);

            return temp.ToArray();
        }
        public static Point[] FindContours_BiggestContourInt(this Mat image)
        {
            Console.WriteLine("123");
            Cv2.FindContours(image, out var foundedContours, out var hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            var sortedContours = foundedContours.OrderByDescending(ContourArea);
            var contourOfDocument = sortedContours.First();
            var peri = Cv2.ArcLength(contourOfDocument, true);

            var approx = Cv2.ApproxPolyDP(contourOfDocument.AsEnumerable(), 0.015 * peri, true);
            var hull = Cv2.ConvexHull(approx, true);
            var boundRect = Cv2.MinAreaRect(hull);
            return boundRect.Points().To64Point();
        }
        public static double ContourArea(this Point[] x) => Cv2.ContourArea(x, true);
        public static Mat DrawBound(this Mat image, Point[] points)
        {
            var rect = Cv2.MinAreaRect(points);
            var pointsRect = rect.Points();
            return image.DrawContour(pointsRect);
        }

        private static Mat DrawContour(this Mat image, Point2f[] pointsRect) => DrawContour(image, pointsRect.To64Point());

        public static Mat DrawContour(this Mat image, IEnumerable<IEnumerable<Point>> contours)
        {
            image.DrawContours( contours, -1, Scalar.Red, 5);
            return image;
        }
        public static Mat DrawContour(this Mat image, IEnumerable<Point> countour)
        {
            image.DrawContours(new List<IEnumerable<Point>> { countour }, -1, Scalar.Red, 5);
            return image;
        }
        public static Mat LoadImage(string filePath) => Cv2.ImRead(filePath);
        public static void SaveImage(this Mat imageToSave, string filePath) => Cv2.ImWrite(filePath, imageToSave);
        public static Mat Transform(this Mat inputImage, Point2f[] toTransform)
            => Transform(inputImage, toTransform, new Point2f[]
                {
                    new Point2f(inputImage.Width, 0),
                    new Point2f(inputImage.Width, inputImage.Height),
                    new Point2f(0, inputImage.Height),
                    new Point2f(0, 0),
                }, new Size(inputImage.Width, inputImage.Height));
        public static Mat Transform(this Mat inputImage, Point2f[] toTransform, Point2f[] destination, double width, double heigth)
            => Transform(inputImage, toTransform, destination, new Size(width, heigth));
        public static Mat Transform(this Mat inputImage, Point2f[] toTransform, Point2f[] destination, Size size)
        {
            var m = Cv2.GetPerspectiveTransform(toTransform, destination);
            inputImage = inputImage.WarpPerspective(m, size);

            m.Dispose();

            return inputImage;
        }
        public static Mat Rotate(this Mat image, double angle, Point2f? center = null, double scale = 1.0)
        {
            // grab the dimensions of the image
            var w = image.Width;
            var h = image.Height;

            // if the center is None, initialize it as the center of
            // the image
            if (center == null) center = new Point2f { X = w / 2, Y = h / 2 };

            // perform the rotation
            var rotationMatrix2d = Cv2.GetRotationMatrix2D(center.Value, angle, scale);

            var warpAffineResult = new Mat(new Size(h, w), image.Type());
            Cv2.WarpAffine(image, warpAffineResult, rotationMatrix2d, new Size((int)(w * scale), (int)(h * scale)));
            rotationMatrix2d.Dispose();
            // return the rotated image
            return warpAffineResult;
        }
        public static Mat Resize(this Mat image, double newWidth, double newHigh, InterpolationFlags inter = InterpolationFlags.Linear)
        {
            // grab the dimensions of the image
            var w = image.Width;
            var h = image.Height;

            var result = new Mat();
            Cv2.Resize(image, result, new Size(newWidth, newHigh), interpolation: inter);

            return result;
        }
        public static Point[] To64Point(this Point2f[] pointsRect)
        {
            var newTable = new Point[pointsRect.Length];
            for (int i = 0; i < pointsRect.Length; i++)
            {
                newTable[i] = pointsRect[i];
            }
            return newTable;
        }
    }
}
