using System.Windows;
using System.Windows.Input;

namespace mV3DViewer
{
    /// <summary>
    /// Interaction logic for Main_Settings.xaml
    /// </summary>
    public partial class Main_Settings : Window
    {
        public Main_Settings() {
            InitializeComponent();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e) {
            Close();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void minBtn_Click(object sender, RoutedEventArgs e) {
            WindowState = WindowState.Minimized;
        }
    }
}
