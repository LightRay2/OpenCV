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

        //попробовать по горизонтали только брать ширину

        public static bool Check(IplImage image, out IplImage transformedImage)
        {
            var smoothed = Noise.EvaluateNoise(image, Noise.EvaluateType.Blur3); //тут важно не переборщить с площадью сглаживания

           // AddInfoInPoint(smoothed, ColorText);

            //  var blackWhite = Helpers.Filter(smoothed, Helpers.Filters.BlackWhite);

            var imgLightLevels = new IplImage(image.Width, image.Height, BitDepth.U8, 1);
            Cv.Split(smoothed, null, imgLightLevels, null, null);

            var imgLightEdges = Edges.CannyEdges(imgLightLevels, 255, 0);
            // Edges.HoughLines(imgLightEdges);

            //!!!!
            /*
             using (new CvWindow(image))
            {

            CannyTester(imgLightLevels);

            }*/
            // transformedImage = Edges.LineSegments(imgLightEdges, 8);

            var lines = Edges.GetBigLines(imgLightEdges, 50, 8);
            var filtered = FilterCloseLines(lines, LengthToDist: 20);
            // filtered = filtered.GetRange(4, 4); //test for big lines with min 200
            var parallLines = GetParallelCouples(filtered, maxAngleDif: 20);
            var rects = GetPossibleRects(ref parallLines, minArea : 30.0*20.0);
            rects = FilterByPerimeter(rects, minPart: 0.95);

            //дальше важный этап, который должен отсечь почти все ложные отклики
            //смотрим цвет по обе стороны от линии внутри прямоугольников, должен быть одинаковый
            //сначала берем точку внутри, затем делаем несколько шагов наружу в поисках такой же
            //(пока закомментировал, т к удаляет часть правильных откликов)
           // rects = FilterRectsByColor(smoothed, rects);


            IplImage imgRects = image.EmptyClone();
            imgRects.Zero();
            foreach (var rect in rects)
            {
                if (rect.InsideImage(imgRects))
                    imgRects = Line.DrawLines(imgRects, false, rect.rectLines);
            }

            transformedImage = Chapter2.Show4Pictures(smoothed, imgLightEdges.FromGrayToBgr(), Line.DrawLines(imgRects, true, filtered.ToArray()), imgRects);
            //transformedImage = Line.DrawLines(imgLightEdges, true, filtered[4], filtered[5], filtered[6], filtered[7]);
            //transformedImage = filtered;
            // transformedImage = Edges.LineSegments( imgLightEdges, 5);
            //transformedImage = Edges.GetBigContours(imgLightEdges, 100);
            return true;


        }

        static bool AreSameColors(IplImage image, CvPoint2D64f one, CvPoint2D64f two, int maxDifColor)
        {
            CvScalar oneColor = image.Get2D((int)Math.Round(one.Y), (int)Math.Round(one.X));
            CvScalar twoColor = image.Get2D((int)Math.Round(two.Y), (int)Math.Round(two.X));
            return Math.Abs(oneColor.Val0 - twoColor.Val0) <= maxDifColor
                && Math.Abs(oneColor.Val1 - twoColor.Val1) <= maxDifColor
                && Math.Abs(oneColor.Val2 - twoColor.Val2) <= maxDifColor;
            
        }

        static List<Rect> FilterRectsByColor(IplImage colorImage, List<Rect> rects, int maxColorDif = 30, int pointsToCheck = 20, double threshhold = 0.59)
        {
            var res = new List<Rect>();
            int pointsPerLine = (int)Math.Ceiling((double)pointsToCheck / 4);

            foreach (var rect in rects)
            {
                int goodCount = 0;
                var rectPoints = new List<CvPoint2D64f> { rect.rectLines[0].a, rect.rectLines[0].b, rect.rectLines[2].b, rect.rectLines[2].a};
                for(int i =0; i < 4; i++)
                {
                    int next = (i+1)%4, parallel = (i+2)%4;

                    //точки
                    List<CvPoint2D64f> points = new List<CvPoint2D64f>();
                    {
                        //на сколько сдвигаемся
                        CvPoint2D64f first = rectPoints[i],
                            second = rectPoints[next];
                        CvPoint2D64f d = (second - first) * (1.0/(pointsPerLine+2));
                        for(int j = 1; j <= pointsPerLine; j++)
                        {
                            points.Add(first + d * j);
                        }
                    }

                    //ищем тестовые отступы внутрь и наружу
                    //todo тут можно и более параметризованно написать
                    CvPoint2D64f norm = ((CvPoint2D64f)(rectPoints[parallel] - rectPoints[i]))
                        * (1.0 / (rectPoints[parallel].DistanceTo(rectPoints[i])));
                    CvPoint2D64f[] insides = new CvPoint2D64f[] { norm * 3, norm * 4, norm * 5 },
                        outsides = new CvPoint2D64f[] {  norm * (-3), norm* (-4), norm * (-5) }; //тут слишком близко лучше не брать


                    //сравниваем
                    foreach (var point in points)
                    {
                        if (point.X < 6 || point.Y < 6 || point.X > colorImage.Width - 7 || point.Y > colorImage.Height - 7)
                            continue; //слишком близкие точки пропускаем для скорости

                        var doublePoint = (CvPoint2D64f)point;
                        bool theSameColor = false;
                        foreach (var inside in insides)
                        {
                            foreach (var outside in outsides)
                                if (AreSameColors(colorImage, doublePoint + inside, doublePoint + outside, maxColorDif))
                                {
                                    theSameColor = true; break;
                                }
                            if (theSameColor) break;
                        }
                        if (theSameColor) goodCount++;
                    }

                }

                if((double)goodCount / (pointsPerLine*4) >= threshhold)
                    res.Add(rect);
            }

            return res;
        }

        static List<Line> FilterCloseLines(List<Line> lines, double LengthToDist = 15)
        {
            List<Line> res = new List<Line>(lines);

            for (int i = 0; i < res.Count - 1; i++)
                for (int j = i + 1; j < res.Count; j++)
                {
                    double dist1 = res[i].a.DistanceTo(res[j].a),
                        dist2 = res[i].b.DistanceTo(res[j].b);
                    if (Math.Max(dist1, dist2) <= Math.Min(res[i].length, res[j].length) / LengthToDist)
                    {
                        res.RemoveAt(j); j--; //delete close line
                    }
                }
            return res;
        }

        static List<ParallelLine> GetParallelCouples(List<Line> lines, double maxAngleDif = 10)
        {
            List<ParallelLine> res = new List<ParallelLine>();
            for (int i = 0; i < lines.Count - 1; i++)
                for (int j = i + 1; j < lines.Count; j++)
                {
                    if (lines[i].angleToLine(lines[j]) <= maxAngleDif)
                        res.Add(new ParallelLine(lines[i], lines[j]));
                }
            return res;
        }

        static List<Rect> GetPossibleRects(ref List<ParallelLine> parall, double minAngle = 80, double maxAngle = 100,
            double minWidhthToHeigth = 1.3, double maxWidthToHeight = 1.7,
            double minArea = 10.0)
        {
            // parall = new List<ParallelLine>( parall.OrderBy(x => x.dist));
            List<Rect> res = new List<Rect>();
            for (int i = 0; i < parall.Count - 1; i++)
                for (int j = i + 1; j < parall.Count; j++)
                {
                    double angleBetween = Line.GetAngleBetween(parall[i].angle, parall[j].angle);
                    if (angleBetween >= minAngle && angleBetween <= maxAngle)
                    {
                        Rect rect = new Rect(parall[i], parall[j]);

                        double k = (rect.rectLines[0].length > rect.rectLines[1].length) ?
                            rect.rectLines[0].length / rect.rectLines[1].length :
                             rect.rectLines[1].length / rect.rectLines[0].length;

                        if (k >= minWidhthToHeigth && k <= maxWidthToHeight && rect.rectLines[0].length * rect.rectLines[1].length >= minArea)
                        {
                            res.Add(rect);
                        }
                    }
                }
            return res;
        }

        const double E = 0.01;
        static bool DoubleEqual(double a, double b)
        {
            return Math.Abs(a - b) < E;
        }

        static bool LineContainsPoint(Line line, CvPoint2D64f point)
        {
            return DoubleEqual(line.length, point.DistanceTo(line.a) + point.DistanceTo(line.b));
        }

        static List<Rect> FilterByPerimeter(List<Rect> rects, double minPart = 0.9)
        {
            var res = new List<Rect>();
            foreach (var rect in rects)
            {
                double rectPerimeter = rect.rectLines.Sum(x => x.length);
                double linePerimeter = 0;
                for (int i = 0; i < 4; i++)
                {
                    //найти общую линию
                    bool firstOnLine = LineContainsPoint(rect.rectLines[i], rect.sourceLines[i].a),
                        secondOnLine = LineContainsPoint(rect.rectLines[i], rect.sourceLines[i].b);
                    double length;
                    if (firstOnLine && secondOnLine)
                        length = rect.sourceLines[i].length;
                    else if (!firstOnLine && !secondOnLine)  //или линия сбоку, или по обе стороны
                        length =   LineContainsPoint(rect.sourceLines[i], rect.rectLines[i].a) ? rect.rectLines[i].length :0;
                    else if (firstOnLine)
                        length = rect.sourceLines[i].a.DistanceTo(rect.rectLines[i].b);
                    else
                        length = rect.sourceLines[i].b.DistanceTo(rect.rectLines[i].a);

                    linePerimeter += length;
                }
                double k = linePerimeter / rectPerimeter;
                if (k >= minPart)
                    res.Add(rect);
            }
            return res;
        }
        //old
        public static void AddInfoInPoint(IplImage image, Func<IplImage, int, int, string> GetText)
        {
            using (CvFont font = new CvFont(FontFace.HersheyComplex, 0.7, 0.7))
            using (var window = new CvWindow("info"))
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
            using (IplImage hls = image.ToHls())
                return "HLS: " + hls[y, x].ToString() + " BGR: " + image[y, x].ToString();
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
    }
}
