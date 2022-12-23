using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace mV3DViewer
{
    /// <summary>
    /// Interaction logic for mV3DDialog.xaml
    /// </summary>
    public partial class mV3DDialog : Window
    {
        public mV3DDialog()
        {
            InitializeComponent();
        }
        public void SetTitle(string title)
        {
            TitleMV.Text = title;
        }
        public void SetContent(string content)
        {
            ContentMV.Text = content;
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState;
        }
    }
}
