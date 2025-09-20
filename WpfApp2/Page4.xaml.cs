using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.MessageBox;

namespace WpfApp2
{
    public partial class Page4 : System.Windows.Controls.UserControl
    {
        public Page4()
        {
            InitializeComponent();
        }

        private void AugmentAndSave_Click(object sender, RoutedEventArgs e)
        {
            string inputFolder = @"c:\img2";
            string outputFolder = Path.Combine(inputFolder, "augmented");
            Directory.CreateDirectory(outputFolder);

            foreach (var file in Directory.GetFiles(inputFolder, "*.bmp"))
            {
                BitmapImage src = new BitmapImage(new Uri(file));

                for (double angle = 0.0; angle <= 5.0; angle += 0.1)
                {
                    // RenderTargetBitmap을 사용해 새로운 비트맵 생성
                    int w = src.PixelWidth;
                    int h = src.PixelHeight;
                    var rtb = new RenderTargetBitmap(w, h, src.DpiX, src.DpiY, PixelFormats.Pbgra32);

                    // DrawingVisual에 회전 적용 후 그림
                    var dv = new DrawingVisual();
                    using (var dc = dv.RenderOpen())
                    {
                        dc.PushTransform(new RotateTransform(angle, w / 2.0, h / 2.0));
                        dc.DrawImage(src, new Rect(0, 0, w, h));
                        dc.Pop();
                    }

                    rtb.Render(dv);

                    string name = Path.GetFileNameWithoutExtension(file);
                    string outPath = Path.Combine(outputFolder, $"{name}_a{angle:F1}.bmp");

                    SaveBitmap(rtb, outPath);
                }
            }

            MessageBox.Show("0~5도 (0.1 간격) 회전 이미지 저장 완료!");
        }

        private void SaveBitmap(BitmapSource src, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                BmpBitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(src));
                encoder.Save(fs);
            }
        }
    }
}

