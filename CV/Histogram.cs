using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Utilities;

namespace CV
{
    class Histogram
    {
        int histWidth = 512;
        public void GreyScaleHist(IplImage image)
        {
            IplImage greyscale = new IplImage(image.Size, image.Depth, 1);
            image.CvtColor(greyscale, ColorConversion.BgrToGray);
           // CvMatND hist = new CvMatND(2, new int[]{image.Height, image.Width}, image.ElemType);
            CvHistogram hist = new CvHistogram(new int[] { histWidth }, HistogramFormat.Array, new float[][] { new float[] { 0, 256 } }, true);
            Cv.CalcHist(greyscale, hist);

            using (new CvWindow(DrawHist( hist, histWidth)))
            {
                Cv.WaitKey();
            }
        }

        IplImage DrawHist(CvHistogram hist, int histSize)
        {
            IplImage img = new IplImage(1024, 768, BitDepth.U8, 3);
            float minValue, maxValue;
            hist.GetMinMaxValue(out minValue, out maxValue);
            Cv.Scale(hist.Bins, hist.Bins, ((double)img.Height) / maxValue, 0);
            img.Set(CvColor.White);
            int binW = Cv.Round((double)img.Width /  histSize);
            for (int i = 0; i < histSize; i++)
            {
                img.Rectangle(
                    new CvPoint(i * binW, img.Height),
                    new CvPoint((i + 1) * binW, img.Height - Cv.Round(hist.Bins[i])),
                    CvColor.Black, -1, LineType.AntiAlias, 0
                );
            }
            return img;
        }

        public void DrawRGBHist(IplImage image)
        {
            IplImage r = new IplImage(image.Size, BitDepth.U8, 1),
                g = new IplImage(image.Size, BitDepth.U8, 1),
                b = new IplImage(image.Size, BitDepth.U8, 1);

            
            Cv.Split(image, b, g, r, null);

            List<IplImage> histList = GetHistogramList(r, g, b);

            var c = new Chapter2();
            c.Show4Pictures(image, histList[0], histList[1], histList[2]);
        }

        public void DrawHLSHist(IplImage image)
        {
            IplImage h = new IplImage(image.Size, BitDepth.U8, 1),
                l = new IplImage(image.Size, BitDepth.U8, 1),
                s = new IplImage(image.Size, BitDepth.U8, 1);

            IplImage hlsImage = new IplImage(image.Size, image.Depth, image.NChannels);
            Cv.CvtColor(image, hlsImage, ColorConversion.BgrToHls);
            Cv.Split(hlsImage, h, l, s, null);

            List<IplImage> histList = GetHistogramList(h, l, s);

            var c = new Chapter2();
            c.Show4Pictures(image, histList[0], histList[1], histList[2]);
        }

        public List<IplImage> GetHistogramList(params IplImage[] images)
        {
            List<IplImage> histList = new List<IplImage>();
            CvHistogram hist = new CvHistogram(new int[] { histWidth }, HistogramFormat.Array, new float[][] { new float[] { 0, 256 } }, true);
            foreach (var image in images)
            {
                Cv.CalcHist(image, hist);
                histList.Add(DrawHist(hist, histWidth));
            }
            return histList;

        }

        public void EqualizeLuminance(IplImage image)
        {
             IplImage h = new IplImage(image.Size, BitDepth.U8, 1),
                l = new IplImage(image.Size, BitDepth.U8, 1),
                s = new IplImage(image.Size, BitDepth.U8, 1);

            IplImage hlsImage = new IplImage(image.Size, image.Depth, image.NChannels);
            Cv.CvtColor(image, hlsImage, ColorConversion.BgrToHls);
            Cv.Split (hlsImage, h,l,s,null);
            Cv.EqualizeHist(l,l);
            Cv.Merge(h,l,s,null,hlsImage);
            Cv.CvtColor(hlsImage, image, ColorConversion.HlsToBgr);
            DrawHLSHist(image);
            
        }

        public static IplImage NormalizeLuminance(IplImage image)
        {
            IplImage res = image.Same();
            IplImage h = new IplImage(image.Size, BitDepth.U8, 1),
                l = new IplImage(image.Size, BitDepth.U8, 1),
                s = new IplImage(image.Size, BitDepth.U8, 1);

            IplImage hlsImage = new IplImage(image.Size, image.Depth, image.NChannels);
            Cv.CvtColor(image, hlsImage, ColorConversion.BgrToHls);
            Cv.Split(hlsImage, h, l, s, null);
            Cv.EqualizeHist(l, l);
            Cv.Merge(h, l, s, null, hlsImage);
            Cv.CvtColor(hlsImage, res, ColorConversion.HlsToBgr);
            return res;
        }

        public List<CvHistogram> GetHistList(IplImage image)
        {
            List<IplImage> channels = new List<IplImage>();
            for(int i = 0; i < image.NChannels;i++)
                channels.Add(new IplImage(image.Size, BitDepth.U8, 1));

            IplImage p1,p2,p3,p4;
            p1=p2=p3=p4=null;
            if(channels.Count>=1) p1=channels[0]; 
            if(channels.Count>=2) p2=channels[1];
            if(channels.Count>=3) p3=channels[2];
            if(channels.Count>=4) p4=channels[3];

            Cv.Split(image, p1, p2, p3, p4);
            List<CvHistogram> res = new List<CvHistogram>();
            foreach (var ch in channels)
            {
                CvHistogram hist = new CvHistogram(new int[] { histWidth }, HistogramFormat.Array, new float[][] { new float[] { 0, 256 } }, true);
                Cv.CalcHist(ch, hist);
                res.Add(hist);
            }
            return res;
        }

        public enum CompareType { Correlation, Chi_square, Intersection, Bhattacharyya, EarthMoversDistance }
        public double CompareImagesByHistograms(IplImage one, IplImage two, CompareType type)
        {
            List<CvHistogram> oneHist = GetHistList(one),
                twoHist = GetHistList(two);
            if(oneHist.Count != twoHist.Count)
                throw new Exception("Different channel count");

            double sum = 0;
            for (int i = 0; i < oneHist.Count; i++)
            {
                switch (type)
                {
                    case CompareType.Correlation:
                        sum += Cv.CompareHist(oneHist[i], twoHist[i], HistogramComparison.Correl);
                        break;
                    case CompareType.Chi_square:
                        sum += Cv.CompareHist(oneHist[i], twoHist[i], HistogramComparison.Chisqr);
                        break;
                    case CompareType.Intersection:
                        sum += Cv.CompareHist(oneHist[i], twoHist[i], HistogramComparison.Intersect);
                        break;
                    case CompareType.Bhattacharyya:
                        sum += Cv.CompareHist(oneHist[i], twoHist[i], HistogramComparison.Bhattacharyya);
                        break;
                    case CompareType.EarthMoversDistance:
                        throw new NotImplementedException();
                        //sum+= Cv.CalcEMD2(oneHist[i], twoHist[i], DistanceType.
                        break;
                }
            }
            return sum / oneHist.Count;
        }

        //Недоразобрал
        /*
        public void KMeans(IplImage image, int k)
        {
            int iterations=6;
            CvMat samples = new CvMat(image.Width * image.Height, 3, MatrixType.F32C1);
            unsafe
            {
                float* sample = (float*)samples.CvPtr;
                for (int row = 0; row < image.Height; row++)
                    for (int col = 0; col < image.Width; col++)
                        for (int channel = 0; channel < 3; channel++)
                            samples.Set2D(row * image.Width + col, channel, image[row, col][channel]);

                CvMat labels = new CvMat(samples.Height, samples.Width, MatrixType.S32C1);
                CvMat centres = new CvMat(k , samples.Width, samples.ElemType);
                Cv.KMeans2(samples, k, labels, new CvTermCriteria(100, 0.0001),  iterations, new CvRNG(),  KMeansFlag.PpCenters, centres );

                CvMat res = new CvMat(image.Height, image.Width, image.ElemType);
                for (int row = 0; row < image.Height; row++)
                    for (int col = 0; col < image.Width; col++)
                            res.Set2D(row, col, new CvScalar(
                                (byte)centres.Get2D(((labels.DataInt32)[ row * image.Width + col]), 0),
                               (byte)centres.Get2D(((labels.DataInt32)[ row * image.Width + col]), 1),
                               (byte)centres.Get2D(((labels.DataInt32)[ row * image.Width + col]), 2)));

                using (new CvWindow(res))
                {
                    Cv.WaitKey();
                }
            }
        }*/

        public void KMeansExample(IplImage srcImg,int k){
            // cvKMeans2
            // k-means法によるクラスタリングを利用して，非常に単純な減色を行う

            // クラスタ数。この値を変えると色数が変わる
            int maxClusters = k;

            // (1)画像を読み込む  
            using (IplImage dstImg = Cv.CloneImage(srcImg))
            {
                int size = srcImg.Width * srcImg.Height;
                using (CvMat color = Cv.CreateMat(maxClusters, 1, MatrixType.F32C3))
                using (CvMat count = Cv.CreateMat(maxClusters, 1, MatrixType.S32C1))
                using (CvMat clusters = Cv.CreateMat(size, 1, MatrixType.S32C1))
                using (CvMat points = Cv.CreateMat(size, 1, MatrixType.F32C3))
                {
                    // (2)ピクセルの値を行列へ代入
                    // unsafeコードを用いて、C/C++と同様にポインタで要素にアクセスする。
                    // ポインタがないVB.NETの場合は、Marshal.Copyでマネージ配列に移し替えてからアクセスし、再びMarshal.Copyで戻せばできる？
                    // (またはMarshal.WriteInt32とか)
                    unsafe
                    {
                        // 以下のように、ポインタ変数に移し替えてからアクセスした方が良いと思われる
                        float* pMat = (float*)points.Data;       // 行列の要素へのポインタ
                        byte* pImg = (byte*)srcImg.ImageData;   // 画像の画素へのポインタ
                        for (int i = 0; i < size; i++)
                        {
                            pMat[i * 3 + 0] = pImg[i * 3 + 0];
                            pMat[i * 3 + 1] = pImg[i * 3 + 1];
                            pMat[i * 3 + 2] = pImg[i * 3 + 2];
                        }
                    }
                    // (3)クラスタリング
                    Cv.KMeans2(points, maxClusters, clusters, new CvTermCriteria(10, 1.0));
                    // (4)各クラスタの平均値を計算
                    Cv.SetZero(color);
                    Cv.SetZero(count);
                    unsafe
                    {
                        // ポインタそのままで取得できるプロパティもある
                        int* pClu = clusters.DataInt32;    // cluster の要素へのポインタ
                        int* pCnt = count.DataInt32;       // count の要素へのポインタ
                        float* pClr = color.DataSingle;    // color の要素へのポインタ
                        float* pPnt = points.DataSingle;   // points の要素へのポインタ
                        for (int i = 0; i < size; i++)
                        {
                            int idx = pClu[i];     // clusters->data.i[i]
                            int j = ++pCnt[idx];   // ++count->data.i[idx];
                            pClr[idx * 3 + 0] = pClr[idx * 3 + 0] * (j - 1) / j + pPnt[i * 3 + 0] / j;
                            pClr[idx * 3 + 1] = pClr[idx * 3 + 1] * (j - 1) / j + pPnt[i * 3 + 1] / j;
                            pClr[idx * 3 + 2] = pClr[idx * 3 + 2] * (j - 1) / j + pPnt[i * 3 + 2] / j;
                        }
                    }
                    // (5)クラスタ毎に色を描画
                    unsafe
                    {
                        int* pClu = clusters.DataInt32;        // cluster の要素へのポインタ
                        float* pClr = color.DataSingle;        // color の要素へのポインタ
                        byte* pDst = (byte*)dstImg.ImageData;     // dst の画素へのポインタ
                        for (int i = 0; i < size; i++)
                        {
                            int idx = pClu[i];
                            pDst[i * 3 + 0] = (byte)pClr[idx * 3 + 0];
                            pDst[i * 3 + 1] = (byte)pClr[idx * 3 + 1];
                            pDst[i * 3 + 2] = (byte)pClr[idx * 3 + 2];
                        }
                    }
                }
                // (6)画像を表示，キーが押されたときに終了
                using (new CvWindow("src", WindowMode.AutoSize, srcImg))
                using (new CvWindow("low-color", WindowMode.AutoSize, dstImg))
                {
                    Cv.WaitKey(0);
                }
            }
        }
        
    }
}
