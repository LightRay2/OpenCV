using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CV
{
    class VideoWriter
    {
        string address = @"output.avi";
        string address2 = @"tanya.avi";
        string addressAlbumPaper = "AlbumPaper.wmv";
        string addressAim = "Aim.wmv";
        public delegate bool FrameChecker(IplImage image, out IplImage transformedImage);

        public VideoWriter()
        {
            CheckVideoFile(addressAim, AlbumPaper.Check, true,0);
            // WriteToAviFile(address2);
        }

        public static void CheckVideoFile(string filename, FrameChecker Checker, bool showTransformedImage, int startFromFrame = 0)
        {
            using (var capture = CvCapture.FromFile(filename))
            using (CvFont font = new CvFont(FontFace.HersheyComplex, 0.7, 0.7))
            using (CvWindow window = new CvWindow("Capture", WindowMode.AutoSize))
            using (CvWindow transformedWindow = new CvWindow("transformed", WindowMode.AutoSize))
            {
                // (4)カメラから画像をキャプチャし，ファイルに書き出す
                int frames = 0;
                for (; frames < startFromFrame; frames++) 
                    capture.QueryFrame();
                for (; frames < capture.FrameCount; frames++)
                {
                    IplImage frame = capture.QueryFrame(), transformedImage;
                    if (frame == null) break;
                    string str = string.Format("{0}[frame]", frames);


                    if (Checker(frame, out transformedImage))
                        frame.PutText("[DETECTED]", new CvPoint(10, 50), font, new CvColor(0, 255, 100));
                    frame.PutText(str, new CvPoint(10, 20), font, new CvColor(0, 255, 100));
                    window.ShowImage(frame);

                    if (showTransformedImage)
                        transformedWindow.ShowImage(transformedImage);

                    int key = CvWindow.WaitKey(100);
                    if (key == '\x1b')
                    {
                        break;
                    }
                    if (key == ' ' || frames == startFromFrame)
                    {
                        CvWindow.WaitKey();
                    }
                }
            }
        }

        public static void WriteToAviFile(string filename)
        {
            // (1)カメラに対するキャプチャ構造体を作成する
            using (CvCapture capture = CvCapture.FromCamera(0))
            {
                // (2)キャプチャサイズを取得する(この設定は，利用するカメラに依存する)
                int width = capture.FrameWidth;
                int height = capture.FrameHeight;
                double fps = 15;//capture.Fps;
                // (3)ビデオライタ構造体を作成する
                using (CvVideoWriter writer = new CvVideoWriter(filename, FourCC.XVID, fps, new CvSize(width, height), true))
                using (CvFont font = new CvFont(FontFace.HersheyComplex, 0.7, 0.7))
                using (CvWindow window = new CvWindow("Capture", WindowMode.AutoSize))
                {
                    // (4)カメラから画像をキャプチャし，ファイルに書き出す
                    for (int frames = 0; ; frames++)
                    {
                        IplImage frame = capture.QueryFrame();
                        string str = string.Format("{0}[frame]", frames);
                        frame.PutText(str, new CvPoint(10, 20), font, new CvColor(0, 255, 100));
                        writer.WriteFrame(frame);
                        window.ShowImage(frame);

                        int key = CvWindow.WaitKey((int)(1000 / fps));
                        if (key == '\x1b')
                        {
                            break;
                        }
                    }
                }
            }
        }

    }
}
