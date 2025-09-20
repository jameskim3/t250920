using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Size = OpenCvSharp.Size;

namespace aruco_test
{
    public class CameraCalibrator2
    {
        public List<Point2f[]> ImagePoints { get; private set; } = new List<Point2f[]>();
        public List<Point3f[]> ObjectPoints { get; private set; } = new List<Point3f[]>();
        public Mat CameraMatrix { get; private set; }
        public Mat DistCoeffs { get; private set; }
        public List<Mat> RotationVectors { get; private set; }
        public List<Mat> TranslationVectors { get; private set; }

        // Point3f[] → Mat 변환
        private Mat ConvertObjectPoints(Point3f[] points)
        {
            var mat = new Mat(points.Length, 1, MatType.CV_32FC3);
            for (int i = 0; i < points.Length; i++)
                mat.Set(i, 0, new Vec3f(points[i].X, points[i].Y, points[i].Z));
            return mat;
        }

        // Point2f[] → Mat 변환
        private Mat ConvertImagePoints(Point2f[] points)
        {
            var mat = new Mat(points.Length, 1, MatType.CV_32FC2);
            for (int i = 0; i < points.Length; i++)
                mat.Set(i, 0, new Vec2f(points[i].X, points[i].Y));
            return mat;
        }

        public bool CalibrateFromFolder(string folderPath, Size patternSize, float squareSize)
        {
            var objectPointsList = new List<Mat>();
            var imagePointsList = new List<Mat>();

            // 기준 3D 포인트 생성
            Point3f[] objp = new Point3f[patternSize.Width * patternSize.Height];
            for (int y = 0; y < patternSize.Height; y++)
                for (int x = 0; x < patternSize.Width; x++)
                    objp[y * patternSize.Width + x] = new Point3f(x * squareSize, y * squareSize, 0);

            var files = Directory.GetFiles(folderPath, "*.png");
            if (files.Length == 0)
            {
                Console.WriteLine("이미지 파일이 없습니다.");
                return false;
            }

            foreach (var file in files)
            {
                using var gray = Cv2.ImRead(file, ImreadModes.Grayscale);
                bool found = Cv2.FindChessboardCorners(gray, patternSize, out Point2f[] corners);

                if (!found)
                {
                    Console.WriteLine($"코너 미발견: {file}");
                    continue;
                }

                Cv2.CornerSubPix(gray, corners, new Size(11, 11), new Size(-1, -1),
                    new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.Count, 30, 0.001));

                imagePointsList.Add(ConvertImagePoints(corners));
                objectPointsList.Add(ConvertObjectPoints(objp));

                Console.WriteLine($"코너 발견: {file}");
            }

            if (imagePointsList.Count < 3)
            {
                Console.WriteLine("캘리브레이션에 충분한 이미지가 아닙니다.");
                return false;
            }

            var imageSize = Cv2.ImRead(files[0]).Size();

            CameraMatrix = new Mat();
            DistCoeffs = new Mat();

            RotationVectors = new List<Mat>();
            TranslationVectors = new List<Mat>();

            double rms = Cv2.CalibrateCamera(
                objectPointsList,
                imagePointsList,
                imageSize,
                CameraMatrix,
                DistCoeffs,
                out _,
                out _);
            Debug.WriteLine($"RMS Error: {rms:F4}");
            Debug.WriteLine("캘리브레이션 완료!");
            Debug.WriteLine(CameraMatrix.Dump());
            Debug.WriteLine(DistCoeffs.Dump());

            return true;
        }
    }
}
