using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aruco_test
{
    using OpenCvSharp;
    using OpenCvSharp.Aruco;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;

    public static class MatConverter
    {
        public static Bitmap MatToBitmap(Mat mat)
        {
            if (mat.Type() != MatType.CV_8UC1 && mat.Type() != MatType.CV_8UC3)
                throw new NotSupportedException("Only 8UC1 and 8UC3 supported");

            PixelFormat format = mat.Type() == MatType.CV_8UC1 ? PixelFormat.Format8bppIndexed : PixelFormat.Format24bppRgb;
            Bitmap bmp = new Bitmap(mat.Width, mat.Height, format);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, mat.Width, mat.Height), ImageLockMode.WriteOnly, format);
            mat.GetArray(out byte[] bytes);
            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, data.Scan0, bytes.Length);
            bmp.UnlockBits(data);

            if (mat.Type() == MatType.CV_8UC1)
            {
                ColorPalette palette = bmp.Palette;
                for (int i = 0; i < 256; i++)
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                bmp.Palette = palette;
            }

            return bmp;
        }
    }

    public class ArucoMarkerGenerator
    {
        private Dictionary _dictionary;
        private int _markerSize;

        public ArucoMarkerGenerator(PredefinedDictionaryName dictName = PredefinedDictionaryName.Dict4X4_50, int size = 150)
        {
            _dictionary = CvAruco.GetPredefinedDictionary(dictName);
            _markerSize = size;
        }

        public Bitmap GenerateMarkerBitmap(int id)
        {
            using (var mat = new Mat())
            {
                _dictionary.GenerateImageMarker(id, _markerSize, mat, 1);
                return MatConverter.MatToBitmap(mat);
            }
        }
    }
}
