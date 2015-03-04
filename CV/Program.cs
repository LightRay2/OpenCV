using System;
using OpenCvSharp;

namespace CV
{
    class Program
    {
        static string[] files = { @"C:\Users\Public\Pictures\Sample Pictures\Jellyfish.jpg",
                                @"C:\Users\Public\Pictures\Sample Pictures\Koala.jpg",
                                @"C:\Users\Public\Pictures\Sample Pictures\Desert.jpg",
                                @"C:\Users\Public\Pictures\Sample Pictures\Hydrangeas.jpg",
                                @"C:\Users\Public\Pictures\Sample Pictures\Koala2.jpg",
                                @"images\whitehouse.jpg",
                                @"images\circles.jpg",
                                @"images\one_circle.jpg",
                                @"images\arrow_paper.jpg",
                                @"images\arrow_template.jpg"};
        static void Main(string[] args)
        {
          //  Chapter2 chapter2 = new Chapter2();
        //    using (var img = new IplImage(files[8], LoadMode.Color))
        //    {
               // chapter2.ChangeQuantisationGrey(img, 4);

                //chapter2.Smaller(img,4.1);
               // chapter2.HLSSplittedIntoChannels(img);
               // chapter2.SkinDetection(img);

                /*
                Cv.SetImageROI(img, new CvRect(400,400, 380, 400));
                Cv.Not(img, img);
                Cv.ResetImageROI(img);
                using (new CvWindow(img))
                {
                    Cv.WaitKey();
                }*/

                /*
                Noise noise = new Noise();
                noise.SaltAndPepper(img, 2);
                noise.EvaluateNoise(img, Noise.EvaluateType.Gaussian5);
                chapter2.Show4Pictures(
                    img,
                    noise.EvaluateNoise(img, Noise.EvaluateType.Blur3),
                    noise.EvaluateNoise(img, Noise.EvaluateType.Gaussian5),
                    noise.EvaluateNoise(img, Noise.EvaluateType.Median));*/

            //    Histogram histogram = new Histogram();
                /*using (var img2 = new IplImage(files[4], LoadMode.Color))
                {
                    double q2 = histogram.CompareImagesByHistograms(img, img2, Histogram.CompareType.Chi_square);
                    double q1 = histogram.CompareImagesByHistograms(img, img2, Histogram.CompareType.Correlation);
                    double q3 = histogram.CompareImagesByHistograms(img, img2, Histogram.CompareType.Bhattacharyya);
                    double q4 = histogram.CompareImagesByHistograms(img, img2, Histogram.CompareType.Intersection);
                }*/
                //histogram.KMeansExample(img,12);
                //histogram.GreyScaleHist(img);
               // histogram.DrawHLSHist(img);
               // noise.SaltAndPepper(img, 0.5);
             //  histogram.EqualizeLuminance(img);


                //--------------------
               // BinaryVision binaryVision = new BinaryVision(img);
                //----
               // Transformations tr = new Transformations(img);
               // Edges edges = new Edges(img);
             //   using (var img2 = new IplImage(files[9], LoadMode.Color))
            //    {
                   // var features = new Features(img2, img);
                  //  var recognition = new Recognition(img2, img);
           //     }
               // new VideoWriter();
         //   }
           // new CppDetection();
            /*
            using (var img = new IplImage(@"face.jpg", LoadMode.Color))
            {
                chapter2.SkinDetection(img);
            }*/

            new VideoWriter();
        }
    }
}
