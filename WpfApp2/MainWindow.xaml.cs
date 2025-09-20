using System.Windows;

namespace WpfApp2
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnPage1_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new Page1();
        }

        private void BtnPage2_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new Page2();
        }

        private void BtnPage3_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new Page3();
        }

        private void BtnPage4_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new Page4();
        }

        private void BtnPage5_Click(object sender, RoutedEventArgs e)
        {
            ContentArea.Content = new Page5();
        }
    }
}
