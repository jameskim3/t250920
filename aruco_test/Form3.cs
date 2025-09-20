using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Size = OpenCvSharp.Size;
using Point = OpenCvSharp.Point;

namespace aruco_test
{

    using cvsize = OpenCvSharp.Size;
    using dsize = System.Drawing.Size;
    using dpoint = System.Drawing.Point;

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
    }
}

