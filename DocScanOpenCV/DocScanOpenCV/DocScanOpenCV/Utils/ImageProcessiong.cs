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
        public static Mat ProcessImage(Mat imageToProcess, Point[] foundedRect)
        {
            imageToProcess = imageToProcess.Transform(foundedRect.To32Point().ToList());
            imageToProcess = imageToProcess.CvtColor(ColorConversionCodes.BGR2GRAY);
            //imageToProcess = imageToProcess.Erode(null);
            //imageToProcess = imageToProcess.Dilate(null);
            //imageToProcess = imageToProcess.AdaptiveThreshold(200, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 3);
            imageToProcess = imageToProcess.AdaptiveThreshold(255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 17, 11);
            //imageToProcess = imageToProcess.MedianBlur(3);
           // imageToProcess = imageToProcess.Erode(null);
            return imageToProcess;
        }

        public static Mat ProcessImage(this Mat image)
        {
            var grayImage = ProccessToGrayContuour(image.Clone());
            var contoursOfDocument = FindContours_BiggestContourFloat(grayImage);
            grayImage.Dispose();
            var transformedImage = Transform(image, contoursOfDocument.ToList());
            return transformedImage;
            //return ProcessToPaperView(transformedImage, 255, 255);
        }
        public static Mat ProccessToGrayContuour(this Mat image)
        {
            image = image.CvtColor(ColorConversionCodes.BGR2GRAY);
            image = image.Canny(50, 150);
            //image = image.MedianBlur(5);
            image = image.Dilate(null);
            return image;
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
        public static double ContourArea(this Point[] x) => Cv2.ContourArea(x, true); public static Mat DrawContourAvrg(this Mat image, List<Point[]> contours)
        {
            contours = contours.Where(x => x.Length == 4).ToList();
            var average = new Point[4];
            foreach (var contour in contours)
            {
                for (int i = 0; i < contour.Length; i++)
                {
                    average[i].X += contour[i].X;
                    average[i].Y += contour[i].Y;
                }
            }
            for (int i = 0; i < average.Length; i++)
            {
                average[i].X /= contours.Count;
                average[i].Y /= contours.Count;
            }
            image = image.DrawContour(average);
            return image;
        }


        public static Mat DrawAllContours(this Mat image, IEnumerable<IEnumerable<Point>> contours)
        {
            image.DrawContours( contours, -1, Scalar.Red, 5);
            return image;
        }
        private static Mat DrawContour(this Mat image, Point2f[] pointsRect) => DrawContour(image, pointsRect.To64Point());
        public static Mat DrawContour(this Mat image, IEnumerable<Point> countour)
        {
            image.DrawContours(new List<IEnumerable<Point>> { countour }, -1, Scalar.Red, 5);
            return image;
        }
        public static Mat Transform(this Mat inputImage, List<Point2f> toTransform)
            => Transform(inputImage, toTransform, new List<Point2f>
                {
                    new Point2f(0, 0),
                    new Point2f(inputImage.Width, 0),
                    new Point2f(inputImage.Width, inputImage.Height),
                    new Point2f(0, inputImage.Height),
                }, new Size(inputImage.Width, inputImage.Height));
        public static Mat Transform(this Mat inputImage, List<Point2f> foundedBound, List<Point2f> toTransform, Size size)
        {
            var src = new Point2f[4];

            var minDistance = foundedBound.Min(x => Point2f.Distance(x, toTransform[0]));
            var firstCorner = foundedBound.FirstOrDefault(x => Point2f.Distance(x, toTransform[0]) == minDistance);
            var startIndex = foundedBound.IndexOf(firstCorner);
            for (int i = 0; i < 4; i++)
            {
                src[i] = foundedBound[(startIndex + i) % 4];
            }

            var m = Cv2.GetPerspectiveTransform(src, toTransform);
            inputImage = inputImage.WarpPerspective(m, size);

            m.Dispose();

            return inputImage;
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
        public static Point2f[] To32Point(this Point[] pointsRect)
        {
            var newTable = new Point2f[pointsRect.Length];
            for (int i = 0; i < pointsRect.Length; i++)
            {
                newTable[i] = pointsRect[i];
            }
            return newTable;
        }
    }
}
