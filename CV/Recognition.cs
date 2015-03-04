using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV
{
    class Recognition
    {


        public Recognition(IplImage template, IplImage scene)
        {
            double startTime =  Cv.GetTickCount();
            template = template.ChangeSize(0.2);
            CvMat matching = TemplateMatchMatrix(template, scene);
            Cv.MatchTemplate(scene, template, matching, MatchTemplateMethod.CCorrNormed);

            double endTime = Cv.GetTickCount();
            double msTime = 1000.0 * (endTime - startTime) / Cv.GetTickFrequency();

            using (new CvWindow(template))
            using(new CvWindow(scene))
            using (new CvWindow(TemplateChamfer(scene)))
            {
                Cv.WaitKey();
            }
        }

        public static CvMat TemplateMatchMatrix(IplImage template, IplImage scene)
        {
            return new CvMat(scene.Height - template.Height + 1, scene.Width - template.Width + 1, MatrixType.F32C1);
        }

        public static IplImage TemplateChamfer(IplImage image)
        {
            IplImage res = new IplImage(image.Size, BitDepth.F32, 1);
            var edgeImage = Edges.CannyEdges(image.Grey(), 100, 200);
            edgeImage.Threshold(edgeImage, 127, 255, ThresholdType.BinaryInv);
            edgeImage.DistTransform(res, DistanceType.L2, 3);
            return res;
        }
    }
}
