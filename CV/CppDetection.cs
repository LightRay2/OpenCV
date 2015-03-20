using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV
{
    class CppDetection
    {
        string path = @"D:\Misha\_PROJECTS\Training\img\pos\pos2.bmp";
        ﻿public CppDetection()
        {
            Run();
        }
        public void Run()
        {
            // Load the cascades
            var haarCascade = new CascadeClassifier(Detection.arrowCascade);

            // Detect faces
            Mat haarResult = DetectFace(haarCascade);

            Cv2.ImShow("Faces by Haar", haarResult);
            Cv2.WaitKey(0);
            Cv2.DestroyAllWindows();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cascade"></param>
        /// <returns></returns>
        private Mat DetectFace(CascadeClassifier cascade)
        {
            Mat result;

            using (var src = new Mat(path, LoadMode.Color))
            using (var gray = new Mat())
            {
                result = src.Clone();
                Cv2.CvtColor(src, gray, ColorConversion.BgrToGray, 0);

                // Detect faces
                OpenCvSharp.CPlusPlus.Rect[] faces = cascade.DetectMultiScale(
                    gray, 1.08, 2, HaarDetectionType.ScaleImage, new Size(30, 30));

                // Render all detected faces
                foreach (var face in faces)
                {
                    var center = new Point
                    {
                        X = (int)(face.X + face.Width * 0.5),
                        Y = (int)(face.Y + face.Height * 0.5)
                    };
                    var axes = new Size
                    {
                        Width = (int)(face.Width * 0.5),
                        Height = (int)(face.Height * 0.5)
                    };
                    Cv2.Ellipse(result, center, axes, 0, 0, 360, new Scalar(255, 0, 255), 4);
                }
            }
            return result;
        }
    }
}
