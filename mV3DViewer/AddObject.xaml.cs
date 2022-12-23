using HelixToolkit.Wpf;
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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace mV3DViewer
{
    /// <summary>
    /// Interaction logic for AddObject.xaml
    /// </summary>
    public partial class AddObject : Window
    {
        #region
        GridLinesVisual3D d = new GridLinesVisual3D();
        public CubeVisual3D cube;
        public SphereVisual3D f;
        public EllipsoidVisual3D ellip;
        public TruncatedConeVisual3D cyn;
        public TruncatedConeVisual3D cone;
        public TorusVisual3D t;
        public TextVisual3D txt;
        public List<object> meshes = new List<object>();
        public string title;
        #endregion
        public AddObject()
        {
            InitializeComponent();
            MeshViewer.Children.Add(d);
        }
        public void ShowCorrectMesh(string mesh)
        {
            switch (mesh)
            {
                case "Cube":
                    cube = new CubeVisual3D
                    {
                        SideLength = 10,
                        Center = new Point3D(0, 1, 0),
                        Fill = Brushes.Blue
                    }; ;
                    title = "Cube";
                    objTitle.Text = title;
                    MeshViewer.Children.Add(cube);
                    List<CubeVisual3D> cb = new List<CubeVisual3D>
                    {
                        cube
                    };
                    propertyMenu.ItemsSource = cb;
                    break;
                case "Sphere":
                    f = new SphereVisual3D
                    {
                        Radius = Convert.ToDouble(10),
                        Center = new Point3D(0, 1, 0),
                        Fill = Brushes.Red
                    };
                    title = "Sphere";
                    objTitle.Text = title;
                    MeshViewer.Children.Add(f);
                    List<SphereVisual3D> sp = new List<SphereVisual3D>
                    {
                        f
                    };
                    propertyMenu.ItemsSource = sp;
                    break;
                case "Ellipsoid":
                    ellip = new EllipsoidVisual3D
                    {
                        PhiDiv = 21,
                        RadiusX = 3,
                        RadiusY = 6,
                        RadiusZ = 5,
                        Center = new Point3D(0, 1, 0),
                        Fill = Brushes.Aquamarine
                    };
                    title = "Ellipsoid";
                    objTitle.Text = title;
                    MeshViewer.Children.Add(ellip);
                    List<EllipsoidVisual3D> el = new List<EllipsoidVisual3D>
                    {
                        ellip
                    };
                    propertyMenu.ItemsSource = el;
                    break;
                case "Cylinder":
                    cyn = new TruncatedConeVisual3D
                    {
                        TopRadius = 3,
                        BaseRadius = 3,
                        Height = 10,
                        Origin = new Point3D(0, 1, 0),
                        Fill = Brushes.Green
                    };
                    title = "Cylinder";
                    objTitle.Text = title;
                    MeshViewer.Children.Add(cyn);
                    List<TruncatedConeVisual3D> cy = new List<TruncatedConeVisual3D>
                    {
                        cyn
                    };
                    propertyMenu.ItemsSource = cy;
                    break;
                case "Cone":
                    cone = new TruncatedConeVisual3D
                    {
                        TopRadius = 0,
                        BaseRadius = 3,
                        Height = 10,
                        Origin = new Point3D(0, 1, 0),
                        Fill = Brushes.Yellow
                    };
                    title = "Cone";
                    objTitle.Text = title;
                    MeshViewer.Children.Add(cone);
                    List<TruncatedConeVisual3D> co = new List<TruncatedConeVisual3D>
                    {
                        cone
                    };
                    propertyMenu.ItemsSource = co;
                    break;
                case "Torus":
                    t = new TorusVisual3D
                    {
                        TubeDiameter = 3,
                        TorusDiameter = 10,
                        ThetaDiv = 36,
                        Fill = Brushes.Purple,
                        Transform = new TranslateTransform3D(0, 1, 0)
                    };
                    title = "Torus";
                    objTitle.Text = title;
                    MeshViewer.Children.Add(t);
                    List<TorusVisual3D> to = new List<TorusVisual3D>
                    {
                        t
                    };
                    propertyMenu.ItemsSource = to;
                    break;
                case "Text":
                    txt = new TextVisual3D()
                    {
                        Text = "text",
                        Position = new Point3D(0, 1, 0),
                        Height = 1,
                        Background = Brushes.Transparent,
                        FontWeight = FontWeights.Bold
                    };
                    title = "Text";
                    objTitle.Text = title;
                    MeshViewer.Children.Add(txt);
                    List<TextVisual3D> tx = new List<TextVisual3D>
                    {
                        txt
                    };
                    propertyMenu.ItemsSource = tx;
                    break;
            }
            xPosTxt.Text = 0.ToString();
            yPosTxt.Text = 0.ToString();
            zPosTxt.Text = 0.ToString();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(meshName.Text))
            {
                object n = null;
                if (cube != null)
                {
                    MeshViewer.Children.Remove(cube);
                    n = cube;
                    cube.SetName(meshName.Text);
                }
                else if (f != null)
                {
                    MeshViewer.Children.Remove(f);
                    n = f;
                    f.SetName(meshName.Text);
                }
                else if (ellip != null)
                {
                    MeshViewer.Children.Remove(ellip);
                    n = ellip;
                    ellip.SetName(meshName.Text);
                }
                else if (cyn != null)
                {
                    MeshViewer.Children.Remove(cyn);
                    n = cyn;
                    cyn.SetName(meshName.Text);
                }
                else if (cone != null)
                {
                    MeshViewer.Children.Remove(cone);
                    n = cone;
                    cone.SetName(meshName.Text);
                }
                else if (t != null)
                {
                    MeshViewer.Children.Remove(t);
                    n = t;
                    t.SetName(meshName.Text);
                }
                else if (txt != null)
                {
                    MeshViewer.Children.Remove(txt);
                    n = txt;
                    txt.SetName(meshName.Text);
                }
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                        (window as MainWindow).viewPort3d.Children.Add((Visual3D)n);
                    this.Close();
                }
            }
            else
            {
                mV3DDialog b = new mV3DDialog();
                b.Show();
                System.Media.SystemSounds.Hand.Play();
                b.SetTitle("Error with adding object");
                b.SetContent("Error: Please add a valid name for the object");
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e) => Close();
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private Point3D SetPosTXT()
        {
            try
            {
                if (String.IsNullOrEmpty(xPosTxt.Text))
                    return new Point3D(0, Convert.ToDouble(yPosTxt.Text), Convert.ToDouble(zPosTxt.Text));
                if (String.IsNullOrEmpty(yPosTxt.Text))
                    return new Point3D(Convert.ToDouble(xPosTxt.Text), 0, Convert.ToDouble(zPosTxt.Text));
                if (String.IsNullOrEmpty(zPosTxt.Text))
                    return new Point3D(Convert.ToDouble(xPosTxt.Text), Convert.ToDouble(yPosTxt.Text), 0);
                return new Point3D(Convert.ToDouble(xPosTxt.Text), Convert.ToDouble(yPosTxt.Text), Convert.ToDouble(zPosTxt.Text));
            }
            catch
            {
                return new Point3D(0, 0, 0);
            }
        }
        private TranslateTransform3D SetTransfTXT()
        {
            try
            {
                if (String.IsNullOrEmpty(xPosTxt.Text))
                    return new TranslateTransform3D(0, Convert.ToDouble(yPosTxt.Text), Convert.ToDouble(zPosTxt.Text));
                if (String.IsNullOrEmpty(yPosTxt.Text))
                    return new TranslateTransform3D(Convert.ToDouble(xPosTxt.Text), 0, Convert.ToDouble(zPosTxt.Text));
                if (String.IsNullOrEmpty(zPosTxt.Text))
                    return new TranslateTransform3D(Convert.ToDouble(xPosTxt.Text), Convert.ToDouble(yPosTxt.Text), 0);
                return new TranslateTransform3D(Convert.ToDouble(xPosTxt.Text), Convert.ToDouble(yPosTxt.Text), Convert.ToDouble(zPosTxt.Text));
            }
            catch
            {
                return new TranslateTransform3D(0, 0, 0);
            }
        }
        private void ChangePosition()
        {
            if (cube != null)
            {
                cube.Center = SetPosTXT();
            }
            else if (f != null)
            {
                f.Center = SetPosTXT();
            }
            else if (ellip != null)
            {
                ellip.Center = SetPosTXT();
            }
            else if (cyn != null)
            {
                cyn.Origin = SetPosTXT();
            }
            else if (cone != null)
            {
                cone.Origin = SetPosTXT();
            }
            else if (t != null)
            {
                t.Transform = SetTransfTXT();
            }
            else if (txt != null)
            {
                txt.Position = SetPosTXT();
            }
        }
        private void xPosTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChangePosition();
        }
        private void meshName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                objTitle.Text = title + " - " + meshName.Text;
            });
        }
        private void meshName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                addToSceneBtn.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
        private void zPosTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(zPosTxt.Text))
                {
                    zPosTxt.Text = (e.Key == Key.Up) ? (Convert.ToDouble(zPosTxt.Text) + 1).ToString() : (e.Key == Key.Down) ? (Convert.ToDouble(zPosTxt.Text) - 1).ToString() : zPosTxt.Text;
                    zPosTxt.CaretIndex = zPosTxt.Text.Length;
                }
            }
            catch {}
        }
        private void yPosTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(yPosTxt.Text))
                {
                    yPosTxt.Text = (e.Key == Key.Up) ? (Convert.ToDouble(yPosTxt.Text) + 1).ToString() : (e.Key == Key.Down) ? (Convert.ToDouble(yPosTxt.Text) - 1).ToString() : yPosTxt.Text;
                    yPosTxt.CaretIndex = yPosTxt.Text.Length;
                }
            }
            catch {}
        }
        private void xPosTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(xPosTxt.Text))
                {
                    xPosTxt.Text = (e.Key == Key.Up) ? (Convert.ToDouble(xPosTxt.Text) + 1).ToString() : (e.Key == Key.Down) ? (Convert.ToDouble(xPosTxt.Text) - 1).ToString() : xPosTxt.Text;
                    xPosTxt.CaretIndex = xPosTxt.Text.Length;
                }
            }
            catch {}
        }
    }
}
