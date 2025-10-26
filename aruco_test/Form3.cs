using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace aruco_test
{

    using cvsize = OpenCvSharp.Size;
    using dpoint = System.Drawing.Point;
    using dsize = System.Drawing.Size;

    public partial class Form3 : Form
    {
        private CameraCalibrator2 calibrator = new();
        private DistortedChessboardGenerator generator = new();


        public Form3()
        {
            InitializeComponent();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            // img 폴더에 체스보드 이미지 있어야 함
            var folder = "img"; // 체스보드 이미지 폴더
            var patternSize = new Size(9, 6);
            float squareSize = 25f;
            bool success = calibrator.CalibrateFromFolder("img", new cvsize(9, 6), 25f);

            if (success)
                MessageBox.Show("캘리브레이션 성공!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("캘리브레이션 실패!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            MessageBox.Show("캘리브레이션 완료");

            // 오버레이 표시용
            var files = Directory.GetFiles(folder, "*.png");
            foreach (var file in files)
            {
                using var img = Cv2.ImRead(file);
                using var gray = img.CvtColor(ColorConversionCodes.BGR2GRAY);

                if (!Cv2.FindChessboardCorners(gray, patternSize, out Point2f[] corners))
                    continue;

                Cv2.CornerSubPix(gray, corners, new Size(11, 11), new Size(-1, -1),
                    new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.Count, 30, 0.001));

                Point3f[] objp = new Point3f[patternSize.Width * patternSize.Height];
                for (int y = 0; y < patternSize.Height; y++)
                    for (int x = 0; x < patternSize.Width; x++)
                        objp[y * patternSize.Width + x] = new Point3f(x * squareSize, y * squareSize, 0);
                using var projectedMat = new Mat();

                Cv2.ProjectPoints(InputArray.Create(objp),
                                  new Mat(),
                                  new Mat(),
                                  calibrator.CameraMatrix,
                                  calibrator.DistCoeffs,
                                  projectedMat);

                projectedMat.GetArray<Point2f>(out Point2f[] projected);

                for (int i = 0; i < corners.Length; i++)
                {
                    Cv2.Circle(img, (Point)corners[i], 3, new Scalar(0, 0, 255), -1);
                    Cv2.Circle(img, corners[i].ToPoint(), 3, new Scalar(0, 0, 255), -1);
                    Cv2.Circle(img, projected[i].ToPoint(), 3, new Scalar(255, 0, 0), -1);
                    Cv2.Line(img, corners[i].ToPoint(), projected[i].ToPoint(), new Scalar(100, 100, 100), 1);
                }

                pictureBox1.Image?.Dispose();
                pictureBox1.Image = BitmapConverter.ToBitmap(img.Clone());

                Application.DoEvents();
                Thread.Sleep(500);
            }

            MessageBox.Show("오버레이 완료");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string folderPath = "img";
            var patternSize = new cvsize(9, 6);
            float squareSize = 25f;

            var files = Directory.GetFiles(folderPath, "*.png");
            if (files.Length == 0)
            {
                MessageBox.Show("체스보드 이미지 없음");
                return;
            }

            foreach (var file in files)
            {
                using var image = Cv2.ImRead(file);
                using var gray = new Mat();
                Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);

                bool found = Cv2.FindChessboardCorners(gray, patternSize, out Point2f[] corners);

                if (found)
                {
                    // 코너 정밀화
                    Cv2.CornerSubPix(gray, corners, new cvsize(11, 11), new cvsize(-1, -1),
                        new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.Count, 30, 0.1));

                    // 이미지에 코너 표시 (오버레이)
                    Cv2.DrawChessboardCorners(image, patternSize, corners, found);
                }

                // WinForms PictureBox에 표시
                pictureBox1.Image?.Dispose();  // 이전 이미지 해제
                pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image.Clone());

                // 사용자에게 이미지 하나씩 확인할 시간 주기
                Application.DoEvents();  // UI 강제 업데이트
                Thread.Sleep(500);       // 0.5초 대기
            }

            MessageBox.Show("오버레이 표시 완료");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            generator.GenerateAndSaveOne();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string folderPath = "img"; // 체스보드 이미지 폴더
            var patternSize = new OpenCvSharp.Size(9, 6);
            float squareSize = 25f;

            string[] files = Directory.GetFiles(folderPath, "*.png");

            if (!calibrator.CalibrateFromFolder(folderPath, patternSize, squareSize))
            {
                MessageBox.Show("캘리브레이션 실패 또는 이미지 부족");
                return;
            }

            // 재투영 오차 오버레이 표시
            for (int i = 0; i < files.Length && i < calibrator.ImagePoints.Count; i++)
            {
                using var img = Cv2.ImRead(files[i]);
                //Cv2.ProjectPoints(calibrator.ObjectPoints[i],
                //                  calibrator.Rvecs[i],
                //                  calibrator.Tvecs[i],
                //                  calibrator.CameraMatrix,
                //                  calibrator.DistCoeffs,
                //                  out Point2f[] projected);

                //var actual = calibrator.ImagePoints[i];

                //for (int j = 0; j < actual.Length; j++)
                //{
                //    Cv2.Circle(img, actual[j], 3, Scalar.Red, -1);         // 검출 코너
                //    Cv2.Circle(img, projected[j], 3, Scalar.Blue, -1);     // 예측 코너
                //    Cv2.Line(img, actual[j], projected[j], Scalar.Gray, 1); // 오차 벡터
                //}

                pictureBox1.Image?.Dispose();
                pictureBox1.Image = img.ToBitmap();

                Application.DoEvents();
                Thread.Sleep(600);
            }

            MessageBox.Show("오버레이 완료");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string inputPath = @"C:\img\origin.bmp";
            string outputDir = @"C:\img\output";

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            using Bitmap originalSrc = new Bitmap(inputPath);
            using Bitmap original = Ensure24bpp(originalSrc); // 변환 작업용 24bpp 보정

            double[] xOffsets = GenerateRange(-100, 100, 5); // -100,-60,-20,20,60,100
            double[] yOffsets = GenerateRange(-100, 100, 5);
            double[] angles = GenerateRange(-10, 10, 5); // -10,-6,-2,2,6,10

            foreach (double x in xOffsets)
            {
                foreach (double y in yOffsets)
                {
                    foreach (double t in angles)
                    {
                        using Bitmap transformed = TransformImage24bpp(original, (float)x, (float)y, (float)t);

                        // 저장 직전 8bpp 그레이스케일로 변환(unsafe 없이)
                        using Bitmap gray8 = ConvertTo8bppGrayscale_NoUnsafe(transformed);

                        string filename = $"img_x{x:+0;-0}_y{y:+0;-0}_t{t:+0;-0}_8bpp.bmp";
                        gray8.Save(Path.Combine(outputDir, filename), ImageFormat.Bmp);
                    }
                }
            }
        }
        static double[] GenerateRange(double min, double max, int steps)
        {
            double[] result = new double[steps + 1];
            for (int i = 0; i <= steps; i++)
                result[i] = Math.Round(min + (max - min) * i / steps, 2);
            return result;
        }

        static Bitmap Ensure24bpp(Bitmap src)
        {
            if (src.PixelFormat == PixelFormat.Format24bppRgb)
                return (Bitmap)src.Clone();

            Bitmap dst = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb);
            dst.SetResolution(src.HorizontalResolution, src.VerticalResolution);
            using (Graphics g = Graphics.FromImage(dst))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawImage(src, 0, 0, src.Width, src.Height);
            }
            return dst;
        }

        static Bitmap TransformImage24bpp(Bitmap src24, float offsetX, float offsetY, float angleDeg)
        {
            Bitmap dest = new Bitmap(src24.Width, src24.Height, PixelFormat.Format24bppRgb);
            dest.SetResolution(src24.HorizontalResolution, src24.VerticalResolution);

            using (Graphics g = Graphics.FromImage(dest))
            {
                g.Clear(Color.Black); // 배경색 필요 시 변경
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                g.TranslateTransform(src24.Width / 2f + offsetX, src24.Height / 2f + offsetY);
                g.RotateTransform(angleDeg);
                g.TranslateTransform(-src24.Width / 2f, -src24.Height / 2f);

                g.DrawImage(src24, new System.Drawing.Point(0, 0));
            }
            return dest;
        }

        /// <summary>
        /// unsafe 없이 24bpp → 8bpp 그레이스케일 변환
        /// </summary>
        static Bitmap ConvertTo8bppGrayscale_NoUnsafe(Bitmap color)
        {
            // 입력은 24bpp 가정. 아니라면 24bpp로 보정
            Bitmap src = color.PixelFormat == PixelFormat.Format24bppRgb
                ? color
                : Ensure24bpp(color);

            int w = src.Width;
            int h = src.Height;

            // 목적지 8bpp 인덱스 비트맵 생성
            Bitmap gray = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
            gray.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            // 회색 팔레트 구성
            ColorPalette pal = gray.Palette;
            for (int i = 0; i < 256; i++) pal.Entries[i] = Color.FromArgb(i, i, i);
            gray.Palette = pal;

            // LockBits
            BitmapData srcData = null!;
            BitmapData dstData = null!;
            byte[] srcBuf = null!;
            byte[] dstBuf = null!;

            try
            {
                srcData = src.LockBits(new Rectangle(0, 0, w, h),
                                       ImageLockMode.ReadOnly,
                                       PixelFormat.Format24bppRgb);
                dstData = gray.LockBits(new Rectangle(0, 0, w, h),
                                        ImageLockMode.WriteOnly,
                                        PixelFormat.Format8bppIndexed);

                int srcStride = srcData.Stride;     // 24bpp: 3바이트/픽셀 + 라인 패딩
                int dstStride = dstData.Stride;     // 8bpp: 1바이트/픽셀 + 라인 패딩

                int srcBytes = Math.Abs(srcStride) * h;
                int dstBytes = Math.Abs(dstStride) * h;

                srcBuf = new byte[srcBytes];
                dstBuf = new byte[dstBytes];

                Marshal.Copy(srcData.Scan0, srcBuf, 0, srcBytes);

                // 라인별 순회 (stride를 사용해 패딩 안전 처리)
                for (int y = 0; y < h; y++)
                {
                    int srcRow = y * srcStride;
                    int dstRow = y * dstStride;

                    for (int x = 0; x < w; x++)
                    {
                        int s = srcRow + x * 3; // BGR
                        byte b = srcBuf[s + 0];
                        byte g = srcBuf[s + 1];
                        byte r = srcBuf[s + 2];

                        // BT.601 (0.299 R + 0.587 G + 0.114 B)
                        int lum = (int)(0.299 * r + 0.587 * g + 0.114 * b + 0.5);
                        if (lum < 0) lum = 0;
                        if (lum > 255) lum = 255;

                        dstBuf[dstRow + x] = (byte)lum;
                    }
                }

                Marshal.Copy(dstBuf, 0, dstData.Scan0, dstBytes);
            }
            finally
            {
                if (srcData != null) src.UnlockBits(srcData);
                if (dstData != null) gray.UnlockBits(dstData);
                if (!ReferenceEquals(src, color)) src.Dispose();
            }

            return gray;
        }
    }
}