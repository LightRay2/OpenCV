using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV
{
    class Helpers
    {
        public enum Filters { Skin, BlackWhite };
        public static IplImage Filter(IplImage image, Filters filters)
        {
            switch (filters)
            {
                case Filters.BlackWhite: 
                    return Filter(image, FilterBlackAndWhite);
                case Filters.Skin: 
                    return Filter(image, FilterSkin); 
            }
            return null;
        }

        public static IplImage Filter(IplImage image, Func<CvScalar, bool> FilterPixel)
        {
            IplImage res = image.Clone();
            for (int row = 0; row < image.Height; row++)
                for (int col = 0; col < image.Width; col++)
                {
                    CvScalar pixel = image.Get2D(row, col);
                    if (FilterPixel(pixel))
                        res[row, col] = new CvScalar(0, 0, 0);
                }
            return res;
        }

        #region some filters
        static bool FilterSkin(CvScalar hlsPixel)
        {
            return (hlsPixel.Val2 >= 0) && ((hlsPixel.Val0 <= 14) || (hlsPixel.Val0 >= 165));
        }

        static bool FilterBlackAndWhite(CvScalar pixel)
        {
            double max = Math.Max(pixel.Val0, Math.Max(pixel.Val2, pixel.Val1));
            double min = Math.Min(pixel.Val0, Math.Min(pixel.Val2, pixel.Val1));
            return max - min > 20;
        }
        #endregion
    }
}
