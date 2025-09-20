using OpenCvSharp.Aruco;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aruco_test
{
    public partial class Form1 : Form
    {
        private const int MarkerSize = 200;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var dict = CvAruco.GetPredefinedDictionary(PredefinedDictionaryName.Dict4X4_50);
            int columns = 3;
            int rows = 3;

            this.ClientSize = new System.Drawing.Size(columns * (MarkerSize + 10) + 10, rows * (MarkerSize + 10) + 10);
            this.Text = "Aruco Markers 0~8";

            for (int i = 0; i < 9; i++)
            {
                int row = i / columns;
                int col = i % columns;

                using (var mat = new Mat())
                {
                    dict.GenerateImageMarker(i, MarkerSize, mat, 1);
                    Bitmap bmp = MatToBitmap(mat);

                    var pb = new PictureBox
                    {
                        Image = bmp,
                        Size = new System.Drawing.Size(MarkerSize, MarkerSize),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        BorderStyle = BorderStyle.FixedSingle,
                        Location = new System.Drawing.Point(10 + col * (MarkerSize + 10), 10 + row * (MarkerSize + 10))
                    };

                    this.Controls.Add(pb);
                }
            }
        }

        private Bitmap MatToBitmap(Mat mat)
        {
            PixelFormat format = mat.Type() == MatType.CV_8UC1
                ? PixelFormat.Format8bppIndexed
                : PixelFormat.Format24bppRgb;

            Bitmap bitmap = new Bitmap(mat.Width, mat.Height, format);
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, mat.Width, mat.Height),
                ImageLockMode.WriteOnly, format);

            mat.GetArray(out byte[] bytes);
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            bitmap.UnlockBits(data);

            if (format == PixelFormat.Format8bppIndexed)
            {
                ColorPalette palette = bitmap.Palette;
                for (int i = 0; i < 256; i++)
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                bitmap.Palette = palette;
            }

            return bitmap;
        }
    }
}
