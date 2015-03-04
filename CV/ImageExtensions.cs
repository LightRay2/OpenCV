using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV
{
    static class ImageExtensions
    {
        public static IplImage Grey(this IplImage image){
            if (image.NChannels == 1)
                return image;
            IplImage res = new IplImage(image.Size, BitDepth.U8, 1);
            Cv.CvtColor(image, res, ColorConversion.BgrToGray);
            return res;
        }

        public static IplImage Same(this IplImage image)
        {
            return new IplImage(image.Size, image.Depth, image.NChannels);
        }

        public static IplImage ToHls(this IplImage image)
        {
            IplImage hls = image.Same();
            Cv.CvtColor(image, hls, ColorConversion.BgrToHls);
            return hls;
        }

        public static IplImage ChangeSize(this IplImage image, double k)
        {
            IplImage res = new IplImage((int)(image.Width * k), (int)(image.Height * k), image.Depth, image.NChannels);
            image.Resize(res);
            return res;
        }
    }
}
