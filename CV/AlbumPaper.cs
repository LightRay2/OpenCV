using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV
{
    class AlbumPaper
    {

        public static bool Check(IplImage image, out IplImage transformedImage)
        {
            var smoothed = Noise.EvaluateNoise(image, Noise.EvaluateType.Gaussian5);

            //AddInfoInPoint(Smoothed, ColorText);

            var blackWhite = Helpers.Filter(smoothed, Helpers.Filters.BlackWhite);

            var imgLightLevels = new IplImage(image.Width, image.Height, BitDepth.U8, 1);
            Cv.Split(blackWhite, null, imgLightLevels, null, null);

            var imgLightEdges = Edges.CannyEdges(blackWhite, 255, 0);

            //!!!!
            /*
             using (new CvWindow(image))
            {

            CannyTester(imgLightLevels);

            }*/
            transformedImage = imgLightEdges;
            return true;
           

        }

        public static void AddInfoInPoint(IplImage image, Func<IplImage,int,int,string> GetText)
        {
            using (CvFont font = new CvFont(FontFace.HersheyComplex, 0.7, 0.7))
            using(var window = new CvWindow("info"))
            {
                Cv.SetMouseCallback("info", new CvMouseCallback((mouseEvent, x, y, mouseEvent2) =>
                {
                    using (var clone = image.Clone())
                    {
                        clone.PutText("[ " + x.ToString() + ", " + y.ToString() + " ] " + GetText(image, x, y), new CvPoint(10, 40), font, new CvScalar(0, 0, 255));
                        window.Image = clone;
                    }
                }));
                Cv.WaitKey();
            }
        }

        public static string ColorText(IplImage image, int x, int y)
        {
            using(IplImage hls = image.ToHls())
                return "HLS: "+hls[y, x].ToString() + " BGR: "+image[y,x].ToString();
        }


        static int lowThresh = 100, highThresh = 200;
        static void CannyTester(IplImage image)
        {
            using (var window = new CvWindow())
            {
                CvTrackbarCallback onLow = delegate(int pos)
                {
                    lowThresh = pos;
                    window.ShowImage(Edges.CannyEdges(image, lowThresh, highThresh));
                };
                CvTrackbarCallback onHigh = delegate(int pos)
                {
                    highThresh = pos;
                    window.ShowImage(Edges.CannyEdges(image, lowThresh, highThresh));
                };
                window.CreateTrackbar("low", lowThresh, 255, onLow);
                window.CreateTrackbar("high", highThresh, 255, onHigh);
                onLow(lowThresh);
                Cv.WaitKey();
            }
        }
        //1. black-white filter
        //2. edges
        //3. contours?
    }
}
