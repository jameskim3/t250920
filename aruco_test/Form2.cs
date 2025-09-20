using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Size = System.Drawing.Size;

namespace aruco_test
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.Load += Form2_Load;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            int rows = 6;       // 내부 코너 수 (가로)
            int cols = 9;       // 내부 코너 수 (세로)
            int squareSize = 50; // 한 칸 크기 (픽셀)

            int width = (cols + 1) * squareSize;
            int height = (rows + 1) * squareSize;

            using (Mat board = new Mat(new OpenCvSharp.Size(width, height), MatType.CV_8UC1, Scalar.White))
            {
                for (int i = 0; i <= rows; i++)
                {
                    for (int j = 0; j <= cols; j++)
                    {
                        if ((i + j) % 2 == 0)
                        {
                            Cv2.Rectangle(board,
                                new OpenCvSharp.Point(j * squareSize, i * squareSize),
                                new OpenCvSharp.Point((j + 1) * squareSize, (i + 1) * squareSize),
                                Scalar.Black, -1);
                        }
                    }
                }

                Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(board);
                Cv2.ImWrite("calibration_chessboard.png", board);

                var pb = new PictureBox
                {
                    Image = bmp,
                    Size = new Size(width, height),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BorderStyle = BorderStyle.FixedSingle,
                    Dock = DockStyle.Fill
                };

                this.Controls.Add(pb);
                this.ClientSize = new Size(width, height);
            }
        }
    }
}
