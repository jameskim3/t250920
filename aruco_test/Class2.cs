using OpenCvSharp;
using System;
using System.IO;

public class DistortedChessboardGenerator
{
    private int cols = 9, rows = 6;
    private int squareSize = 50;
    private int boardWidth, boardHeight;
    private string folder = "img2";

    private int imageIndex = 1;

    public DistortedChessboardGenerator()
    {
        boardWidth = (cols + 1) * squareSize;
        boardHeight = (rows + 1) * squareSize;
        Directory.CreateDirectory(folder);
    }

    public Mat GenerateAndSaveOne()
    {
        var baseBoard = GenerateBaseBoard();
        Cv2.ImWrite(Path.Combine(folder, "base.png"), baseBoard);

        var distorted = ApplyRandomPerspective(baseBoard, DateTime.Now.Millisecond);

        // 저장 경로 생성
        string filename = $"distorted_{imageIndex:000}.png";
        string fullPath = Path.Combine(folder, filename);
        Cv2.ImWrite(fullPath, distorted);
        Console.WriteLine("Saved to: " + fullPath);

        imageIndex++;
        return distorted;
    }

    private Mat GenerateBaseBoard()
    {
        var mat = new Mat(new OpenCvSharp.Size(boardWidth, boardHeight), MatType.CV_8UC1, Scalar.White);
        for (int i = 0; i <= rows; i++)
        {
            for (int j = 0; j <= cols; j++)
            {
                if ((i + j) % 2 == 0)
                {
                    Cv2.Rectangle(mat,
                        new OpenCvSharp.Point(j * squareSize, i * squareSize),
                        new OpenCvSharp.Point((j + 1) * squareSize, (i + 1) * squareSize),
                        Scalar.Black, -1);
                }
            }
        }
        return mat;
    }

    private Mat ApplyRandomPerspective(Mat input, int seed)
    {
        int margin = 50;
        Random rand = new Random(seed);

        Point2f[] src = new Point2f[]
        {
            new Point2f(0, 0),
            new Point2f(input.Cols - 1, 0),
            new Point2f(input.Cols - 1, input.Rows - 1),
            new Point2f(0, input.Rows - 1)
        };

        Point2f[] dst = new Point2f[4];
        for (int i = 0; i < 4; i++)
        {
            float dx = (float)(rand.NextDouble() * margin - margin / 2);
            float dy = (float)(rand.NextDouble() * margin - margin / 2);
            dst[i] = new Point2f(src[i].X + dx, src[i].Y + dy);
        }

        Mat H = Cv2.GetPerspectiveTransform(src, dst);
        Mat output = new Mat();
        Cv2.WarpPerspective(input, output, H, input.Size(), InterpolationFlags.Linear, BorderTypes.Constant, Scalar.White);
        return output;
    }
}
