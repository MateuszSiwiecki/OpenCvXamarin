using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using System.IO;
using Xunit.Abstractions;
using OpenCvSharp;
using System.Linq;

namespace DocScanOpenCV.Utils.Tests
{
    public class ImageProcessingTests
    {
        private readonly ITestOutputHelper output;

        public ImageProcessingTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        public const string TestPath = @"C:\Users\mateu\OneDrive\Desktop\QR\documents";
        public const string TestImageRecipe = @"C:\Users\mateu\OneDrive\Desktop\QR\documents\recepta_test1.png";
        public const string TestImageRevo = @"C:\Users\mateu\OneDrive\Desktop\QR\documents\rewol_test.png";
        public const string TestImageRevo2 = @"C:\Users\mateu\OneDrive\Desktop\QR\documents\revol2_test.png";
        public const string TestImageRevo3 = @"C:\Users\mateu\OneDrive\Desktop\QR\documents\revol3_test.png";
        public static IEnumerable<object[]> ImagePaths =>
            new List<object[]>
            {
            new object[] { TestImageRecipe },
            new object[] { TestImageRevo },
            new object[] { TestImageRevo2 },
            new object[] { TestImageRevo3 },
            };

        private static string TestImageDirectory(string additionalInfo = "", [CallerMemberName] string testName = "")
        {
            
            var path = TestPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            path = Path.Combine(path, testName);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var nameOfFile = string.IsNullOrWhiteSpace(additionalInfo) ? $"{testName}.png" : $"{additionalInfo}_{testName}.png";

            return Path.Combine(path, nameOfFile);
        }
        private static void SaveAndCheckIfSavedCorrect(OpenCvSharp.Mat result, string additionalInfo = "", [CallerMemberName] string name = "")
        {
            var savePath = TestImageDirectory(additionalInfo, name);
            result.SaveImage(savePath);
            Assert.True(File.Exists(savePath));
        }


        [MemberData(nameof(ImagePaths))]
        [Theory()]
        public void OpenCvCorrectLoadTest_ShouldLoadAndSaveImage_NoChangesToImage(string imagePath)
        {
            using var testObject = ImageProcessing.LoadImage(imagePath);
            Assert.NotNull(testObject);

            SaveAndCheckIfSavedCorrect(testObject, Path.GetFileName(imagePath));
        }

        [MemberData(nameof(ImagePaths))]
        [Theory()]
        public void ProccessToGrayContuourTest(string imagePath)
        {
            using var testObject = ImageProcessing.LoadImage(imagePath);
            var result = ImageProcessing.ProccessToGrayContuour(testObject);

            SaveAndCheckIfSavedCorrect(result, Path.GetFileName(imagePath));
        }

        [MemberData(nameof(ImagePaths))]
        [Theory()]
        public void FindContours_DrawContour_BiggestContourTest(string imagePath)
        {
            using var testObject = ImageProcessing.LoadImage(imagePath);
            using var testObject2 = ImageProcessing.ProccessToGrayContuour(testObject);

            var contours = ImageProcessing.FindContours_BiggestContourInt(testObject2);

            using var result = ImageProcessing.DrawContour(testObject.Clone(), contours);
            SaveAndCheckIfSavedCorrect(result, Path.GetFileName(imagePath));
        }

        [InlineData(10, 1000, 10, 1000, TestImageRecipe)]
        [Theory()]
        public void DrawContourTest_RandomContour(int xLeft, int xRight, int yUp, int yDown, string imagePath)
        {
            using var testObject = ImageProcessing.LoadImage(imagePath);
            using var result = ImageProcessing.DrawContour(testObject.Clone(), new List<Point>
            {
                new Point(xLeft, yUp) ,
                new Point(xRight, yUp) ,
                new Point(xRight, yDown),
                new Point(xLeft, yDown) ,
            });

            SaveAndCheckIfSavedCorrect(result, $"{xLeft}x{xRight}x{yUp}x{yDown}_{Path.GetFileName(imagePath)}");
        }

        [InlineData(10, 1000, 10, 1000, TestImageRecipe)]
        [InlineData(2000, 1000, 10, 1000, TestImageRecipe)]
        [InlineData(2000, 4000, 2000, 4000, TestImageRecipe)]
        [InlineData(8000, 1000, 8000, 1000, TestImageRecipe)]
        [Theory()]
        public void TransformTest_Rectangle(int xLeft, int xRight, int yUp, int yDown, string imagePath)
        {
            using var testObject = ImageProcessing.LoadImage(imagePath);

            var pointsOfFragment = new Point2f[]
            {
                new Point2f(xLeft, yUp) ,
                new Point2f(xRight, yUp) ,
                new Point2f(xRight, yDown),
                new Point2f(xLeft, yDown) ,
            };
            var pointsOfDestination = new Point2f[]
            {
                new Point2f(0, 0),
                new Point2f(testObject.Width, 0),
                new Point2f(testObject.Width, testObject.Height),
                new Point2f(0, testObject.Height)
            };

            using var contouredImage = ImageProcessing.DrawContour(testObject.Clone(), new List<Point>
            {
                new Point(xLeft, yUp) ,
                new Point(xRight, yUp) ,
                new Point(xRight, yDown),
                new Point(xLeft, yDown)
            });
            using var result = ImageProcessing.Transform(testObject, pointsOfFragment.ToList());


            SaveAndCheckIfSavedCorrect(contouredImage, $"{xLeft}x{xRight}x{yUp}x{yDown}_{Path.GetFileName(imagePath)}_orgin");
            SaveAndCheckIfSavedCorrect(result, $"{xLeft}x{xRight}x{yUp}x{yDown}_{Path.GetFileName(imagePath)}_result");
        }

        [InlineData(10, 10, 1000, 1000, 1200, 2000, 500, 1800, TestImageRecipe)]
        [Theory()]
        public void TransformTest_UnnormalShape(int point1x, int point1y, int point2x, int point2y, int point3x, int point3y, int point4x, int point4y, string imagePath)
        {
            using var testObject = ImageProcessing.LoadImage(imagePath);

            var pointsOfFragment = new Point2f[]
            {
                new Point2f(point1x, point1y) ,
                new Point2f(point2x, point2y) ,
                new Point2f(point3x, point3y),
                new Point2f(point4x, point4y)
            };
            var pointsOfDestination = new Point2f[]
            {
                new Point2f(0, 0),
                new Point2f(testObject.Width, 0),
                new Point2f(testObject.Width, testObject.Height),
                new Point2f(0, testObject.Height)
            };

            using var contouredImage = ImageProcessing.DrawContour(testObject.Clone(), new List<Point>
            {
                new Point(point1x, point1y) ,
                new Point(point2x, point2y) ,
                new Point(point3x, point3y),
                new Point(point4x, point4y)
            });
            using var result = ImageProcessing.Transform(testObject, pointsOfFragment.ToList());


            SaveAndCheckIfSavedCorrect(contouredImage, $"{point1x}x{point2x}x{point3x}x{point4x}_{Path.GetFileName(imagePath)}_orgin");
            SaveAndCheckIfSavedCorrect(result, $"{point1x}x{point2x}x{point3x}x{point4x}_{Path.GetFileName(imagePath)}_result");
        }

    }
}