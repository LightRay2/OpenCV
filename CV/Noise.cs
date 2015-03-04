using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace CV
{
    class Noise
    {
        CvRNG RNG = new CvRNG(DateTime.Now);
        Random rand = new Random();
        public void GaussianNoise(IplImage image, double average, double standardDeviation)
        {
            CvMat noiseImage = Cv.CreateMat(image.Height, image.Width, MatrixType.S16C3);
            Cv.RandArr(RNG, noiseImage, DistributionType.Normal, new CvScalar(average,average,average), new CvScalar(standardDeviation,standardDeviation,standardDeviation));
            CvMat temp = new CvMat(image.Height, image.Width, MatrixType.S16C3);
            image.Convert(temp);
            Cv.AddWeighted(temp, 1.0, noiseImage, 1.0, 0.0, temp);
            temp.Convert(image);
        }

        public void SaltAndPepper(IplImage image, double percentage)
        {
            int noisePoints = (int)(((double)image.Height * image.Width * image.NChannels) * percentage / 100.0);
            for (int count = 0; count < noisePoints; count++)
            {
                int row = rand.Next(image.Height),
                    column = rand.Next(image.Width),
                    channel = rand.Next(image.NChannels);
                unsafe{
                    byte* pixel = (byte*)image.ImageData;
                    int offset = row * image.WidthStep + (column * 3);
                    pixel[offset + channel] = rand.Next(2) == 1 ? (byte)0 : (byte)255;
                }
            }
        }

        //http://www.nudoq.org/#!/Packages/OpenCvSharp-x64/OpenCvSharp/SmoothType
        public enum EvaluateType{Blur3, Gaussian5, Median }
        public static IplImage EvaluateNoise(IplImage image, EvaluateType type)
        {
            IplImage result = Cv.CreateImage(image.Size, image.Depth, image.NChannels);
            switch (type)
            {
                case EvaluateType.Blur3:
                    Cv.Smooth(image, result, SmoothType.Blur,3,3);
                    break;
                case EvaluateType.Gaussian5:
                    Cv.Smooth(image, result, SmoothType.Gaussian, 5, 5, 1.5);
                    break;
                case EvaluateType.Median:
                    Cv.Smooth(image, result, SmoothType.Median, 5,5);
                    break;
            }
            return result;
        }
    }
}
