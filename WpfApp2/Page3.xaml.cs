using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.PMAlign;

namespace WpfApp2
{
    public partial class Page3 : System.Windows.Controls.UserControl
    {
        private CogDisplay _cogDisplay;
        private CogToolBlockEditV2 _toolBlockEdit;
        private CogToolBlock _toolBlock;

        public Page3()
        {
            InitializeComponent();

            // CogDisplay 초기화
            _cogDisplay = new CogDisplay
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                BackColor = System.Drawing.Color.Black
            };
            WinFormsHostDisplay.Child = _cogDisplay;

            string tbPath = @"c:\tb\4.vpp";
            _toolBlock = File.Exists(tbPath)
                ? (CogToolBlock)CogSerializer.LoadObjectFromFile(tbPath)
                : new CogToolBlock();
            _toolBlockEdit = new CogToolBlockEditV2
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Subject = _toolBlock
            };
            WinFormsHostToolbox.Child = _toolBlockEdit;
            LoadFileList(@"c:\img\gray");
        }

        private void LoadFileList(string folder)
        {
            FileList.Items.Clear();
            if (!Directory.Exists(folder)) return;

            string[] files = Directory.GetFiles(folder, "*.bmp");
            foreach (var f in files)
            {
                FileList.Items.Add(f);
            }
        }
        private void FileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileList.SelectedItem is string path && File.Exists(path))
            {
                try
                {
                    CogImageFileTool ImageFileTool =new();
                    ImageFileTool.Operator.Open(path, CogImageFileModeConstants.Read);
                    ImageFileTool.Run();
                    _cogDisplay.Image = ImageFileTool.OutputImage;
                    _cogDisplay.Fit();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("이미지 로드 실패: " + ex.Message);
                }
            }
        }

    }
}
