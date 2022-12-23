using AutoMapper;
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
    /// Interaction logic for CutMesh.xaml
    /// </summary>
    public partial class CutMesh : Window
    {
        #region
        GridLinesVisual3D gl = new GridLinesVisual3D();
        public ModelVisual3D currentObj;
        public List<Plane3D> planes = new List<Plane3D>();
        public CuttingPlaneGroup cg = new CuttingPlaneGroup();
        #endregion
        public CutMesh()
        {
            InitializeComponent();
            MeshViewer.Children.Add(gl);
            CopyMesh();
        }
        private void CopyMesh()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    MainWindow mw = window as MainWindow;
                    var config = new MapperConfiguration(cfg => cfg.CreateMap<ModelVisual3D, ModelVisual3D>());
                    var mapper = new Mapper(config);
                    ModelVisual3D newModel = mapper.Map<ModelVisual3D>(mw.SelectedObject);
                    meshName.Text = $"Cut Mesh - {(mw.SelectedObject as ModelVisual3D).GetName()}";
                    currentObj = newModel;
                    MeshViewer.Children.Add(newModel);
                }
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void addToSceneBtn_Click(object sender, RoutedEventArgs e)
        {

        }
        private void cutNorm_Click(object sender, RoutedEventArgs e)
        {
            MeshViewer.Children.Remove(currentObj);
            cg.Children.Remove(currentObj);
            string[] eachLine = normalMap.Text.Split('\n');
            for (int i = 0; i < eachLine.Length; i++)
            {
                Plane3D a = new Plane3D();
                double[] paramaters = Array.ConvertAll(eachLine[i].Split(','), Double.Parse);
                a.Normal = new Vector3D(paramaters[0], paramaters[1], paramaters[2]);
                planes.Add(a);
            }
            cg.CuttingPlanes = planes;
            cg.Children.Add(currentObj);
            Close();
        }
        private void meshName_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
