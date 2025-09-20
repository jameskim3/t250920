using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using myPen= System.Windows.Media.Pen;
using myBrush= System.Windows.Media.Brush;
using myPoint= System.Windows.Point;
namespace WpfApp2
{
    public partial class Page2 : System.Windows.Controls.UserControl
    {
        private readonly DrawingVisual _overlay = new DrawingVisual();
        private BitmapSource _src;
        private List<myPoint> _crossCenters = new();

        public Page2()
        {
            InitializeComponent();
            OverlayHost.Visual = _overlay;
            LoadFileList(@"c:\img\gray");
        }
        private void BtnConvertGray_Click(object sender, RoutedEventArgs e)
        {
            string folder = @"c:\img";
            if (!Directory.Exists(folder))
            {
                System.Windows.MessageBox.Show(@"폴더가 없습니다: c:\img");
                return;
            }

            try
            {
                int count = ConvertJpgsToGray8Bmp(folder);
                System.Windows.MessageBox.Show($"변환 완료: {count}개 파일", "JPG → 8-bit Gray");
                LoadFileList(folder); // 리스트 갱신 (bmp도 보이게)
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("변환 중 오류: " + ex.Message);
            }
        }
        private int ConvertJpgsToGray8Bmp(string folder)
        {
            string[] jpgs = Directory.GetFiles(folder, "*.jpg")
                                     .Concat(Directory.GetFiles(folder, "*.jpeg"))
                                     .ToArray();
            if (jpgs.Length == 0) return 0;

            string outDir = Path.Combine(folder, "gray");
            Directory.CreateDirectory(outDir);

            int ok = 0;
            foreach (var path in jpgs)
            {
                try
                {
                    // 1) 원본 로드
                    var src = new BitmapImage();
                    src.BeginInit();
                    src.UriSource = new Uri(path, UriKind.Absolute);
                    src.CacheOption = BitmapCacheOption.OnLoad; // 파일 잠금 방지
                    src.EndInit();
                    src.Freeze();

                    // 2) Gray8 변환 (WPF 네이티브)
                    BitmapSource gray = new FormatConvertedBitmap(src, PixelFormats.Gray8, null, 0);
                    gray.Freeze();

                    // 3) BMP로 저장 (8bpp 인덱스 팔레트 유지)
                    string name = Path.GetFileNameWithoutExtension(path);
                    string outPath = Path.Combine(outDir, $"{name}_gray.bmp");
                    using (var fs = new FileStream(outPath, FileMode.Create))
                    {
                        var enc = new BmpBitmapEncoder();
                        enc.Frames.Add(BitmapFrame.Create(gray));
                        enc.Save(fs);
                    }
                    ok++;
                }
                catch
                {
                    // 개별 파일 오류는 스킵하고 계속
                }
            }
            return ok;
        }

        private void Page2_Loaded(object sender, RoutedEventArgs e)
        {
            OverlayHost.Visual = _overlay;
            RedrawOverlay();
        }

        private void LoadFileList(string folder)
        {
            if (!Directory.Exists(folder)) return;
            string[] files = Directory.GetFiles(folder, "*.bmp");
            foreach (var f in files)
            {
                FileList.Items.Add(f);
            }
        }

        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileList.SelectedItem is string path)
            {
                try
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(path, UriKind.Absolute);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    bi.Freeze();

                    _src = bi;
                    PlayImage.Source = _src;

                    RedrawOverlay();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("이미지 로드 실패: " + ex.Message);
                }
            }
        }

        private void RedrawOverlay()
        {
            if (_src == null) return;

            _crossCenters.Clear();

            using (var dc = _overlay.RenderOpen())
            {
                var penRect = new myPen(System.Windows.Media.Brushes.Lime, 2); penRect.Freeze();
                var penCross = new myPen(System.Windows.Media.Brushes.Red, 2); penCross.Freeze();
                var textBrush = System.Windows.Media.Brushes.White;
                var typeface = new Typeface(new System.Windows.Media.FontFamily("Segoe UI"),
                                            FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);

                // 사각형 2개 (예시 좌표)
                Rect r1 = new Rect(80, 60, 140, 100);
                Rect r2 = new Rect(360, 180, 140, 100);

                DrawRectWithCrossAndLabel(dc, r1, penRect, penCross, textBrush, typeface);
                DrawRectWithCrossAndLabel(dc, r2, penRect, penCross, textBrush, typeface);
            }
            OverlayHost.Visual = _overlay; // 매번 보장
        }

        private void DrawRectWithCrossAndLabel(
          System.Windows.Media.DrawingContext dc,
          System.Windows.Rect r,
          System.Windows.Media.Pen rectPen,
          System.Windows.Media.Pen crossPen,
          System.Windows.Media.Brush textBrush,
          System.Windows.Media.Typeface typeface)
        {
            // 1) 사각형
            dc.DrawRectangle(null, rectPen, r);

            // 2) 사각형 중심 계산
            double cx = r.X + r.Width / 2.0;
            double cy = r.Y + r.Height / 2.0;

            // 3) 중심 크로스
            double size = Math.Min(r.Width, r.Height) * 0.25;
            dc.DrawLine(crossPen, new System.Windows.Point(cx - size, cy), new System.Windows.Point(cx + size, cy));
            dc.DrawLine(crossPen, new System.Windows.Point(cx, cy - size), new System.Windows.Point(cx, cy + size));

            // 4) 텍스트 (사각형 위 중앙에 표시)
            string label = $"({(int)System.Math.Round(cx)},{(int)System.Math.Round(cy)})";
            var ft = new System.Windows.Media.FormattedText(
                label,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Windows.FlowDirection.LeftToRight,   // ✅ enum은 형식 이름으로 직접 접근
                typeface,
                14,
                textBrush,
                1.25);

            double tx = r.X + (r.Width - ft.Width) / 2.0;
            double ty = r.Y - ft.Height - 4;
            if (ty < 0) ty = 0;

            dc.DrawText(ft, new System.Windows.Point(tx, ty));
        }
        private void PlayImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_src == null) return;

            // 마우스 위치 (Image 컨트롤 좌표계)
            var pos = e.GetPosition(PlayImage);

            // 이미지 크기
            double imgW = _src.PixelWidth;
            double imgH = _src.PixelHeight;

            // 실제 컨트롤 크기
            double ctrlW = PlayImage.ActualWidth;
            double ctrlH = PlayImage.ActualHeight;

            // Stretch=None 이므로, 이미지가 중앙 정렬된 상태
            double offsetX = (ctrlW - imgW) / 2;
            double offsetY = (ctrlH - imgH) / 2;

            double x = pos.X - offsetX;
            double y = pos.Y - offsetY;

            // 이미지 내부인지 확인
            if (x >= 0 && y >= 0 && x < imgW && y < imgH)
            {
                // 픽셀 좌표로 변환 (정수 반올림)
                int px = (int)Math.Round(x);
                int py = (int)Math.Round(y);

                // 메인창 제목 변경
                var mw = Window.GetWindow(this) as MainWindow;
                if (mw != null)
                {
                    mw.Title = $"WpfApp2 - X:{px}, Y:{py}";
                }
            }
        }
    }

    /// <summary>
    /// DrawingVisual 오버레이 호스트
    /// </summary>
    public sealed class DrawingVisualHost : FrameworkElement
    {
        private readonly VisualCollection _children;

        public DrawingVisualHost()
        {
            _children = new VisualCollection(this);
        }

        public Visual Visual
        {
            get => _children.Count > 0 ? _children[0] : null;
            set
            {
                _children.Clear();
                if (value != null) _children.Add(value);
            }
        }

        protected override int VisualChildrenCount => _children.Count;
        protected override Visual GetVisualChild(int index) => _children[index];
    }

}
