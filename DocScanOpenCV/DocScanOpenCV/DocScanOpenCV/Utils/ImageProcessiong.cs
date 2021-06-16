using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MoreLinq.Extensions.MaxByExtension;

namespace DocScanOpenCV.Utils
{
    public static class ImageProcessing
    {
        public static Mat LoadImage(string filePath) => Cv2.ImRead(filePath);
        public static void SaveImage(string filePath, Mat imageToSave) => Cv2.ImWrite(filePath, imageToSave);
        public static Mat ProcessImage(Mat imageToProcess, Point[] foundedRect)
        {
            imageToProcess = imageToProcess.Transform(foundedRect.To32Point().ToList());
            imageToProcess = imageToProcess.Sharpness();
            imageToProcess = imageToProcess.ChangeGamma(0.8);
            return imageToProcess;
        }
        public static Mat DrawTransparentContour(this Mat src, Point[] toDraw)
        {
            using var baseImageCopy = src.Clone();
            src = src.DrawContour(toDraw);

            Cv2.AddWeighted(src, 1, baseImageCopy, -1, 0, src);
            using var gray = src.CvtColor(ColorConversionCodes.BGR2GRAY);
            using var alphaChannel = gray.Threshold(10, 255, ThresholdTypes.Binary);

            var channels = src.Split().ToList();
            channels[0] = Mat.Zeros(new Size(channels[0].Cols, channels[0].Rows), MatType.CV_8UC1);
            channels[1] = Mat.Zeros(new Size(channels[1].Cols, channels[1].Rows), MatType.CV_8UC1);
            channels[2] = alphaChannel; //fill the red channel, cuz on weight we can lose some elements from this channel
            channels.Add(alphaChannel);

            Cv2.Merge(channels.ToArray(), src);
            src = src.DrawContour(toDraw);

            channels.ForEach(x => x.Dispose());

            return src;
        }
        public static Mat Sharpness(this Mat src)
        {
            var blured = src.GaussianBlur(new Size(0, 0), 3);
            Cv2.AddWeighted(src, 1.5, blured, -0.5, 0, src);
            blured.Dispose();
            return src;
        }
        public static Mat ChangeGamma(this Mat src, double gamma)
        {
            Mat lookUpTable = new Mat(1, 256, MatType.CV_8U);
            byte[] lookUpTableData = new byte[(int)(src.Total() * src.Channels())];

            for (int i = 0; i < lookUpTable.Cols; i++)
            {
                lookUpTableData[i] = Saturate(Math.Pow(i / 255.0, gamma) * 255.0);
            }
            lookUpTable.SetArray(0, 0, lookUpTableData);
            src = src.LUT(lookUpTable);
            lookUpTable.Dispose();
            return src;
        }
        private static byte Saturate(double val)
        {
            int iVal = (int)Math.Round(val);
            iVal = iVal > 255 ? 255 : (iVal < 0 ? 0 : iVal);
            return (byte)iVal;
        }
        public static Mat ProccessToGrayContuour(this Mat image)
        {
            image = image.Canny(50, 200);
            image = image.Dilate(null);
            return image;

            var red = image.ExtractChannel(0);
            image = red.CvtColor(ColorConversionCodes.BGR2GRAY);
            red.Dispose();
            image = image.Canny(50, 150);
            //image = image.MedianBlur(5);
            image = image.Dilate(null);
            return image;
        }
        public async static Task<Point[]> FindContours_MultiChannel(this Mat image)
        {
            var tsk1 = FindContours(image.ExtractChannel(0));
            var tsk2 = FindContours(image.ExtractChannel(1));
            var tsk3 = FindContours(image.ExtractChannel(2));

            var contours = new List<Point[]>()
            {
                await tsk1,
                await tsk2,
                await tsk3
            };

            return contours.MaxBy(ContourArea).First();
        }
        public async static Task<Point[]> FindContours(this Mat image)
        {
            return await Task.Run(() =>
            {
                image = image.ProccessToGrayContuour();
                return image.FindContours_BiggestContourInt();
            });
        }
        public static Point[] FindContours_BiggestContourInt(this Mat image)
        {
            Cv2.FindContours(image, out var foundedContours, out var hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone);
            image.Release();
            image.Dispose();
            var sortedContours = foundedContours.OrderByDescending(ContourArea);
            var contourOfDocument = sortedContours.First();

            var peri = Cv2.ArcLength(contourOfDocument, true);

            var approx = Cv2.ApproxPolyDP(contourOfDocument.AsEnumerable(), 0.01 * peri, true);
            var hull = Cv2.ConvexHull(approx, true);

            peri = Cv2.ArcLength(hull, true);
            approx = Cv2.ApproxPolyDP(hull.AsEnumerable(), 0.01 * peri, true);

            if (approx.Length != 4)
            {
                var boundRect = Cv2.MinAreaRect(approx);
                return boundRect.Points().To64Point();
            }
            else
            {
                var dst = new Point[4];
                for (int i = 0; i < 4; i++)
                {
                    dst[i] = approx[approx.Length - i - 1];
                }
                return dst;
            }
        }
        public static double ContourArea(this Point[] x) => Cv2.ContourArea(x, true);

        public static Mat DrawContourAvrg(this Mat image, List<Point[]> contours)
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
            image.DrawContours(contours, -1, Scalar.Red, 5);
            return image;
        }
        private static Mat DrawContour(this Mat image, Point2f[] pointsRect) => DrawContour(image, pointsRect.To64Point());
        public static Mat DrawContour(this Mat image, IEnumerable<Point> countour)
        {
            if (countour.Count() == 0) return image;
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
