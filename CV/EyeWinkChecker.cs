using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV
{
    class EyeWinkChecker
    {
        public static bool Check(IplImage image)
        {
            
            //image = BinaryVision.Closing(image, 15, 15);
            //IplImage[] channels = image.ToHls().Split();

            /*channels[0].Normalize(channels[0], 125, 255, NormType.L1);

            channels[1].Normalize(channels[1], 125, 255, NormType.L1);
            image.Merge(channels[0],channels[1],channels[2],null);
            image.CvtColor(image, ColorConversion.HlsToBgr);*/

            /*IplImage hueEdges = Edges.CannyEdges(channels[0]);
            IplImage saturationEdges = Edges.CannyEdges(channels[2]);
            IplImage lightEdges = Edges.CannyEdges(channels[1]);*/
            //CannyTester(channels[0]);
            //CannyTester(image);
            //SkinTester(image);
            // RGBFilter(image, true);
           /* IplImage step1 = Histogram.NormalizeLuminance(image),
             step2 = Edges.CannyEdges(image),
             step3 = BinaryVision.AdaptiveThreshhold(image, 61, 10);*/



            // using(new CvWindow("hueEdges", hueEdges))
            // using (new CvWindow("saturationEdges", saturationEdges))
            // using (new CvWindow("lightEdges", lightEdges))

            /* using(new CvWindow("hueEdges", Edges.LaplacianEdges(channels[0])))
             using (new CvWindow("saturationEdges", Edges.LaplacianEdges(channels[1])))
             using (new CvWindow("lightEdges", Edges.LaplacianEdges(image)))

            using(var original =  new CvWindow("original", image))
            using(new CvWindow("skin", SkinDetection(image,14,165,0))){

                Cv.WaitKey();
            }*/


            image = Transformations.Rotate(image, -6);
            ReturnAns(image);
            var hls = image.ToHls();

            //using (new CvWindow(blackWhiteImage))
            using (CvFont font = new CvFont(FontFace.HersheyComplex, 0.7, 0.7))
            using (var window = new CvWindow("original"))
            {
                Cv.SetMouseCallback("original", new CvMouseCallback((mouseEvent, x, y, mouseEvent2) =>
                {
                    var clone = image.Clone();
                    try
                    {
                        clone.PutText(hls[y, x].ToString(), new CvPoint(10, 40), font, new CvScalar(0, 0, 255));
                        clone.PutText(image[y, x].ToString() + x.ToString() + " " + y.ToString(), new CvPoint(10, 60), font, new CvScalar(0, 0, 255));
                    }
                    catch { }
                    
                    window.ShowImage(clone);
                }));
                Cv.WaitKey();
            }
            return true;
        }

        static bool ReturnAns(IplImage image)
        {
            image = Noise.EvaluateNoise(image, Noise.EvaluateType.Blur3);
            image = Histogram.NormalizeLuminance(image);
            for (int lineLenght = 23; lineLenght <= 30; lineLenght += 2)
            {
                List<CvPoint> eyes = GetEyeCentersByDrop(image, lineLenght, 30);
                eyes = OnlyEyesOnTheFaceRemain(image, eyes, lineLenght, 0.3);

               /* using (var window = new CvWindow())
                {
                    foreach (CvPoint eye in eyes)
                    {
                        Cv.DrawLine(image, eye.X - lineLenght / 2, eye.Y, eye.X + lineLenght / 2, eye.Y, new CvScalar(0, 255, 134), 3);
                    }
                    window.Image = image;
                    Cv.WaitKey();
                }*/

                List<CvPoint> openedEyes;
                if (IsThereTwoOpenedEyes(eyes, out openedEyes, lineLenght))
                {
                    return true;
                }

            }
            return false;
        }

        

       

#region old 
        static void CannyTester(IplImage image)
        {
            int lowThresh = 100, highThresh = 200;
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
                window.CreateTrackbar("minS", highThresh, 255, onHigh);
                onLow(lowThresh);
                Cv.WaitKey();
            }
        }

        static void SkinTester(IplImage image)
        {
            int minH = 14, maxH = 165, minS = 0;
            using (var window = new CvWindow())
            {
                CvTrackbarCallback onMinH = delegate(int pos)
                {
                    minH = pos;
                    window.ShowImage(SkinDetection(image, minH, maxH, minS));
                };
                CvTrackbarCallback onMaxH = delegate(int pos)
                {
                    maxH = pos;
                    window.ShowImage(SkinDetection(image, minH, maxH, minS));
                };
                CvTrackbarCallback onMinS = delegate(int pos)
                {
                    minS = pos;
                    window.ShowImage(SkinDetection(image, minH, maxH, minS));
                };
                window.CreateTrackbar("minH", minH, 255, onMinH);
                window.CreateTrackbar("maxH", maxH, 255, onMaxH);
                window.CreateTrackbar("minS", minS, 180, onMinS);
                onMinH(14);
                Cv.WaitKey();
            }
        }


        static void RGBFilter(IplImage image, bool above)
        {
            int r = 0, g = 0, b = 0;
            using (var window = new CvWindow())
            {
                CvTrackbarCallback onMinH = delegate(int pos)
                {
                    r = pos;
                    window.ShowImage(RGBDetection(image, r, g, b, above));
                };
                CvTrackbarCallback onMaxH = delegate(int pos)
                {
                    g = pos;
                    window.ShowImage(RGBDetection(image, r, g, b, above));
                };
                CvTrackbarCallback onMinS = delegate(int pos)
                {
                    b = pos;
                    window.ShowImage(RGBDetection(image, r, g, b, above));
                };
                window.CreateTrackbar("minH", r, 255, onMinH);
                window.CreateTrackbar("maxH", g, 255, onMaxH);
                window.CreateTrackbar("minS", b, 255, onMinS);
                onMinH(14);
                Cv.WaitKey();
            }
        }

        public static IplImage RGBDetection(IplImage image, int r, int g, int b, bool above)
        {
            IplImage res = image.Clone();
            //CvMat hls = new CvMat(image.Height, image.Width, MatrixType.U8C3);
            //Cv.CvtColor(image, hls, ColorConversion.BgrToHls);
            for (int row = 0; row < image.Height; row++)
                for (int col = 0; col < image.Width; col++)
                {
                    CvScalar pixel = image[row, col];
                    if (above)
                    {
                        if (!(pixel.Val0 >= b && pixel.Val1 >= g && pixel.Val2 >= r))
                            res[row, col] = new CvScalar(0, 0, 0);
                    }
                    else
                    {
                        if (pixel.Val0 > b && pixel.Val1 > g && pixel.Val2 > r)
                            res[row, col] = new CvScalar(0, 0, 0);
                    }
                }
            return res;
        }

        public static IplImage SkinDetection(IplImage image, int minH, int maxH, int minS)
        {
            IplImage res = image.Clone();
            CvMat hls = new CvMat(image.Height, image.Width, MatrixType.U8C3);
            Cv.CvtColor(image, hls, ColorConversion.BgrToHls);
            for (int row = 0; row < image.Height; row++)
                for (int col = 0; col < image.Width; col++)
                {
                    if (!IsSkinPixel(hls.Get2D(row, col), minH, maxH, minS))
                        res[row, col] = new CvScalar(0, 0, 0);
                    else
                        res[row, col] = new CvScalar(255, 255, 255);
                }
            return res;
        }

        static bool IsSkinPixel(CvScalar hlsPixel, int minH, int maxH, int minS)
        {
            return (hlsPixel.Val2 >= minS) && ((hlsPixel.Val0 <= minH) || (hlsPixel.Val0 >= maxH));
        }

#endregion

        //313 172
        static List<CvPoint> GetEyeCentersByDrop(IplImage originalImage, int lineLength, int colorMinimalDrop)
        {

            var image = Helpers.Filter(originalImage, Helpers.Filters.BlackWhite);
            var res = new List<CvPoint>();
            unsafe
            {
                for (int row = 0; row < image.Height; row++)
                    for (int column = 0; column < image.Width - lineLength-1; column++)
                    {
                        byte* pixel = (byte*)image.ImageData;
                        int offset = row * image.WidthStep + (column * 3);
                        //сначала посмотрим, что неотфильтрованных точек в линии достаточно
                        int pointCount = 0;
                        for (int i = 0; i < lineLength; i++)
                            if (pixel[offset + i * 3] != 0) pointCount++;
                        if ((double)pointCount / lineLength > 0.75) //75%
                        {
                            bool eyeFound = false;
                            #region Ищем перепады  от белого к черному слева и справа
                            //ищем перепад от белого к черному слева
                            int curLeft = -1;
                            int leftDropIndex = -1;
                            for (int i = 0; i < lineLength; i++)
                            {
                                if (pixel[offset + i * 3] == 0) continue;
                                int sum = pixel[offset + i * 3] + pixel[offset + i * 3 + 1] + pixel[offset + i * 3 + 2];
                                if (curLeft == -1) curLeft = sum;
                                else if (curLeft - sum >= colorMinimalDrop)
                                {
                                    leftDropIndex = i; break;
                                }
                            }
                            if (leftDropIndex == -1) continue; //экономим время
                            //продублировали для справа
                            int curRight = -1;
                            int rightDropIndex = -1;
                            for (int i = lineLength - 1; i >= 0; i--)
                            {
                                if (pixel[offset + i * 3] == 0) continue;
                                int sum = pixel[offset + i * 3] + pixel[offset + i * 3 + 1] + pixel[offset + i * 3 + 2];
                                if (curRight == -1) curRight=sum;
                                else if (curRight - sum >= colorMinimalDrop)
                                {
                                    rightDropIndex = i; break;
                                }
                            }
                            //если оба найдены, убедимся, что в середине зрачок (т е нет обратного перепада к белому цвету)
                            //также проверим, что расстояние между перепадами больше или равно четверти длины проверяемой линии
                            if (leftDropIndex != -1 && rightDropIndex != -1 && leftDropIndex + lineLength / 3 <= rightDropIndex)
                            {
                                eyeFound = true;
                                for (int i = leftDropIndex; i <= rightDropIndex; i++)
                                {
                                    if (pixel[offset + i * 3] == 0) continue;
                                    int sum = pixel[offset + i * 3] + pixel[offset + i * 3 + 1] + pixel[offset + i * 3 + 2];
                                    if (sum >= colorMinimalDrop + Math.Max(curRight, curLeft))  //из двух сумм цветов выбираем ту, которая белее (т е смягченное условие)
                                    {
                                        eyeFound = false; break;
                                    }
                                }
                            }
                            #endregion

                            if (eyeFound)
                            {
                                bool newEye = true;
                                CvPoint p = new CvPoint(column + lineLength / 2, row);
                                foreach (var point in res) if (point.DistanceTo(p) < lineLength) { newEye = false; break; }
                                if (newEye) res.Add(p); 
                            }
                        }
                    }
            }
            return res;
        }

        static List<CvPoint> OnlyEyesOnTheFaceRemain(IplImage image, List<CvPoint> eyeCenters, int lineLength, double skinPointPart)
        {
            var res= new List<CvPoint>();
            var filteredImage = Helpers.Filter(image.ToHls(), Helpers.Filters.Skin);
            foreach(CvPoint eye in eyeCenters)
            {
                CvRect innerRect = new CvRect(eye.X - lineLength, eye.Y - lineLength / 2, lineLength * 2, lineLength);
                CvRect outerRect = new CvRect(innerRect.X - lineLength, innerRect.Y - lineLength, innerRect.Width + lineLength * 2, innerRect.Height + lineLength * 2);
                int totalSkinCount = outerRect.Width * outerRect.Height - innerRect.Width * innerRect.Height;
                int skinPointCount=0;
                //бежим по пространству между внешним и внутренним прямоугольником и считаем, сколько точек с кожей
                for (int row = Math.Max(0,outerRect.X); row < Math.Min(outerRect.X + outerRect.Width, image.Width); row++)
                    for (int column = Math.Max(0,outerRect.Y); column < Math.Min(outerRect.Y + outerRect.Height, image.Height); column++)
                    {
                        if(innerRect.Contains(column,row)) continue; 
                        if (filteredImage[ column, row].Val0 != 0)
                            skinPointCount++;
                    }
                //если проверка пройдена, добавляем в результат
                if ((double)skinPointCount / totalSkinCount >= skinPointPart)
                    res.Add(eye);

            }

            return res;
        }

        static bool IsThereTwoOpenedEyes(List<CvPoint> candidates, out List<CvPoint> coupleOfEyesCanBeEmpty, int lineLength)
        {
            coupleOfEyesCanBeEmpty = new List<CvPoint>();
            //попарно проверяем взаимное расположение
            for(int i =0;i< candidates.Count-1;i++)
                for (int j = i + 1; j < candidates.Count; j++)
                {
                    CvPoint first = candidates[i], second = candidates[j];
                    int horDiff = Math.Abs(first.X - second.X),
                        vertDiff = Math.Abs(first.Y - second.Y);
                    if (horDiff > 1.5 * lineLength && horDiff > 5 * lineLength && vertDiff < lineLength * 2)
                    {
                        coupleOfEyesCanBeEmpty.Add(first);
                        coupleOfEyesCanBeEmpty.Add(second);
                        return true;
                    }
                }
            return false;
        }
    }
}
