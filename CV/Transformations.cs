using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV
{
    class Transformations
    {
        public Transformations(IplImage image)
        {
            IplImage res = Screw(image);
            IplImage res2= Rotate(image);
            IplImage res3 = GetAndApplyAffineMatrix(image);
            using (new CvWindow(res3))
            {
                Cv.WaitKey();
            }
        }

        public static IplImage Screw(IplImage image){
            IplImage res = image.Same();
            float[,] x= {{1,0.2f,-30.0f}, {0,1,0}};
            CvMat mat  = CvMat.FromArray<float>(x, MatrixType.F32C1);
            Cv.WarpAffine(image, res, mat);
            return res;
        }

        public static IplImage Rotate(IplImage image, double angle = 30)
        {
            IplImage res = image.Same();
            CvMat mat = CvMat.RotationMatrix(new CvPoint2D32f(0, 0), angle, 1.0);
            Cv.WarpAffine(image, res, mat);
            return res;
        }

        public static IplImage GetAndApplyAffineMatrix(IplImage image, CvPoint2D32f[] src=null, CvPoint2D32f[] dst=null)
        {
            //4 points for perspective transformation
            //CvMat.PerspectiveTransform()
             //   Cv.WarpPerspective()
            IplImage res = image.Same();
            CvMat mat;
            if (src == null) //demo version
            {
                mat = CvMat.AffineTransform(new CvPoint2D32f[]{
                    new CvPoint2D32f( 0,0), new CvPoint2D32f(1,0), new CvPoint2D32f(0,1)},
                    new CvPoint2D32f[]{ 
                    new CvPoint2D32f( 0,0), new CvPoint2D32f(1,0), new CvPoint2D32f(0,2.5)}
                    );
            }
            else
            {
                mat = CvMat.AffineTransform(src, dst);
            }
            Cv.WarpAffine(image, res, mat, Interpolation.Cubic);
            return res;
        }
    }
}
