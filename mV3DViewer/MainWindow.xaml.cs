using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using AutoMapper;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace mV3DViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region
        public string MODEL_PATH = null;
        public Point mouseLocation;
        GridLinesVisual3D d = new GridLinesVisual3D();
        DefaultLights light = new DefaultLights();
        private Dictionary<Model3D, string> Models =
        new Dictionary<Model3D, string>();
        bool checkTriangle = false;
        int sphereCount = 0;
        int cubeCount = 0;
        List<string> history = new List<string>();
        List<Tuple<string, int>> variable = new List<Tuple<string, int>>();
        int currCmd;
        string mv3dFile = null;
        string currentTexture = null;
        string historyList = "";
        public object SelectedObject = null;
        object SelectedMesh = null;
        private SurfacePlotModel viewmodel;
        bool maxmin = false;
        TextBoxHistory textboxHis = new TextBoxHistory();
        #endregion

        #region COMP2
        private static NReco.Linq.LambdaParser lambdaParser = new NReco.Linq.LambdaParser();
        private static Dictionary<string, object> varContext = new Dictionary<string, object>();
        #endregion

        #region CLOTH_PHYSICS
        public GeometryModel3D clothModel { get; set; }
        public Cloth cloth { get; private set; }
        private Stopwatch watch;
        private Thread integratorThread;
        #endregion
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            //this.WindowStyle = WindowStyle.None;
            ModelVisual3D device3D = new ModelVisual3D();
            device3D.Content = Display3d(MODEL_PATH);
            viewPort3d.Children.Add(device3D);
            viewPort3d.Children.Add(d);
            viewPort3d.Children.Add(light);

            viewPort3d.DefaultCamera = new PerspectiveCamera();
            viewPort3d.DefaultCamera.Position = new Point3D(0, 0, 0);
            viewPort3d.DefaultCamera.LookDirection = new Vector3D(-100, -100, -100);
            viewPort3d.DefaultCamera.UpDirection = new Vector3D(0, 0, 1);
            label.Content = "mV3DViewer - " + MODEL_PATH;
            if (MODEL_PATH != null)
            {
                FileInfo f = new FileInfo(MODEL_PATH);
                string fileNameinLbl = f.Name;
                fileNamelbl.Content = "Current file: " + fileNameinLbl;
            }

            textBox.Text += "\t--- Welcome to the mV3DViewer Console! Type 'help' to get started...---" + Environment.NewLine + Environment.NewLine + EntryPoint();
            plotTxtBox.Text += "> ";
            currCmd = history.Count;

            //3D Plotter
            viewmodel = new SurfacePlotModel();
            hViewport.DataContext = viewmodel;

            //TruncatedConeVisual3D cyl = new TruncatedConeVisual3D();
            //cyl.BaseRadius = 7;
            //cyl.TopRadius = 3;
            //cyl.Height = 10;
            //cyl.Origin = new Point3D(0, 0, 0);
            //cyl.Fill = Brushes.Yellow;
            //viewPort3d.Children.Add(cyl);

            //TorusVisual3D t = new TorusVisual3D();
            //t.TubeDiameter = 3;
            //t.TorusDiameter = 10;
            //t.ThetaDiv = 36;
            //t.Fill = Brushes.Purple;
            //t.Transform = new TranslateTransform3D(0, 0, 0);
            //viewPort3d.Children.Add(t);

            //EllipsoidVisual3D df = new EllipsoidVisual3D();
            //df.PhiDiv = 21;
            //df.RadiusX = 3;
            //df.RadiusY = 6;
            //df.RadiusZ = 5;
            //df.Fill = Brushes.Aquamarine;
            //viewPort3d.Children.Add(df);

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }
        private Model3D Display3d(string model)
        {
            Model3D device = null;
            try
            {
                //Adding a gesture here
                viewPort3d.RotateGesture = new MouseGesture(MouseAction.LeftClick);

                //Import 3D model file
                ModelImporter import = new ModelImporter();
                
                //Load the 3D model file
                device = import.Load(model);
            }
            catch (Exception e)
            {
                // Handle exception in case can not file 3D model
                SendTXTBoxMsg("Exception Error : " + e.StackTrace);
            }
            return device;
        }
        private void titleGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
            if (maxmin == true)
            {
                this.WindowState = WindowState.Normal;
                maxmin = false;
                maximumMin.Content = "🗖";
                DragMove();
            }
        } 
        private void button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private SolidColorBrush ColorSelector(string colorName)
        {
            switch (colorName)
            {
                case "red":
                    return Brushes.Red;
                case "blue":
                    return Brushes.Blue;
                case "yellow":
                    return Brushes.Yellow;
                default:
                    return Brushes.White;
            }
        }
        private async void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            string[] lines = textBox.Text.Split('\n');
            string lastLine = lines[lines.Length - 1];
            string[] command = lastLine.Split('>');
            try
            {
                string[] args = command[1].Split(' ');
                if (e.Key == Key.Enter)
                {
                    try
                    {
                        currCmd = history.Count;
                        if (args.Length <= 2)
                        {
                            if (String.IsNullOrWhiteSpace(command[1]))
                                BlankEntry(textBox);
                            else
                            {
                                string ah = command[1].TrimStart();
                                switch (ah.ToLower())
                                {
                                    case "help":
                                        history.Add(command[1]);
                                        Help();
                                        break;
                                    case "clear":
                                        history.Clear();
                                        historyList = "";
                                        textBox.Text = EntryPoint();
                                        textBox.SelectionStart = textBox.Text.Length;
                                        textBox.SelectionLength = 0;
                                        break;
                                    case "removemanipulator":
                                        history.Add(command[1]);
                                        ClearManipulator();
                                        SendTXTBoxMsg("Removed all selected manipulators...");
                                        break;
                                    case "history":
                                        for (int j = 0; j < history.Count; j++)
                                        {
                                            historyList += "\t" + (j + 1) + ". " + history[j] + Environment.NewLine;
                                        }
                                        SendTXTBoxMsg("---History List---" + Environment.NewLine + historyList);
                                        break;
                                    case "show_triangle":
                                        history.Add(command[1]);
                                        checkTriangle = checkTriangle ? checkTriangle = false : checkTriangle = true;
                                        SendTXTBoxMsg(!checkTriangle ? "Triangle print OFF" : "Triangle print ON");
                                        break;
                                    case "multiplex":
                                        history.Add(command[1]);
                                        string ThisDir = Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName;
                                        #if DEBUG
                                        SendTXTA("Initializing engine please wait...\nSetting new default environment");
                                        await Task.Run(() => 
                                        Process.Start($@"{ThisDir}\x64\Debug\MultiplexEngine.exe"));
                                        #endif
                                        SendTXTBoxMsg("Starting Multiplex Engine...");
                                        break;
                                    case "shaded":
                                        GLSLShading gLSLShading = new GLSLShading();
                                        gLSLShading.Show();
                                        SendTXTBoxMsg("Opening ShadeD compiler");
                                        break;
                                    case "exit":
                                    case "close":
                                        Close();
                                        break;
                                    default:
                                        if (!IsEmptyOrAllSpaces(command[1]))
                                            history.Add(command[1]);
                                        EmptyEntry();
                                        break;
                                }
                            }
                        }
                        else
                        {
                            if (String.IsNullOrWhiteSpace(command[1]))
                                BlankEntry(textBox);
                            else
                            {
                                if (command[1].ToLower().StartsWith(" sphere") == true)
                                {
                                    history.Add(command[1]);
                                    CreateBubble(args[2], args[3], args.Length < 5 ? "red" : args[4]);
                                }
                                else if (command[1].ToLower().StartsWith(" cube") == true)
                                {
                                    history.Add(command[1]);
                                    CreateCuboid(args[2], args[3], (args.Length < 5) ? "blue" : args[4]);
                                }
                                else if (command[1].ToLower().StartsWith(" cut_mesh") == true)
                                {
                                    history.Add(command[1]);
                                    if (SelectedObject != null)
                                    {
                                        double[] cutParams = Array.ConvertAll(args[2].Split(','), Double.Parse);
                                        MeshGeometry3D cutted = MeshGeometryHelper.Cut((MeshGeometry3D)SelectedMesh,
                                            new Point3D(cutParams[0], cutParams[1], cutParams[2]), new Vector3D(0, 0, 1));
                                        LinesVisual3D cuttedLines = new LinesVisual3D()
                                        {
                                            Thickness = Convert.ToDouble(args[3]),
                                            Points = cutted.Positions,
                                            Color = Color.FromRgb(0, 255, 255),
                                        };
                                        ModelVisual3D setCutname = SelectedObject as ModelVisual3D;
                                        cuttedLines.SetName(setCutname.GetName() + " (cut)");
                                        SendTXTBoxMsg($"Success! Created mesh: {setCutname.GetName() + " (cut)"}");
                                        viewPort3d.Children.Add(cuttedLines);
                                    }
                                    else
                                        SendTXTBoxMsg("Error: No mesh is selected...");
                                }
                                else if (command[1].ToLower().StartsWith(" gridlines") == true)
                                {
                                    history.Add(command[1]);
                                    GridLines(args[2]);
                                }
                                else if (command[1].ToLower().StartsWith(" text3d"))
                                {
                                    history.Add(command[1]);
                                    string txt = "";
                                    for (int i = 2; i < args.Length - 1; i++)
                                        txt += args[i] + " ";
                                    txt += args[args.Length - 1];
                                    CreateText(txt);
                                }
                                else if (command[1].ToLower().StartsWith(" triangle_position"))
                                {
                                    history.Add(command[1]);
                                    if (checkTriangle)
                                    {
                                        double x = Convert.ToDouble(args[2]);
                                        double y = Convert.ToDouble(args[3]);
                                        double z = Convert.ToDouble(args[4]);
                                        ManipulateTriangle(x, y, z);
                                        SendTXTBoxMsg(String.Format("Triangle manipulator set to {0}, {1}, {2}", x, y, z));
                                    }
                                    else
                                        SendTXTBoxMsg("Error: Please enter 'show_triangle' in order to set selected triangle position...");
                                }
                                else if (command[1].ToLower().StartsWith(" history") == true)
                                {
                                    if (Convert.ToInt32(args[2]) > 0 || Convert.ToInt32(args[2]) < history.Count)
                                        HistoryCmd(Convert.ToInt32(args[2]));
                                }
                                else if (command[1].ToLower().StartsWith(" plane_cut") == true)
                                {
                                    //a.Normal = new Vector3D(0,10,0);
                                    //a.Normal = new Vector3D(1,0,-0.5);
                                    history.Add(command[1]);
                                    List<Plane3D> planes = new List<Plane3D>();
                                    for (int i = 2; i < args.Length; i++)
                                    {
                                        Plane3D a = new Plane3D();
                                        double[] paramaters = Array.ConvertAll(args[i].Split(','), Double.Parse);
                                        a.Normal = new Vector3D(paramaters[0], paramaters[1], paramaters[2]);
                                        planes.Add(a);
                                    }
                                    CuttingPlaneGroup cg = new CuttingPlaneGroup();
                                    cg.CuttingPlanes = planes;
                                    //CuttingOperation mv = new CuttingOperation();
                                    //CuttingOperation.Subtract
                                    //cg.Operation

                                    if (SelectedObject != null)
                                    {
                                        Visual3D selecObj = SelectedObject as Visual3D;
                                        try
                                        {
                                            viewPort3d.Children.Remove(selecObj);
                                            cg.Children.Add(selecObj);
                                            SendTXTBoxMsg("Successfully cut geometry mesh!");
                                        }
                                        catch (Exception ex)
                                        {
                                            SendTXTBoxMsg(ex.ToString());
                                        }
                                        viewPort3d.Children.Add(cg);
                                    }
                                    else
                                        SendTXTBoxMsg("Error: Cannot cut mesh with null object...");

                                }
                                else if (command[1].ToLower().StartsWith(" cmd_color") == true)
                                {
                                    history.Add(command[1]);
                                    textBox.Foreground = ColorSelector(args[2]);
                                    SendTXTBoxMsg($"Success! Changed color to {args[2]}");
                                }
                                else if (command[1].ToLower().StartsWith(" plot3d") == true)
                                {
                                    Plotter3D(args[2]);
                                }
                                else if (command[1].ToLower().StartsWith(" background") == true)
                                {
                                    history.Add(command[1]);
                                    switch (args[2])
                                    {
                                        case "black":
                                            viewPort3d.Background = Brushes.Black;
                                            viewPort3d.InfoForeground = Brushes.White;
                                            lblSelected.Foreground = Brushes.White;
                                            break;
                                        case "red":
                                            viewPort3d.Background = Brushes.Red;
                                            viewPort3d.InfoForeground = Brushes.White;
                                            lblSelected.Foreground = Brushes.White;
                                            break;
                                        case "gray":
                                            viewPort3d.Background = new SolidColorBrush(Color.FromRgb(62,62,62));
                                            viewPort3d.InfoForeground = Brushes.White;
                                            lblSelected.Foreground = Brushes.White;
                                            break;
                                        case "default":
                                            viewPort3d.Background = Brushes.AntiqueWhite;
                                            viewPort3d.InfoForeground = Brushes.Black;
                                            lblSelected.Foreground = Brushes.Black;
                                            break;
                                    }
                                    SendTXTBoxMsg($"Changed colour to '{args[2]}'");
                                }
                                else if (command[1].ToLower().StartsWith(" shaded") == true){
                                    GLSLShading gLsl = new GLSLShading();
                                    if (args[2].ToLower() == "--gen-verts")
                                        gLsl.GenerateVertsTemplate();
                                    else if (args[2].ToLower() == "--no-verts")
                                        gLsl.vertexCode.Text = "";
                                    string Fx = "";
                                    for (int i = 3; i < args.Length; i++)
                                        Fx += args[i];
                                    gLsl.GetFileThroughMV3D(Fx);
                                    gLsl.Show();
                                    SendTXTBoxMsg(File.Exists(Fx) ? $"Successfully opened {Fx} in ShadeD" :
                                    "ShadeD Error: There was an error with opening the file. Check if it's a '.frag' file or if it has been removed");
                                }
                                else if (command[1].ToLower().StartsWith(" cloth") == true)
                                {
                                    history.Add(command[1]);
                                    if (args[2] == "--physics-on-thread") {
                                        CreateCloth(args.Length < 4 ? Materials.Orange : MaterialHelper.CreateImageMaterial(args[3]));
                                        watch = new Stopwatch();
                                        watch.Start();
                                        integratorThread = new Thread(IntegrationWorker);
                                        integratorThread.Start();
                                        CompositionTarget.Rendering += CompositionTarget_Rendering1;
                                        SendTXTBoxMsg("Successfully created physics world on thread!");
                                    } else if (args[2] == "--stop") {
                                        if (watch != null && integratorThread != null) {
                                            watch.Stop();
                                            integratorThread.Abort();
                                            SendTXTBoxMsg("Thread has been aborted and timer has been stopped...");
                                        }
                                    }
                                }
                                else
                                {
                                    if (!IsEmptyOrAllSpaces(command[1]))
                                        history.Add(command[1]);
                                    SendTXTBoxMsg("Error: Unknown command(s)...");
                                }
                            }
                        }
                    }
                    catch
                    {
                        if (!IsEmptyOrAllSpaces(command[1]))
                            history.Add(command[1]);
                        SendTXTBoxMsg("Error: Unknown command(s)...");
                    }
                }
                else if (e.Key == Key.F1)
                {
                    if (currCmd >= 0 && history.Count > 0)
                    {
                        ClearAndRefresh();
                        textBox.Text += history[currCmd];
                        textBox.SelectionStart = textBox.Text.Length;
                        textBox.SelectionLength = 0;
                        textBox.ScrollToEnd();
                        currCmd--;
                    }
                    textBox.SelectionStart = textBox.Text.Length;
                    textBox.SelectionLength = 0;
                }
                else if (e.Key == Key.F2)
                {
                    if (currCmd + 1 < history.Count && history.Count > 0)
                    {
                        ClearAndRefresh();
                        textBox.Text += history[currCmd + 1];
                        SendToEnd();
                        currCmd++;
                    }
                    else if (currCmd + 1 == history.Count)
                    {
                        ClearAndRefresh();
                        textBox.Text += " ";
                        SendToEnd();
                    }
                    textBox.SelectionStart = textBox.Text.Length;
                    textBox.SelectionLength = 0;
                }
            }
            catch
            {
                SendTXTBoxMsg("Error: There was an error with parsing the command...");
            }
        }
        //CLOTH PHYSICS
        private void CompositionTarget_Rendering1(object sender, EventArgs e)
        {
            cloth.Transfer();
        }
        private void IntegrationWorker()
        {
            while (true)
            {
                double dt = 1.0 * watch.ElapsedTicks / Stopwatch.Frequency;
                watch.Restart();
                cloth.Update(dt);
            }
        }
        public void CreateCloth(Material pathToImage)
        {
            cloth = new Cloth(pathToImage);
            cloth.Init();
            MeshGeometryVisual3D msh = new MeshGeometryVisual3D()
            {
                MeshGeometry = cloth.Mesh,
                Material = cloth.Material,
                BackMaterial = cloth.Material
            };
            viewPort3d.Children.Add(msh);
        }
        public void SendToEnd()
        {
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;
            textBox.ScrollToEnd();
        }
        public bool IsEmptyOrAllSpaces(string str)
        {
            return null != str && str.All(c => c.Equals(' '));
        }
        private void ClearAndRefresh()
        {
            string k = textBox.Text;
            textBox.Clear();
            string[] lines = k.Split('\n');
            for (int i = 0; i < lines.Length - 2; i++)
            {
                textBox.Text += lines[i] + "\n";
            }
            textBox.Text += Environment.NewLine + EntryPointNoSpace();
        }
        public string EntryPoint()
        {
            if (MODEL_PATH != null)
                return MODEL_PATH + "> ";
            return "> ";
        }
        public void ManipulateTriangle(double x, double y, double z)
        {
            TriangleManipulator.x = x; TriangleManipulator.y = y; TriangleManipulator.z = z;
        }
        private void HistoryCmd(int arg)
        {
            ClearAndRefresh();
            textBox.Text += history[arg - 1];
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;
            textBox.ScrollToEnd();
        }
        public string EntryPointNoSpace()
        {
            if (MODEL_PATH != null)
                return MODEL_PATH + ">";
            return ">";
        }
        public void SendTXTBoxMsg(string msg)
        {
            textBox.Text += Environment.NewLine;
            textBox.Text += "\r" + msg + "\r\n" + Environment.NewLine;
            textBox.Text += EntryPoint();
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;
            textBox.ScrollToEnd();
        }
        public void SendTXTA(string msg)
        {
            textBox.Text = textBox.Text + Environment.NewLine;
            textBox.Text += "\r" + msg + Environment.NewLine;
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;
            textBox.ScrollToEnd();
        }
        public void BlankEntry(TextBox txbox)
        {
            txbox.Text += Environment.NewLine + EntryPoint();
            txbox.SelectionStart = txbox.Text.Length;
            txbox.ScrollToEnd();
        }
        public void SendPlotTXTMsg(string msg)
        {
            plotTxtBox.Text = plotTxtBox.Text + Environment.NewLine;
            plotTxtBox.Text += "\r" + msg + "\r\n" + Environment.NewLine;
            plotTxtBox.Text += "> ";
            plotTxtBox.SelectionStart = plotTxtBox.Text.Length;
            plotTxtBox.SelectionLength = 0;
            plotTxtBox.ScrollToEnd();
        }
        private void EmptyEntry()
        {
            SendTXTBoxMsg("Invalid input(s)! Try typing 'help' for a list of commands...");
        }
        private void NewItemMake()
        {
            if (MODEL_PATH != null)
            {
                Clear();
                MODEL_PATH = null;
                label.Content = "mV3DViwer -";
                fileNamelbl.Content = "";
                SendTXTBoxMsg("View port has been cleared...");
            }
        }
        public void Plotter3D(string arg)
        {
            Func<double, double, double> function;
            switch (arg)
            {
                case "funnel":
                    function = (x, y) => -1 / (x * x + y * y);
                    viewmodel.PlotFunction(function, -1, 1);
                    SendTXTBoxMsg("3D Graph with function '" + arg + "' has been plotted");
                    SendPlotTXTMsg("Function => -1 / (x^2 + y^2)");
                    break;
                case "sinc":
                    function = (x, y) => 10 * Math.Sin(Math.Sqrt(x * x + y * y)) / Math.Sqrt(x * x + y * y);
                    viewmodel.PlotFunction(function, -10, 10);
                    SendTXTBoxMsg("3D Graph with function '" + arg + "' has been plotted");
                    SendPlotTXTMsg("Function => 10 * sin(sqrt(x^2+y^2)) / sqrt(x^2 + y^2)");
                    break;
                case "gaussian":
                    function = (x, y) => 5 * Math.Exp(-1 * Math.Pow(x, 2) / 4 - Math.Pow(y, 2) / 4) / (Math.Sqrt(2 * Math.PI));
                    viewmodel.PlotFunction(function, -10, 10);
                    SendTXTBoxMsg("3D Graph with function '" + arg + "' has been plotted");
                    SendPlotTXTMsg("Function => 5 * e^(-1 * x^2 / 4 - y^2 / 4) / sqrt(2 * pi)");
                    break;
                case "ripple":
                    function = (x, y) => 0.25 * Math.Sin(Math.PI * Math.PI * x * y);
                    viewmodel.PlotFunction(function, 0, 2, 300);
                    SendTXTBoxMsg("3D Graph with function '" + arg + "' has been plotted");
                    SendPlotTXTMsg("Function => -0.25 * sin(pi^2 * x * y)");
                    break;
                default:
                    SendTXTBoxMsg("Error: " + arg + " is an unknown function name...");
                    break;
            }
        }
        public void CreateCuboid(string arg, string arg2, string arg3)
        {
            // CREATE CUBOID
            //cube.SideLength = 10;
            //cube.Fill = new SolidColorBrush(Colors.Red);
            //viewPort3d.Children.Add(cube);
            string[] r = arg.Split(' ');
            string[] ubi = arg2.Split(',');
            CubeVisual3D cube = new CubeVisual3D
            {
                SideLength = Convert.ToDouble(r[0]),
                Center = new Point3D(Convert.ToDouble(ubi[0]), Convert.ToDouble(ubi[1]), Convert.ToDouble(ubi[2]))
            };
            string nm = "cube" + cubeCount;
            cube.SetName(nm);

            if (!arg3.Contains("."))
            {
                switch (arg3)
                {
                    case "red":
                        cube.Fill = Brushes.Red;
                        break;
                    case "blue":
                        cube.Fill = Brushes.Blue;
                        break;
                    case "black":
                        cube.Fill = Brushes.Black;
                        break;
                    case "white":
                        cube.Fill = Brushes.White;
                        break;
                    case "green":
                        cube.Fill = Brushes.Green;
                        break;
                    case "yellow":
                        cube.Fill = Brushes.Yellow;
                        break;
                    case "cyan":
                        cube.Fill = Brushes.Cyan;
                        break;
                    case "lime":
                        cube.Fill = Brushes.Lime;
                        break;
                    default:
                        cube.Fill = Brushes.Red;
                        break;
                }
            }
            else
            {
                ImageBrush ib = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(arg3, UriKind.Relative))
                };
                cube.Fill = ib;
            }
            cubeCount++;
            viewPort3d.Children.Add(cube);
            SendTXTBoxMsg("Cube created...");
        }
        public void CreateBubble(string arg, string arg2, string arg3)
        {
            string[] r = arg.Split(' ');
            string[] ubi = arg2.Split(',');
            SphereVisual3D f = new SphereVisual3D
            {
                Radius = Convert.ToDouble(r[0]),
                Center = new Point3D(Convert.ToDouble(ubi[0]), Convert.ToDouble(ubi[1]), Convert.ToDouble(ubi[2]))
            };
            string nm = "sphere" + sphereCount;
            f.SetName(nm);

            if (!arg3.Contains("."))
            {
                switch (arg3)
                {
                    case "red":
                        f.Fill = Brushes.Red;
                        break;
                    case "blue":
                        f.Fill = Brushes.Blue;
                        break;
                    case "black":
                        f.Fill = Brushes.Black;
                        break;
                    case "white":
                        f.Fill = Brushes.White;
                        break;
                    case "green":
                        f.Fill = Brushes.Green;
                        break;
                    case "yellow":
                        f.Fill = Brushes.Yellow;
                        break;
                    case "cyan":
                        f.Fill = Brushes.Cyan;
                        break;
                    case "lime":
                        f.Fill = Brushes.Lime;
                        break;
                    default:
                        f.Fill = Brushes.Red;
                        break;
                }
            }
            else
            {
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(new Uri(arg3, UriKind.Relative));

                f.Fill = ib;
            }
            sphereCount++;
            viewPort3d.Children.Add(f);
            SendTXTBoxMsg("Created Sphere" + Environment.NewLine + "Radius: " + r[0] + Environment.NewLine + "X: " + ubi[0] + " Y: " + ubi[1] + " Z: " + ubi[2]);
        }
        public void Help()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("---List of Commands---");
            sb.AppendLine("(Press F1 to traverse up on command history and F2 to traverse down)");
            sb.AppendLine("\t-- help - lists all available commands");
            sb.AppendLine("\t-- sphere [radius] [x,y,z] [brush]* - creates sphere object");
            sb.AppendLine("\t-- cube [side-length] [x,y,z] [brush]* - creates cube object");
            sb.AppendLine("\t-- gridlines [on/off] - enables or disables view port gridlines");
            sb.AppendLine("\t-- show_triangle - prints vertices of selected triangle from mesh");
            sb.AppendLine("\t-- text3d [text] - creates 3D text object");
            sb.AppendLine("\t-- plane_cut [normals...] -- cut selected mesh with plane with specified normals");
            sb.AppendLine("\t-- cut_mesh [plane_x,plane_y,plane_z] [thickness]* -- creates mesh with specified plane");
            sb.AppendLine("\t-- removemanipulator -- removes all selected manipulators");
            sb.AppendLine("\t-- cmd_color [color] -- sets color of the CMD");
            sb.AppendLine("\t-- history [index]* -- lists command history");
            sb.AppendLine("\t-- cloth [--physics-on-thread/--remove] [texture] -- creates cloth with real time physics");
            sb.AppendLine("\t-- shaded [frag_file]* -- opens ShadeD, the GLSL compiler");
            sb.AppendLine("\t-- multiplex -- runs the Multiplex Engine, a game engine integrated with MV3D");
            SendTXTBoxMsg(sb.ToString());
        }
        public void GridLines(string arg)
        {
            switch (arg)
            {
                case "on":
                    viewPort3d.Children.Add(d);
                    SendTXTBoxMsg("Current visual grid lines added...");
                    break;
                case "off":
                    viewPort3d.Children.Remove(d);
                    SendTXTBoxMsg("Current visual grid lines removed...");
                    break;
            }
        }
        private void CreateText(string msg)
        {
            var txt = new TextVisual3D();
            txt.Text = msg;
            txt.Position = new Point3D(0, 0, 0.2 + 0.5);
            txt.Height = 1;
            txt.Background = Brushes.Transparent;
            txt.FontWeight = System.Windows.FontWeights.Bold;
            txt.SetName(msg + "__3DTXT");
            viewPort3d.Children.Add(txt);
            SendTXTBoxMsg("Created 3D Text: '" + msg + "'");
        }
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenNewModel();
        }
        private void ClearManipulator()
        {
            List<Visual3D> child = viewPort3d.Children.ToList();
            foreach (var m in child)
            {
                if (m is CombinedManipulator)
                {
                    viewPort3d.Children.Remove(m);
                }
            }
        }
        private void OnlyGetModel()
        {
            List<Visual3D> child = viewPort3d.Children.ToList();
            foreach (var m in child)
            {
                if (m is GridLinesVisual3D || m is CombinedManipulator)
                {
                    viewPort3d.Children.Remove(m);
                }
            }
        }
        private void Clear()
        {
            List<Visual3D> child = viewPort3d.Children.ToList();
            foreach (var m in child)
            {
                if (!(!(m is ModelVisual3D) || m is DefaultLights || m is GridLinesVisual3D))
                {
                    viewPort3d.Children.Remove(m);
                }
            }
        }
        private void viewPort3d_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Point mouse_pos = e.GetPosition(viewPort3d);
            // Perform the hit test.
            HitTestResult result =
                VisualTreeHelper.HitTest(viewPort3d, mouse_pos);

            //    Display information about the hit.
            //    Console.WriteLine("Distance: " +
            //    mesh_result.DistanceToRayOrigin);
            //    Console.WriteLine("Point hit: (" +
            //    mesh_result.PointHit.ToString() + ")");
            RayMeshGeometry3DHitTestResult mesh_result =
                result as RayMeshGeometry3DHitTestResult;

            CombinedManipulator k = new CombinedManipulator()
            {
                CanRotateX = true,
                CanRotateY = true,
                CanRotateZ = true
            };
            if (mesh_result == null) this.Title = "";
            else
            {
                if (checkTriangle)
                {
                    MeshGeometry3D trig = mesh_result.MeshHit;
                    SendTXTBoxMsg("Triangle: V1: " + trig.Positions[mesh_result.VertexIndex1].ToString() + ", V2: " + 
                        trig.Positions[mesh_result.VertexIndex2].ToString() + ", V3: " + trig.Positions[mesh_result.VertexIndex3].ToString());
                    //trig.Positions[mesh_result.VertexIndex3] = new Point3D(TriangleManipulator.x, TriangleManipulator.y, TriangleManipulator.z);
                    //trig.Positions[mesh_result.VertexIndex2] = new Point3D(TriangleManipulator.x, TriangleManipulator.y, TriangleManipulator.z);
                    //trig.Positions[mesh_result.VertexIndex1] = new Point3D(TriangleManipulator.x, TriangleManipulator.y, TriangleManipulator.z);
                }
                RayMeshGeometry3DHitTestResult rayMeshResult = mesh_result as RayMeshGeometry3DHitTestResult;
                if (rayMeshResult.VisualHit is ModelVisual3D && !(rayMeshResult.VisualHit is CombinedManipulator))
                {
                    string name = rayMeshResult.VisualHit.GetName();
                    lblSelected.Content = "Selected: " + name;
                    selectedItmLbl.Text = "Item: " + name;
                    string[] arrobj = rayMeshResult.VisualHit.GetType().ToString().Split('.');
                    Transform3D trns = rayMeshResult.VisualHit.Transform;
                    propertyGrid.SelectedObject = new ModelProperty()
                    {
                        Name = name,
                        Object = arrobj[arrobj.Length - 1],
                        Transform = rayMeshResult.VisualHit.GetViewportTransform(),
                        HasAnimatedProperties = rayMeshResult.VisualHit.HasAnimatedProperties.ToString(),
                        Offset = trns.Value.OffsetX + "," + trns.Value.OffsetY + "," + trns.Value.OffsetZ
                    };
                }
            }
            if (result != null && result.VisualHit is ModelVisual3D && !(result.VisualHit is GridLinesVisual3D))
            {
                ClearManipulator();
                RayMeshGeometry3DHitTestResult rayMeshResult = mesh_result;
                if (rayMeshResult.MeshHit is MeshGeometry3D)
                {
                    MeshGeometry3D mesh = (MeshGeometry3D)rayMeshResult.MeshHit;
                    k.Position = mesh.Positions[mesh.Positions.Count / 2];
                    k.Bind((ModelVisual3D)result.VisualHit);
                    SelectedObject = (ModelVisual3D)result.VisualHit;
                    SelectedMesh = mesh_result.MeshHit;
                    viewPort3d.Children.Add(k);
                }
            }
            else if (result.VisualHit is GridLinesVisual3D || result.VisualHit is CameraController) //result.VisualHit is GridLinesVisual3D
            {
                ClearManipulator();
                lblSelected.Content = "Selected: ";
                selectedItmLbl.Text = "No item selected";
                SelectedObject = null;
                SelectedMesh = null;
                propertyGrid.SelectedObject = null;
            }

            if (currentTexture != null)
            {
                RayMeshGeometry3DHitTestResult rayMeshResult = mesh_result as RayMeshGeometry3DHitTestResult;
                try
                {
                    if (rayMeshResult.VisualHit is SphereVisual3D)
                    {
                        SphereVisual3D h = rayMeshResult.VisualHit as SphereVisual3D;
                        ImageBrush ib = new ImageBrush();
                        ib.ImageSource = new BitmapImage(new Uri(currentTexture, UriKind.Relative));
                        h.Fill = ib;
                    }
                    else if (rayMeshResult.VisualHit is CubeVisual3D)
                    {
                        CubeVisual3D h = rayMeshResult.VisualHit as CubeVisual3D;
                        ImageBrush ib = new ImageBrush();
                        ib.ImageSource = new BitmapImage(new Uri(currentTexture, UriKind.Relative));
                        h.Fill = ib;
                    }
                    else
                        SendTXTBoxMsg("Error: Cannot set texture to null object...");
                }
                catch { }
            }
        }

        private void textBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            textBox.SelectionStart = textBox.Text.Length;
            textBox.SelectionLength = 0;
            textBox.ScrollToEnd();
        }

        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            tt_folder.Visibility = (tg_btn.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;
            tt_save.Visibility = (tg_btn.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;
            tt_saveas.Visibility = (tg_btn.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;
            tt_newfile.Visibility = (tg_btn.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;
            tt_settings.Visibility = (tg_btn.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;
            tt_cutmdl.Visibility = (tg_btn.IsChecked == true) ? Visibility.Collapsed : Visibility.Visible;
        }
        private void tg_btn_Unchecked(object sender, RoutedEventArgs e)
        {
            viewPort3d.Opacity = 1;
            textBox.Opacity = 1;
            dragableTab.Opacity = 1;
            fileNamelbl.Opacity = 1;
            textBox.IsReadOnly = false;
            //MessageBox.Show(viewPort3d.Opacity.ToString());
        }
        private void tg_btn_Checked(object sender, RoutedEventArgs e)
        {
            viewPort3d.Opacity = 0.3;
            textBox.Opacity = 0.3;
            dragableTab.Opacity = 0.3;
            fileNamelbl.Opacity = 0.3;
            textBox.IsReadOnly = true;
        }
        private void ListViewItem_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            NewItemMake();
        }
        private void codeExec_Click(object sender, RoutedEventArgs e)
        {
            variable.Clear();
            string[] eachLine = codeComp.Text.Split('\n');
            string output = "";
            for (int i = 0; i < eachLine.Length; i++)
            {
                string[] asd = codeComp.Text.Split(';');
                if (eachLine[i].StartsWith("print"))
                {
                    string[] r = eachLine[i].Split('"');
                    if (r.Length % 2 == 0)
                        output += Environment.NewLine + "Unhandled Exception: In line " + (i + 1) + " LINE: '" + eachLine[i] + "' -- Expected closing quotation";
                    else
                    {
                        try
                        {
                            string[] f = r[1].Split(' ');
                            string[] tfe = r[1].Split('%');
                            string[] pn = r[1].Split(new string[] { @"\n" }, StringSplitOptions.None);

                            if (tfe.Length > 1) //f[1].Contains('%')
                            {
                                for (int ok = 0; ok < variable.Count; ok++)
                                {
                                    if (tfe[1] == variable[ok].Item1)
                                        output += variable[ok].Item2.ToString();
                                }
                            }
                            else
                            {
                                //output += r[1];
                                for (int b = 0; b < pn.Length - 1; b++)
                                    output += pn[b] + Environment.NewLine;
                                output += pn[pn.Length - 1];
                            }
                        }
                        catch
                        {
                            output += Environment.NewLine + "Unhandled Exception: In line " + (i + 1) + " LINE: " + eachLine[i];
                        }
                    }
                }
                else if (eachLine[i].StartsWith("int"))
                {
                    string[] f = eachLine[i].Split('=');
                    string[] nm = eachLine[i].Split(' ');
                    string name = nm[1];
                    //cons.Add(name);
                    int num;
                    if (int.TryParse(f[1], out num) || (f[1].Contains("+") && !f[1].Contains("?")) || f[1].Contains("-") || f[1].Contains("Camera"))
                    {
                        string[] r = f[1].Split('+');
                        string[] re = f[1].Split('-');
                        int k = 0;
                        if (r.Length > 1)
                        {
                            for (int zxc = 0; zxc < r.Length; zxc++)
                            {
                                if (f[1].Contains("+"))
                                {
                                    int nmber;
                                    if (int.TryParse(r[zxc], out nmber))
                                        k += Convert.ToInt32(r[zxc]);
                                    else
                                    {
                                        for (int x = 0; x < variable.Count; x++)
                                        {
                                            string[] t = r[zxc].Split('\r');
                                            string gh = t[0].Replace(" ", "");
                                            string kt = variable[x].Item1;
                                            if (gh == kt)
                                            {
                                                k += variable[x].Item2;
                                            }
                                        }
                                    }
                                }
                            }
                            variable.Add(new Tuple<string, int>(name, k));
                        }
                        else if (re.Length > 1)
                        {
                            for (int zxc = 0; zxc < re.Length; zxc++)
                            {
                                if (f[1].Contains("-"))
                                {
                                    int nmber;
                                    k = Convert.ToInt32(re[0]);
                                    if (int.TryParse(re[zxc], out nmber))
                                        k -= Convert.ToInt32(re[zxc]);
                                    else
                                    {
                                        for (int x = 0; x < variable.Count; x++)
                                        {
                                            string[] t = re[zxc].Split('\r');
                                            string gh = t[0].Replace(" ", "");
                                            string kt = variable[x].Item1;
                                            if (gh == kt)
                                            {
                                                k -= variable[x].Item2;
                                            }
                                        }
                                    }
                                }
                            }
                            variable.Add(new Tuple<string, int>(name, k));
                        }
                        else
                        {
                            //SendTXTBoxMsg(f[1]);
                            //output += f[1];
                            if (f[1].Contains("Camera"))
                            {
                                string[] properties = f[1].Split('.');
                                string[] lol = properties[1].Split('\r');
                                switch (lol[0])
                                {
                                    case "posX":
                                        variable.Add(new Tuple<string, int>(name, Convert.ToInt32(viewPort3d.Camera.Position.X)));
                                        break;
                                    case "posY":
                                        variable.Add(new Tuple<string, int>(name, Convert.ToInt32(viewPort3d.Camera.Position.Y)));
                                        break;
                                    case "posZ":
                                        variable.Add(new Tuple<string, int>(name, Convert.ToInt32(viewPort3d.Camera.Position.Z)));
                                        break;
                                    default:
                                        output += "Unhandled Exception: In line " + (i + 1) + " LINE: " + eachLine[i] + Environment.NewLine + "Camera property does not exist...";
                                        break;
                                }
                            }
                            else
                            {
                                variable.Add(new Tuple<string, int>(name, Convert.ToInt32(f[1])));
                            }
                        }
                    }
                    else if (f[1].Contains('('))
                    {
                        string yza = "";
                        for (int pp = 1; pp < f.Length - 1; pp++)
                            yza += f[pp] + "=";
                        yza += f[f.Length - 1];
                        string condition = yza.Split('(', ')')[1];

                        string ifTrue = yza.Split('?', ':')[1];
                        string ifFalse = yza.Split('?', ':')[2];

                        int tokenist = ConvertToken(ifTrue);
                        int tokenistFalse = ConvertToken(ifFalse);
                        if (condition.Contains("=="))
                        {
                            string[] cnd2 = Regex.Split(condition, @"\=\=");
                            int stt = 0;
                            for (int n = 0; n < variable.Count; n++)
                            {
                                string b = cnd2[0].Replace(" ", "");
                                if (b == variable[n].Item1)
                                    stt += variable[n].Item2;  //ifTrue & ifFalse
                            }
                            if (stt == Convert.ToInt32(cnd2[1]))
                                variable.Add(new Tuple<string, int>(name, tokenist));
                            else
                                variable.Add(new Tuple<string, int>(name, tokenistFalse));
                        }
                        else if (condition.Contains('>'))
                        {
                            string[] cnd2 = condition.Split('>');
                            int stt = 0;
                            for (int n = 0; n < variable.Count; n++)
                            {
                                string b = cnd2[0].Replace(" ", "");
                                if (b == variable[n].Item1)
                                    stt += variable[n].Item2;  //ifTrue & ifFalse
                            }
                            if (stt > Convert.ToInt32(cnd2[1]))
                                variable.Add(new Tuple<string, int>(name, tokenist));
                            else
                                variable.Add(new Tuple<string, int>(name, tokenistFalse));
                        }
                        else if (condition.Contains('<'))
                        {
                            ConditionalStatement(condition, '<', name, tokenist, tokenistFalse);
                        }
                    }
                    else
                    {
                        //SendTXTBoxMsg(Environment.NewLine + "Unhandled Exception: In line " + (i + 1) + " LINE: " + eachLine[i] + Environment.NewLine + "Variable equated is not an integer...");
                        int nmw;
                        if (int.TryParse(f[1], out nmw))
                            variable.Add(new Tuple<string, int>(name, Convert.ToInt32(f[1])));
                        else
                        {
                            for (int z = 0; z < variable.Count; z++)
                            {
                                string[] j = f[1].Split('\r');
                                string w = j[0].Replace(" ", "");
                                if (w == variable[z].Item1)
                                {
                                    variable.Add(new Tuple<string, int>(name, variable[z].Item2));
                                }
                                else
                                    output += Environment.NewLine + "Unhandled Exception: In line " + (i + 1) + " LINE: " + eachLine[i] + Environment.NewLine + "Variable equated is not an integer...";
                            }
                        }
                        //output += Environment.NewLine + "Unhandled Exception: In line " + (i + 1) + " LINE: " + eachLine[i] + Environment.NewLine + "Variable equated is not an integer...";
                    }
                }
                else
                {
                    string[] f = eachLine[i].Split('=');
                    for (int a = 0; a < variable.Count; a++)
                    {
                        if (eachLine[i].StartsWith(variable[a].Item1))
                        {
                            string ty = variable[a].Item1;
                            variable.RemoveAll(p => p.Item1 == variable[a].Item1);
                            variable.Add(new Tuple<string, int>(ty, ConvertToken(f[1])));
                        }
                    }
                }
            }
            if (!IsEmptyOrAllSpaces(codeComp.Text))
                SendTXTBoxMsg(output);
        }

        private int ConvertToken(string token)
        {
            string[] u = token.Split('+');
            int t = 0;
            int nbm;
            for (int i = 0; i < u.Length; i++)
            {
                if (int.TryParse(u[i], out nbm))
                {
                    t += Convert.ToInt32(u[i]);
                }
                else
                {
                    for (int p = 0; p < variable.Count; p++)
                    {
                        string b = RemoveIndentsAndSpaces(u[i]);
                        if (b == variable[p].Item1)
                            t += variable[p].Item2;
                    }
                }
            }
            return t;
        }
        private string RemoveIndentsAndSpaces(string str)
        {
            string v = str.Split('\r')[0];
            string z = v.Replace(" ", "");
            return z;
        }
        private void ConditionalStatement(string conditionSt, char splitter, string name, int ifTrue, int ifFalse)
        {
            string[] cnd2 = conditionSt.Split(splitter);
            int stt = 0;
            for (int n = 0; n < variable.Count; n++)
            {
                string b = cnd2[0].Replace(" ", "");
                if (b == variable[n].Item1)
                    stt += variable[n].Item2;  //ifTrue & ifFalse
            }
            if (stt < Convert.ToInt32(cnd2[1]))
                variable.Add(new Tuple<string, int>(name, ifTrue));
            else
                variable.Add(new Tuple<string, int>(name, ifFalse));
        }

        private void loadmV3dFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "All mv3d|*.mv3d;"
            };
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                codeComp.Text = File.ReadAllText(openFileDlg.FileName);
                filemVComp.Text = "File: " + openFileDlg.FileName;
                mv3dFile = openFileDlg.FileName;
            }
        }

        private void savemV3dFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "All MV3D|*.mv3d;"
            };
            if (mv3dFile == null)
            {
                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllText(saveFileDialog.FileName, codeComp.Text);
            }
            else
            {
                File.WriteAllText(mv3dFile, codeComp.Text);
            }
        }

        private void ListViewItem_MouseDoubleClick_2(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "3D Object|*.STL;*.OBJ;*.FBX"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                OnlyGetModel();
                viewPort3d.Export(saveFileDialog.FileName);
                viewPort3d.Children.Add(d);
                MODEL_PATH = saveFileDialog.FileName;
                SendTXTBoxMsg("File '" + saveFileDialog.FileName + "' has been saved...");
            }
        }

        private void ListViewItem_MouseDoubleClick_3(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "3D Object|*.STL;*.OBJ;*.FBX"
            };

            if (MODEL_PATH != null)
            {
                OnlyGetModel();
                viewPort3d.Export(MODEL_PATH);
                viewPort3d.Children.Add(d);
                SendTXTBoxMsg("File '" + MODEL_PATH + "' has been saved..");
            }
        }
        private void ListViewItem_MouseDoubleClick_4(object sender, MouseButtonEventArgs e)
        {
            currentTexture = @"Textures\bricktexture.jpg";
            selectedTxture.Text = "Selected Texture: Brick";
        }

        private void ListViewItem_MouseDoubleClick_5(object sender, MouseButtonEventArgs e)
        {
            currentTexture = @"Textures\papertexture.jpg";
            selectedTxture.Text = "Selected Texture: Paper";
        }

        private void ListViewItem_MouseDoubleClick_6(object sender, MouseButtonEventArgs e)
        {
            currentTexture = @"Textures\marbletexture.jpg";
            selectedTxture.Text = "Selected Texture: Marble";
        }

        private void ListViewItem_MouseDoubleClick_7(object sender, MouseButtonEventArgs e)
        {
            currentTexture = @"Textures\woodtexture.jpg";
            selectedTxture.Text = "Selected Texture: Wood";
        }

        private void ListViewItem_MouseDoubleClick_8(object sender, MouseButtonEventArgs e)
        {
            currentTexture = @"Textures\fullytransparent.png";
            selectedTxture.Text = "Selected Texture: Transparent";
        }
        private double FindFactorial(double num)
        {
            double i, fact = 1;
            for (i = 1; i <= num; i++)
                fact = fact * i;
            return fact;
        }
        private void PlotFunctionFromTXTBox()
        {
            string[] lines = plotTxtBox.Text.Split('\n');
            string lastLine = lines[lines.Length - 1];
            string[] command = lastLine.Split('>');
            string[] arguments = command[1].Split('[', ']');

            var lambdaParser = new NReco.Linq.LambdaParser();
            Func<double, double, double> function = (x, y) =>
            {
                var varContext = new Dictionary<string, object>();
                Random rnd = new Random();
                //Constants and Variables
                varContext["x"] = x;
                varContext["y"] = y;
                varContext["pi"] = 3.1415937M;
                varContext["e"] = 2.71828183M;
                varContext["eumas"] = 0.57721566M;
                varContext["rand"] = rnd.NextDouble();
                //Lambda functions
                varContext["pow"] = (Func<double, double, double>)((a, b) => Math.Pow(a, b));
                varContext["sqrt"] = (Func<double, double>)((a) => Math.Sqrt(a));
                varContext["exp"] = (Func<double, double>)((a) => Math.Exp(a));
                varContext["cos"] = (Func<double, double>)((a) => Math.Cos(a));
                varContext["sin"] = (Func<double, double>)((a) => Math.Sin(a));
                varContext["tan"] = (Func<double, double>)((a) => Math.Tan(a));
                varContext["log"] = (Func<double, double>)((a) => Math.Log(a));
                varContext["fact"] = (Func<double, double>)((a) => FindFactorial(a));
                varContext["abs"] = (Func<double, double>)((a) => Math.Abs(a));
                varContext["random"] = (Func<double, double, double>)((a,b) => rnd.NextDouble() * (a -b) + b);

                return Convert.ToDouble(lambdaParser.Eval(arguments[0], varContext));
            };

            if (arguments.Length <= 0b1)
                viewmodel.PlotFunction(function);
            else
                viewmodel.PlotFunction(function, Convert.ToDouble(arguments[1]), Convert.ToDouble(arguments[3]));
            string plotted = command[1].TrimStart();
            SendPlotTXTMsg("Function plotted " + plotted + "...");
        }
        private void btnPlot_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = plotTxtBox.Text.Split('\n');
            string lastLine = lines[lines.Length - 1];
            string[] command = lastLine.Split('>');
            _ = command[1].Split('[', ']');
            try
            {
                if (command[1].StartsWith(" clear"))
                {
                    plotTxtBox.Text = "> ";
                    plotTxtBox.SelectionStart = plotTxtBox.Text.Length;
                    plotTxtBox.SelectionLength = 0;
                    plotTxtBox.ScrollToEnd();
                }
                else
                    PlotFunctionFromTXTBox();
            }
            catch (Exception ex)
            {
                SendTXTBoxMsg(ex.ToString());
            }

        }
        private void plotTxtBox_KeyDown(object sender, KeyEventArgs e)
        {
            string[] lines = plotTxtBox.Text.Split('\n');
            string lastLine = lines[lines.Length - 1];
            string[] command = lastLine.Split('>');
            string[] arguments = command[1].Split('[', ']');
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(command[1]))
                        BlankEntry(plotTxtBox);
                    else
                    {
                        if (command[1].StartsWith(" clear") || command[1].StartsWith("clear"))
                        {
                            textboxHis.ClearHistory();
                            plotTxtBox.Text = "> ";
                            plotTxtBox.SelectionStart = plotTxtBox.Text.Length;
                            plotTxtBox.SelectionLength = 0;
                            plotTxtBox.ScrollToEnd();
                        }
                        else if (command[1].StartsWith("help") || command[1].StartsWith(" help"))
                        {
                            textboxHis.AddToHistory(command[1]);
                            string power = "\tpow(base,index) -- by the power of" + Environment.NewLine;
                            string cos = "\tcos() -- cosine function" + Environment.NewLine;
                            string sin = "\tsin() -- sine function" + Environment.NewLine;
                            string log = "\tlog() -- logarithm function" + Environment.NewLine;
                            string exp = "\texp() -- function raised to the euler number" + Environment.NewLine;
                            string abs = "\tabs() -- absolute value of function" + Environment.NewLine;
                            string fact = "\tfact() -- factorial of function (MAY CAUSE TO CRASH)" + Environment.NewLine;
                            string pi = "\tpi -- pi constant" + Environment.NewLine;
                            string euler = "\te -- euler constant" + Environment.NewLine;
                            string eumas = "\teumas -- euler-mascheroni constant" + Environment.NewLine;
                            string rand = "\trand -- random double from 0-1" + Environment.NewLine;
                            string random = "\trandom(max,min) -- random double from 0-1" + Environment.NewLine;
                            SendPlotTXTMsg("---List of commands---" + Environment.NewLine + "\t--Functions--" + Environment.NewLine + power + cos + sin + log + exp + abs + fact
                                + "\t--Constants--" + Environment.NewLine + pi + euler + eumas + rand + random);
                        }
                        else
                        {
                            textboxHis.AddToHistory(command[1]);
                            PlotFunctionFromTXTBox();
                        }
                    }
                }
                catch (Exception ex)
                {
                    SendPlotTXTMsg($"ERROR: {ex.Message}");
                }
            }
            else if (e.Key == Key.F1)
            {
                textboxHis.TraverseUp(plotTxtBox, ">");
            }
            else if (e.Key == Key.F2)
            {
                textboxHis.TraverseDown(plotTxtBox, ">");
            }
        }
        private void hViewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
        private void btnPlotSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "3D Object|*.STL;*.OBJ;*.FBX"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                hViewport.Export(saveFileDialog.FileName);
                SendTXTBoxMsg("File '" + saveFileDialog.FileName + "' has been saved...");
            }
        }
        private void plotTxtBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            plotTxtBox.Text += "";
            plotTxtBox.SelectionStart = plotTxtBox.Text.Length;
            plotTxtBox.SelectionLength = 0;
            plotTxtBox.ScrollToEnd();
        }
        private void viewPort3d_KeyDown(object sender, KeyEventArgs e)
        {
            if (SelectedObject != null)
            {
                if (e.Key == Key.Delete)
                {
                    ClearManipulator();
                    viewPort3d.Children.Remove((ModelVisual3D)SelectedObject);
                    lblSelected.Content = "Selected: ";
                    selectedItmLbl.Text = "No item selected"; 
                    propertyGrid.SelectedObject = null;
                }
                else if (e.Key == Key.I)
                {
                    ClearManipulator();
                    switch (SelectedObject)
                    {
                        case TruncatedConeVisual3D g:
                            g.Height += 0.5;
                            break;
                        case EllipsoidVisual3D g:
                            g.RadiusX += 0.5;
                            g.RadiusY += 0.5;
                            g.RadiusZ += 0.5;
                            break;
                        case SphereVisual3D g:
                            g.Radius += 0.5;
                            break;
                        case CubeVisual3D g:
                            g.SideLength += 0.5;
                            break;
                        case TextVisual3D g:
                            g.Height += 0.5;
                            break;
                        case TorusVisual3D g:
                            g.TorusDiameter += 0.5;
                            break;
                    }
                }
                else if (e.Key == Key.K)
                {
                    ClearManipulator();
                    switch (SelectedObject)
                    {
                        case TruncatedConeVisual3D g:
                            g.Height -= (g.Height > 0) ? 0.5 : g.Height;
                            break;
                        case EllipsoidVisual3D g:
                            if (g.RadiusX > 0 && g.RadiusY > 0 && g.RadiusZ > 0)
                            {
                                g.RadiusX -= 0.5;
                                g.RadiusY -= 0.5;
                                g.RadiusZ -= 0.5;
                            }
                            break;
                        case SphereVisual3D g:
                            g.Radius -= (g.Radius > 0) ? 0.5 : g.Radius;
                            break;
                        case CubeVisual3D g:
                            g.SideLength -= (g.SideLength > 0) ? 0.5 : g.SideLength;
                            break;
                        case TextVisual3D g:
                            g.Height -= (g.Height > 0) ? 0.5 : g.Height;
                            break;
                        case TorusVisual3D g:
                            g.TorusDiameter -= (g.TorusDiameter > 0) ? 0.5 : g.TorusDiameter;
                            break;
                    }
                }
                else if (e.Key == Key.J)
                {
                    ClearManipulator();
                    if (SelectedObject is TruncatedConeVisual3D)
                    {
                        TruncatedConeVisual3D g = SelectedObject as TruncatedConeVisual3D;
                        if (g.BaseRadius > 0)
                            g.BaseRadius -= 0.5;
                    }
                }
                else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
                    && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    var config = new MapperConfiguration(cfg => cfg.CreateMap<ModelVisual3D, ModelVisual3D>());
                    var mapper = new Mapper(config); 
                    ModelVisual3D newModel = mapper.Map<ModelVisual3D>(SelectedObject);
                    ModelVisual3D bn = SelectedObject as ModelVisual3D;
                    newModel.SetName(bn.GetName() + " (copy)");
                    CopyAndPasteModel.model = newModel;
                }
                else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
                    && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    viewPort3d.Children.Add(CopyAndPasteModel.model);
                    SendTXTBoxMsg("Successfully pasted model...");
                }
            }
        }
        private void ListViewItem_MouseDoubleClick_9(object sender, MouseButtonEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "3D Object|*.STL;*.OBJ;*.FBX"
            };
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                ModelVisual3D device3D = new ModelVisual3D();
                FileInfo f = new FileInfo(openFileDlg.FileName);
                string filename = f.Name;
                device3D.Content = Display3d(openFileDlg.FileName);
                device3D.SetName(filename + " (external)");
                viewPort3d.Children.Add(device3D);
            }
        }
        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            AddObject b = new AddObject();
            if (item != null && item.IsSelected)
            {
                if (item == MeshObjectList.Items[0])
                {
                    b.Show();
                    b.ShowCorrectMesh("Cube");
                }
                else if (item == MeshObjectList.Items[1])
                {
                    b.Show();
                    b.ShowCorrectMesh("Sphere");
                }
                else if (item == MeshObjectList.Items[2])
                {
                    b.Show();
                    b.ShowCorrectMesh("Ellipsoid");
                }
                else if (item == MeshObjectList.Items[3])
                {
                    b.Show();
                    b.ShowCorrectMesh("Cylinder");
                }
                else if (item == MeshObjectList.Items[4])
                {
                    b.Show();
                    b.ShowCorrectMesh("Cone");
                }
                else if (item == MeshObjectList.Items[5])
                {
                    b.Show();
                    b.ShowCorrectMesh("Torus");
                }
                else if (item == MeshObjectList.Items[6])
                {
                    b.Show();
                    b.ShowCorrectMesh("Text");
                }
            }
        }
        private void ListViewItem_MouseDoubleClick_10(object sender, MouseButtonEventArgs e)
        {
            GLSLShading glslShade = new GLSLShading();
            glslShade.Show();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.BorderThickness = new System.Windows.Thickness(6);
            else
                this.BorderThickness = new System.Windows.Thickness(0);
            maximumMin.Content = (WindowState == WindowState.Normal) ? "🗖" : "⧉";
        }
        private void maximumMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState = WindowState.Maximized : WindowState = WindowState.Normal;
            maximumMin.Content = (WindowState == WindowState.Normal) ? "🗖" : "⧉";
        }
        private void viewPort3d_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string fn = System.IO.Path.GetFileName(files[0]);
                for (int i = 0; i < files.Length; i++)
                {
                    ModelVisual3D device3D = new ModelVisual3D
                    {
                        Content = Display3d(files[i])
                    };
                    device3D.SetName(fn + " (external)");
                    viewPort3d.Children.Add(device3D);
                    FileInfo f = new FileInfo(files[i]);
                    SendTXTBoxMsg("Successfully imported external model: " + f.Name);
                }
            }
        }
        public void OpenNewModel()
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "3D Object|*.STL;*.OBJ;*.FBX"
            };
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                try
                {
                    if (openFileDlg.FileName == MODEL_PATH)
                    {
                        SendTXTBoxMsg("Error: Current file is already open...");
                    }
                    else
                    {
                        Clear();
                        MODEL_PATH = openFileDlg.FileName;
                        ModelVisual3D device3D = new ModelVisual3D
                        {
                            Content = Display3d(MODEL_PATH)
                        };

                        viewPort3d.Children.Add(device3D);
                        label.Content = "mV3DViewer - " + MODEL_PATH;
                        FileInfo f = new FileInfo(MODEL_PATH);
                        string fileNameinLbl = f.Name;
                        device3D.SetName(fileNameinLbl);
                        fileNamelbl.Content = "Current file: " + fileNameinLbl;
                        SendTXTBoxMsg("Loaded model: " + MODEL_PATH);
                    }
                }
                catch
                {
                    SendTXTBoxMsg("Error: There was an issue with loading the model...");
                }
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            NewItemMake();
        }
        private void codeExec_2_Click(object sender, RoutedEventArgs e)
        {
            ParseCommand(codeComp.Text);
        }
        private void MoveSelectedObjectX(double amountX, double amountY, double amountZ)
        {
            if (SelectedObject is SphereVisual3D)
            {
                ClearManipulator();
                SphereVisual3D v = SelectedObject as SphereVisual3D;
                v.Center = new Point3D(amountX,amountY,amountZ);
            }
        }
        private void SetCamPosition(double x, double y, double z)
        {
            viewPort3d.Camera.Position = new Point3D(x,y,z);
        }
        public void ParseCommand(string command)
        {
            //LAMBDA IF ELSE
            //Expression.Lambda<Action>(ifThenElseExpr).Compile()();
            //LAMBDA IF ELSE
            Random rnd = new Random();
            varContext["pi"] = Math.PI;
            varContext["cos"] = (Func<double, double>)((a) => Math.Cos(a));
            varContext["printmv"] = (Func<object, object>)((a) => a);
            varContext["selectedPos"] = (Action<double,double,double>)MoveSelectedObjectX;
            varContext["rand"] = (Func<double, double, double>)((a, b) => rnd.Next((int)a, (int)b)); //rnd.Next(0, 100);
            varContext["mVCamera"] = viewPort3d.Camera.Position;
            varContext["setCamPosition"] = (Action<double, double, double>)SetCamPosition;
            string[] ert = command.Split(';');
            for (int i = 0; i < ert.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(ert[i]))
                {
                    string nm = ert[i].TrimStart();
                    if (!nm.Contains("var") && !nm.Contains("if") && !nm.Contains("}") && !nm.Contains("function") && !nm.Contains("return") && !nm.Contains("while"))
                    {
                        try
                        {
                            try
                            {
                                SendTXTBoxMsg(lambdaParser.Eval(ert[i], varContext).ToString());
                            }
                            catch { }
                        }
                        catch (Exception e)
                        {
                            SendTXTBoxMsg(e.ToString().Split('\n')[0].Split(':')[1].TrimStart().TrimEnd());
                        }
                    }
                    else
                        Declare(ert[i]);
                }
            }
        }
        public void Declare(string dcl)
        {
            string[] gh = dcl.Split(';');
            for (int i = 0; i < gh.Length; i++)
            {
                string nm = gh[i].TrimStart();
                if (nm.StartsWith("var")) {
                    string name0 = nm.Split(' ')[1];
                    string name = name0.Split('=')[0];
                    string[] bnm = nm.Split('=');
                    if (bnm.Length > 1)
                    {
                        string parameter = nm.Split('=')[1];
                        object r = lambdaParser.Eval(parameter, varContext);
                        varContext[name] = r;
                    }
                    else
                        varContext[name] = null;
                }
                if (nm.Contains("if")) {
                    string condition = nm.Split('(', ')')[1];
                    object trueORFalse = lambdaParser.Eval(condition, varContext);
                    string ifTrueBracket = nm.Split('{', '}')[1];
                    string[] tyui = ifTrueBracket.Split(':');
                    if (trueORFalse.ToString() == "True")
                    {
                        for (int q = 0; q < tyui.Length; q++)
                            ParseCommand(tyui[q]);
                    }
                    else
                        continue;
                }
                if (nm.Contains("function"))
                {
                    string functionname = nm.Split(' ')[1];
                    string functionS = nm.Split('{', '}')[1];
                    string[] tyu = functionS.Split(':');
                    Dictionary<string, object> vCT = new Dictionary<string, object>();
                    for (int j = 0; j < tyu.Length; j++)
                    {
                        string ui = tyu[j].TrimStart();
                        if (!ui.Contains("return"))
                            ParseCommand(ui);
                        else
                        {
                            string[] returnType = ui.Split(' ');
                            string parseReturn = "";
                            for (int u = 1; u < returnType.Length; u++)
                                parseReturn += returnType[u];
                            object parseFunctionS = lambdaParser.Eval(parseReturn, varContext);
                            varContext[functionname] = (Func<object>)(() => parseFunctionS);
                        }

                    }
                    //object parseFunctionS = lambdaParser.Eval(functionS, varContext);
                    //varContext[functionname] = (Func<object>)(() => parseFunctionS);
                }
                if (nm.Contains("while"))
                {
                   string condition = nm.Split('(', ')')[1];
                   object trueORFalse = lambdaParser.Eval(condition, varContext);
                   string ifTrueBracket = nm.Split('{', '}')[1];
                    string[] BracketInputs = ifTrueBracket.Split(':');
                   while (trueORFalse.ToString() == "True")
                   {
                        for (int y = 0; y < BracketInputs.Length; y++)
                        {
                            Dispatcher.Invoke(() => {
                            trueORFalse = lambdaParser.Eval(condition, varContext);
                            ParseCommand(BracketInputs[y]);
                            });
                        }
                   }
                }
            }
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            OpenNewModel();
        }
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            GridLines((isGridLines.SetGridLines) ? "on" : "off");
            isGridLines.SetGridLines = (!isGridLines.SetGridLines);
        }
        private void ListViewItem_MouseDoubleClick_11(object sender, MouseButtonEventArgs e)
        {
            if (SelectedObject != null) {
                CutMesh cm = new CutMesh();
                cm.Show();
            }
            else
                SendTXTBoxMsg("Error: Cannot cut null object...");
        }
        private void settingsMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Main_Settings main_Settings = new Main_Settings();
            main_Settings.Show();
        }
    }
    public class isGridLines
    {
        public static bool SetGridLines = false;
    }
    public class ModelProperty
    {
        public string Name { get; set; }
        public string Object { get; set; }
        public Matrix3D Transform { get; set; }
        public string HasAnimatedProperties { get; set; }
        public string Offset { get; set; }
    }
    public class CopyAndPasteModel
    {
        public static ModelVisual3D model { get; set; }
    }
    public class TriangleManipulator
    {
        public static double x { get; set; }
        public static double y { get; set; }
        public static double z { get; set; }
    }
}
