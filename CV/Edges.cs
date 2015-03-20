using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Utilities;

namespace CV
{
    class Edges
    {
        public Edges(IplImage image)
        {
          //  Cv.non
            //Sharpening(image);
            IplImage res = LaplacianEdges(image);
            IplImage cannyRes = CannyEdges(image,100,200);
            IplImage lineSegments = LineSegments(cannyRes);
           // using (new CvWindow(res))
           /* using(new CvWindow(lineSegments))
            using (new CvWindow(cannyRes))
            {
                Cv.WaitKey();
            }*/
            //HoughLines(image);
            HoughCircles(image);
        }

        public static IplImage LaplacianEdges(IplImage image)
        {
            image = image.Gray();

            IplImage laplacian = image.Same();
            IplImage blurred_image = image.Same();
            Cv.Smooth(image, blurred_image, SmoothType.Gaussian, 5, 5, 1.5);
            Cv.Laplace(blurred_image, laplacian);
            return laplacian;
        }

        public static IplImage CannyEdges(IplImage image, double threshhold1=7, double threshhold2=250)
        {
            image = image.Gray();

            IplImage res = image.Same();
            Cv.Canny(image, res, threshhold1, threshhold2);
            return res;
        }

        public static void Sharpening(IplImage image){
            IplImage image32 = new IplImage(image.Size, BitDepth.F32, 3);
            image.Convert(image32);
            IplImage laplacian = image.Same();

            Cv.Laplace(image,laplacian);
            CvMat sharp = Cv.GetMat(image32) - 0.3*Cv.GetMat(laplacian);
            IplImage res = new IplImage(image.Size, BitDepth.U8, 3);
            sharp.Convert(res);
           using(new CvWindow(image))
           using (new CvWindow(res))
           {
               Cv.WaitKey();
           }
        }

        public static List<Line> GetBigLines(IplImage contoursImage,  double minLength, double precision = 3)
        {
            var lines = new List<Line>();
            

            CvMemStorage storage = new CvMemStorage();
            CvContourScanner scanner = new CvContourScanner(contoursImage.Gray(), storage);
            while(true){
                var contour = scanner.FindNextContour();
                if (contour == null) break;
                contour = Cv.ApproxPoly(contour, CvContour.SizeOf, storage, ApproxPolyMethod.DP, precision, true);
                for(int i =0; i < contour.Total; i++)
                {

                    CvPoint p1 = contour.GetSeqElem(i).Value;
                    CvPoint p2 = contour.GetSeqElem((i+1)%contour.Total).Value;
                    if (p1.DistanceTo(p2) >= minLength)
                    {
                        lines.Add(new Line(p1, p2));
                    }
                    
                }
            }
            return lines;
        }

        public static IplImage LineSegments(IplImage contoursImage, double precision=3)
        {
            IplImage res = new IplImage(contoursImage.Size, BitDepth.U8, 3);
            res.Zero();
            CvSeq<CvPoint> contours;
            CvMemStorage storage = new CvMemStorage();
            contoursImage.Clone().FindContours(storage, out contours, CvContour.SizeOf, ContourRetrieval.Tree, ContourChain.ApproxNone);
           // storage = new CvMemStorage();
            var approxContours = Cv.ApproxPoly(contours, CvContour.SizeOf, storage, ApproxPolyMethod.DP, precision, true);
            Cv.DrawContours(res, approxContours, CvColor.Red, CvColor.Green, 1000, 1, LineType.AntiAlias);
            return res;
        }

        public static IplImage HoughLines(IplImage image, bool useCanny = true, int minLength = 20, int maxGap = 10)
        {
            // cvHoughLines2
            // 標準的ハフ変換と確率的ハフ変換を指定して線（線分）の検出を行なう．サンプルコード内の各パラメータ値は処理例の画像に対してチューニングされている．

            // (1)画像の読み込み 
            IplImage srcImgProb = image.Clone();
            using (IplImage srcImgGray = image.Gray())
           // using (IplImage srcImgStd = image.Clone())
            {
                // (2)ハフ変換のための前処理 
                if(useCanny)
                    Cv.Canny(srcImgGray, srcImgGray, 250, 0, ApertureSize.Size3);
                using (CvMemStorage storage = new CvMemStorage())
                {
                    // (3)標準的ハフ変換による線の検出と検出した線の描画
                 //  CvSeq lines = srcImgGray.HoughLines2(storage, HoughLinesMethod.Standard, 1, Math.PI / 180, 50, 0, 0);
                    // wrapper style
                    //CvLineSegmentPolar[] lines = src_img_gray.HoughLinesStandard(1, Math.PI / 180, 50, 0, 0);

                    //int limit = lines.Total;
                    //for (int i = 0; i < limit; i++)
                    //{
                    //    // native code style
                    //    /*
                    //    unsafe
                    //    {
                    //        float* line = (float*)lines.GetElem<IntPtr>(i).Value.ToPointer();
                    //        float rho = line[0];
                    //        float theta = line[1];
                    //    }
                    //    //*/

                    //    // wrapper style
                    //    CvLineSegmentPolar elem = lines.GetSeqElem<CvLineSegmentPolar>(i).Value;
                    //    float rho = elem.Rho;
                    //    float theta = elem.Theta;

                    //    double a = Math.Cos(theta);
                    //    double b = Math.Sin(theta);
                    //    double x0 = a * rho;
                    //    double y0 = b * rho;
                    //    CvPoint pt1 = new CvPoint { X = Cv.Round(x0 + 1000 * (-b)), Y = Cv.Round(y0 + 1000 * (a)) };
                    //    CvPoint pt2 = new CvPoint { X = Cv.Round(x0 - 1000 * (-b)), Y = Cv.Round(y0 - 1000 * (a)) };
                    //    srcImgStd.Line(pt1, pt2, CvColor.Red, 1, LineType.AntiAlias, 0);
                    //}
                
                    // (4)確率的ハフ変換による線分の検出と検出した線分の描画
                    CvSeq lines = srcImgGray.HoughLines2(storage, HoughLinesMethod.Probabilistic, 1, Math.PI / 180, 1, minLength, maxGap);
                    // wrapper style
                    //CvLineSegmentPoint[] lines = src_img_gray.HoughLinesProbabilistic(1, Math.PI / 180, 50, 0, 0);

                    for (int i = 0; i < lines.Total; i++)
                    {
                        // native code style
                        /*
                        unsafe
                        {
                            CvPoint* point = (CvPoint*)lines.GetElem<IntPtr>(i).Value.ToPointer();
                            src_img_prob.Line(point[0], point[1], CvColor.Red, 3, LineType.AntiAlias, 0);
                        }
                        //*/

                        // wrapper style
                        CvLineSegmentPoint elem = lines.GetSeqElem<CvLineSegmentPoint>(i).Value;
                        srcImgProb.Line(elem.P1, elem.P2, CvColor.Yellow, 2, LineType.AntiAlias, 0);
                    }
                }

                // (5)検出結果表示用のウィンドウを確保し表示する
                //using (new CvWindow("Hough_line_standard", WindowMode.AutoSize, srcImgStd)) 
            }
            return srcImgProb;
        }

        public static void HoughCircles(IplImage image)
        {
            using (IplImage res = CannyEdges(image.Gray()))
            using (CvMemStorage storage = new CvMemStorage())
            {
                CvSeq circles = image.Gray().HoughCircles(storage, HoughCirclesMethod.Gradient, 1, 20, 150, 100 ,5,0);//upper treshhold for canny, thresh for centers, min radius, max radius 
                for (int i = 0; i < circles.Total; i++)
                {
                    CvCircleSegment c = circles.GetSeqElem<CvCircleSegment>(i).Value;
                    res.DrawCircle(c.Center, (int)c.Radius, CvColor.Blue,3);
                }
                using (new CvWindow(res))
                {
                    Cv.WaitKey();
                }
            }
        }
    }
}
