using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CV
{
    class Line
    {
        /// <summary>
        /// a всегда выше b(или левее, если высоты равны) 
        /// </summary>
        public CvPoint2D64f a, b, center;
        public double length;
        /// <summary>
        /// (0-180] , как в школьных координатах 
        /// </summary>
        public double angle;

        public Line(CvPoint2D64f a, CvPoint2D64f b)
        {
            this.a = a;
            this.b = b;
            length = a.DistanceTo(b);
            center = new CvPoint2D64f((a.X + b.X) / 2, (a.Y + b.Y) / 2);

            angle = Math.Atan2(b.Y - a.Y, b.X - a.X) ;
            if (angle <= 0) 
            {
                angle += Math.PI;
                CvPoint2D64f temp = this.a; this.a = this.b; this.b = temp;
            }

            angle = angle / Math.PI * 180;
        }

        public static IplImage DrawLines(IplImage image, bool clearBackground, params Line[] lines)
        {
            IplImage res = image.Clone();
            if (clearBackground)
                res.Zero();
            foreach(var line in lines)
            res.DrawLine(line.a, line.b, CvColor.White);
            return res;
        }

        public double angleToLine(Line other)
        {
            return GetAngleBetween(angle, other.angle);
        }


        /// <summary>
        /// Calculates intersection - if any - of two lines
        /// </summary>
        /// <param name="otherLine"></param>
        /// <returns>Intersection or null</returns>
        /// <remarks>Taken from <a href="http://tog.acm.org/resources/GraphicsGems/gemsii/xlines.c">http://tog.acm.org/resources/GraphicsGems/gemsii/xlines.c</a> </remarks>
        public CvPoint2D64f? Intersection(Line otherLine)
        {
            double a1 = b.Y - a.Y;
            double b1 = a.X - b.X;
            double c1 = b.X * a.Y - a.X * b.Y;

            /* Compute r3 and r4.
             */

         //   var r3 = a1 * otherLine.a.X + b1 * otherLine.a.Y + c1;
         //   var r4 = a1 * otherLine.b.X + b1 * otherLine.b.Y + c1;

            /* Check signs of r3 and r4.  If both point 3 and point 4 lie on
             * same side of line 1, the line segments do not intersect.
             */

           // if (r3 != 0 && r4 != 0 && Math.Sign(r3) == Math.Sign(r4))
           // {
           //     return null; // DONT_INTERSECT
          //  }

            /* Compute a2, b2, c2 */

            double a2 = otherLine.b.Y - otherLine.a.Y;
            double b2 = otherLine.a.X - otherLine.b.X;
            double c2 = otherLine.b.X * otherLine.a.Y - otherLine.a.X * otherLine.b.Y;

            /* Compute r1 and r2 */

           // var r1 = a2 * a.X + b2 * a.Y + c2;
          //  var r2 = a2 * b.X + b2 * b.Y + c2;

            /* Check signs of r1 and r2.  If both point 1 and point 2 lie
             * on same side of second line segment, the line segments do
             * not intersect.
             */
           // if (r1 != 0 && r2 != 0 && Math.Sign(r1) == Math.Sign(r2))
           // {
           //     return (null); // DONT_INTERSECT
          //  }

            /* Line segments intersect: compute intersection point. 
             */

            double denom = a1 * b2 - a2 * b1;
            if (denom == 0)
            {
                return null; //( COLLINEAR );
            }
         //   double offset = denom < 0 ? -denom / 2 : denom / 2;
            double offset = 0;
            /* The denom/2 is to get rounding instead of truncating.  It
             * is added or subtracted to the numerator, depending upon the
             * sign of the numerator.
             */

            double num = b1 * c2 - b2 * c1;
            double x = (num < 0 ? num - offset : num + offset) / denom;

            num = a2 * c1 - a1 * c2;
            double y = (num < 0 ? num - offset : num + offset) / denom;
            return new CvPoint2D64f(x, y);
        }

        public static double GetAngleBetween(double oneAngle, double twoAngle)
        {
            return Math.Min(Math.Abs(oneAngle - twoAngle), Math.Abs(oneAngle + 180 - twoAngle));
        }
    }

    class ParallelLine
    {
        public Line a, b;
        public double angle, dist;

        public ParallelLine(Line one, Line two)
        {
            a = one;
            b = two;
            dist = one.center.DistanceTo(two.center);

            double diffAngle = one.angleToLine(two);
            if (one.angle - two.angle <= 90)
            {
                angle = (two.angle + diffAngle) ;
            }
            else
            {
                angle = (one.angle + diffAngle) ;
            }
            if (angle > 180) angle -= 180;
        }
    }

    class Rect
    {
        public ParallelLine a, b;
        public Line[] sourceLines = new Line[4];
        public Line[] rectLines = new Line[4];
        public Rect(ParallelLine one, ParallelLine two)
        {
            a = one; b = two;
            //возьмем первую прямую и посмотрим, что ближе к ее верхней точке, чем к нижней
            Line first = sourceLines[0] = one.a;
            sourceLines[2] = one.b;
            CvPoint2D64f cross1 = (CvPoint2D64f)first.Intersection(two.a),
                cross2 = (CvPoint2D64f)first.Intersection(two.b);

            Line crosses = new Line(cross1, cross2); //тут они отсортировались как надо, crosses.a соответствует first.a
            

            bool cross1LiesBetween = Math.Abs( first.length - (first.a.DistanceTo(cross1) + first.b.DistanceTo(cross1))) < 0.1;

            if (crosses.a.X == cross1.X && crosses.a.Y == cross1.Y)
            {
                //значит прямая two.a следующая
                sourceLines[1] = two.a;
                sourceLines[3] = two.b;
            }
            else
            {
                sourceLines[1] = two.b;
                sourceLines[3] = two.a;
            }

            for (int i = 0; i < 4; i++)
            {
                rectLines[i] = new Line((CvPoint2D64f)sourceLines[i].Intersection(sourceLines[(i - 1 + 4) % 4]),
                    (CvPoint2D64f)sourceLines[i].Intersection(sourceLines[(i + 1) % 4]));
            }

        }

        public bool InsideImage(IplImage image)
        {
            var points = new List<CvPoint2D64f>();

            foreach (var l in rectLines)
            {
                points.Add(l.a);
                points.Add(l.b);
            }

            foreach (var point in points)
            {
                if (point.X < 0 || point.Y < 0 || point.X >= image.Width || point.Y >= image.Height) return false;
            }
            return true;
        }
    }

    public static class PointExtension
    {
        public static double DistanceTo(this CvPoint2D64f one, CvPoint2D64f two)
        {
            return Math.Sqrt((one.X - two.X)*(one.X - two.X) + (one.Y - two.Y)*(one.Y - two.Y));
        }
    }
}
