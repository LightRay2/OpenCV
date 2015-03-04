using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
//using OpenCvSharp.CPlusPlus;

namespace CV
{
    class Chapter2
    {
         
        public void ChangeQuantisationGrey(IplImage image, int bits)
        {
            CvMat mat = Cv.GetMat(image);
            IplImage binary = new IplImage(image.Size, BitDepth.U8, 1);

            Cv.CvtColor(image, binary, ColorConversion.BgrToGray);

            int max = -1000000, min = 100000;
            byte mask = (byte)( 0xFF << (8 - bits));
            for (int row = 0; row < image.Height; row++)
                for (int col = 0; col < image.Width; col++)
                {
                    binary[row, col] = new CvScalar(((int)(binary[row, col].Val0) & mask));
                    if ((int)binary[row, col].Val0 > max) max = (int)binary[row, col].Val0;
                    if ((int)binary[row, col].Val0 <min) min = (int)binary[row, col].Val0;
                }
            
            using (new CvWindow(binary))
                {
                    Cv.WaitKey();
                };
        }

        public void Smaller(IplImage image, double k)
        {
            IplImage smallPicture = new IplImage( (int)(image.Width / k), (int)(image.Height / k), image.Depth, image.NChannels);
            Cv.Resize(image, smallPicture);
            using ( new CvWindow(smallPicture))
            {
                Cv.WaitKey();
            }
        }



        public void HLSSplittedIntoChannels(IplImage image)
        {
            CvMat rgb = Cv.GetMat(image);
            CvMat hls = new CvMat(rgb.Height, rgb.Width, MatrixType.U8C3);
            Cv.CvtColor(rgb,hls, ColorConversion.BgrToHls);
            CvMat h = new CvMat(rgb.Height, rgb.Width, MatrixType.U8C1), l = new CvMat(rgb.Height, rgb.Width, MatrixType.U8C1), s = new CvMat(rgb.Height, rgb.Width, MatrixType.U8C1);
            Cv.Split(hls, h, l, s, null);

            Show4Pictures(h, h, l, s);
            /*
            using (new CvWindow(s))
            {
                Cv.WaitKey();
            }*/
        }


        public void Show4Pictures(IplImage a, IplImage b, IplImage c, IplImage d)
        {
            Show4Pictures(Cv.GetMat(a), Cv.GetMat(b), Cv.GetMat(c), Cv.GetMat(d), a.Depth, a.NChannels);
        }
        public void Show4Pictures(CvMat a, CvMat b, CvMat c, CvMat d, BitDepth depth = BitDepth.U8, int channels = 1)
        {
            IplImage image = new IplImage(a.Width, a.Height, depth, channels);
            int sizeX = image.Width/2,
                sizeY = image.Height/2;

            Cv.SetImageROI(image,new CvRect(0,0,sizeX, sizeY));
            a.Resize(image);
            Cv.ResetImageROI(image);
            Cv.SetImageROI(image, new CvRect(sizeX, 0, sizeX, sizeY));
            b.Resize(image);
            Cv.ResetImageROI(image);
            Cv.SetImageROI(image, new CvRect(0, sizeY, sizeX, sizeY));
            c.Resize(image);
            Cv.ResetImageROI(image);
            Cv.SetImageROI(image, new CvRect(sizeX, sizeY, sizeX, sizeY));
            d.Resize(image);
            Cv.ResetImageROI(image);

            using (new CvWindow(image))
            {
                Cv.WaitKey();
            }
        }

        public static IplImage SkinDetection(IplImage image)
        {
            IplImage res = image.Clone();
            CvMat hls = new CvMat(image.Height, image.Width, MatrixType.U8C3);
            Cv.CvtColor(image, hls, ColorConversion.BgrToHls);
            for (int row = 0; row < image.Height; row++)
                for (int col = 0; col < image.Width; col++)
                {
                    if (!IsSkinPixel(hls.Get2D(row, col)))
                        res[row, col] = new CvScalar(0, 0, 0);
                }
            return res;
        }

        static bool IsSkinPixel(CvScalar hlsPixel)
        {
            double LS_ratio = ( hlsPixel.Val1) / ( hlsPixel.Val2);
            return  (hlsPixel.Val2 >= 20) && (LS_ratio > 0.5) &&
            (LS_ratio < 3.0) && ((hlsPixel.Val0 <= 14)  || (hlsPixel.Val0 >= 165));
        }
    }
}
