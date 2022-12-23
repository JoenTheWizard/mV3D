using CefSharp;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace mV3DViewer
{
    /// <summary>
    /// Interaction logic for GLSLShading.xaml
    /// </summary>
    public partial class GLSLShading : Window
    {
        #region MAIN
        TextBoxHistory hist = new TextBoxHistory();
        List<string> textures;
        public string MUSIC;
        public string FRAGMENT_FILE = null;
        #region CONFIGS
        public struct CONFIGS
        {
            public bool isPaused;
            public bool isAutocomplete;
            public bool isFullScreen;
            public bool isInterpCommand;
            public bool isMusicLoop;
        }
        public CONFIGS cnfgs;
        #endregion
        #endregion
        public GLSLShading()
        {
            InitializeComponent();
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            GLSLRender.MenuHandler = new CustomMenuHandler();
            GLSLRender.ConsoleMessage += GLSLRender_ConsoleMessage;
            textures = new List<string>();

            cnfgs = new CONFIGS() {
                isPaused = false,
                isFullScreen = false,
                isAutocomplete = true,
                isInterpCommand = true,
                isMusicLoop = false
            };

            //isPaused = false;
            //isFullScreen = false;
            //isAutocomplete = true;
            //isInterpCommand = true;
            //isMusicLoop = false;

            MUSIC = "";
            glslConsole.Text = $"Welcome to the ShadeD console! Type help for a list of commands!{Environment.NewLine}" +
                $"Reminder: For graphically heavy and expensive programs, try running them in lower resolution!{Environment.NewLine}> ";
            vertexCode.TextArea.TextEntering += TextArea_TextEntering;
        }
        private void SetHightlighting(ICSharpCode.AvalonEdit.TextEditor textBox)
        {
            using (XmlTextReader reader = new XmlTextReader(new FileStream("glsl.xshd", FileMode.Open)))
                textBox.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetHightlighting(vertexCode);
            SetHightlighting(fragmentCode);
            using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
            {
                foreach (ManagementObject obj in searcher.Get())
                    gpuUsage.Text += $"{obj["Name"]} ";
            }
        }
        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (cnfgs.isAutocomplete)
            {
                if (e.Text == "v" || e.Text == "ve")
                {
                    CompletionWindow windowComp = new CompletionWindow(vertexCode.TextArea)
                    {
                        Background = new SolidColorBrush(Color.FromArgb(122, 172, 172, 172)),
                        Foreground = Brushes.White,
                        FontFamily = new FontFamily("Consolas"),
                        BorderThickness = new Thickness(5, 5, 5, 5),
                        AllowsTransparency = true,
                        BorderBrush = new SolidColorBrush(Color.FromArgb(122, 72, 72, 72))
                    };
                    IList<ICompletionData> data = windowComp.CompletionList.CompletionData;
                    data.Add(new MyCompletionData("vec2"));
                    data.Add(new MyCompletionData("vec3"));
                    data.Add(new MyCompletionData("vec4"));
                    data.Add(new MyCompletionData("ivec2"));
                    data.Add(new MyCompletionData("ivec3"));
                    data.Add(new MyCompletionData("ivec4"));
                    data.Add(new MyCompletionData("bvec2"));
                    data.Add(new MyCompletionData("bvec3"));
                    data.Add(new MyCompletionData("bvec4"));
                    windowComp.Show();
                }
                else if (e.Text == "m" || e.Text == "ma")
                {
                    CompletionWindow windowComp = new CompletionWindow(vertexCode.TextArea)
                    {
                        Background = new SolidColorBrush(Color.FromArgb(122, 172, 172, 172)),
                        Foreground = Brushes.White,
                        FontFamily = new FontFamily("Consolas"),
                        BorderThickness = new Thickness(5, 5, 5, 5),
                        AllowsTransparency = true,
                        BorderBrush = new SolidColorBrush(Color.FromArgb(122, 72, 72, 72))
                    };
                    IList<ICompletionData> data = windowComp.CompletionList.CompletionData;
                    data.Add(new MyCompletionData("mat2"));
                    data.Add(new MyCompletionData("mat3"));
                    data.Add(new MyCompletionData("mat4"));
                    windowComp.Show();
                }
            }
        }
        private void GoFullscreen()
        {
            WindowState = WindowState.Maximized;
            GLSLRender.SetValue(Grid.ColumnProperty, 0);
            GLSLRender.SetValue(Grid.RowProperty, 0);
            GLSLRender.Margin = new Thickness(0, 0, 0, 0);
            GLSLRender.Height = SystemParameters.PrimaryScreenHeight - (SystemParameters.PrimaryScreenHeight * 0.01);
            GLSLRender.Width = SystemParameters.PrimaryScreenWidth;
            cnfgs.isFullScreen = true;
        }
        private void ResizeTextBoxes()
        {
            double newWidth = tabGLSL.ActualWidth - 10;
            glslConsole.Width = (!(tabGLSL.ActualWidth <= 10)) ? newWidth : glslConsole.Width;
            errorLog.Width = (!(tabGLSL.ActualWidth <= 10)) ? newWidth : glslConsole.Width;
        }
        private void GLSLRender_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                errorLog.Text += e.Message + Environment.NewLine;
                errorLog.ScrollToEnd();
            }));
        }
        string musicHTMLRender(string url)
        {
            return "<html><head><meta http-equiv='X-UA-Compatible' content='IE=edge'></head><center></center><body></body><body><iframe width='0' height='0' src='https://www.youtube.com/embed/" + url + "?&autoplay=1;loop=1' frameborder='0' allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture' allowfullscreen></iframe></body></html>";
        }
        string musicHTMLRenderLoop(string url)
        {
            return "<html><head><meta http-equiv='X-UA-Compatible' content='IE=edge'></head><iframe class='embed-responsive-item' id='ytplayer' type='text/html' width='0' height='0' src='https://www.youtube.com/embed/" + url + "?&autoplay=1&loop=1&rel=0&showinfo=0&color=white&iv_load_policy=3&playlist=" + url + "' frameborder='0' allowfullscreen ></iframe></html>";
        }
        public void InlineCommands()
        {
            try
            {
                string[] texBox = fragmentCode.Text.Split('\n');
                foreach (string a in texBox)
                {
                    string trimmed = a.TrimStart();
                    if (trimmed.StartsWith("///"))
                    {
                        string[] cmd = trimmed.Split(new string[] { "///" }, StringSplitOptions.None);
                        string[] cmdParams = cmd[1].Split('<', '>')[1].TrimStart().Split(' ');
                        string endResult = "";
                        if (cmdParams[0].ToLower().StartsWith("music"))
                        {
                            for (int i = 1; i < cmdParams.Length; i++)
                            {
                                if (cmdParams[i].ToLower().StartsWith("id"))
                                {
                                    string songURL = cmdParams[i].Split('=')[1].Split('"', '"')[1];
                                    endResult = songURL;
                                }
                                else if (cmdParams[i].ToLower().StartsWith("loop"))
                                {
                                    string isTrueOrFalse = cmdParams[i].Split('=')[1].Split('"', '"')[1];
                                    if (isTrueOrFalse.ToLower() == "true")
                                        cnfgs.isMusicLoop = true;
                                    else if (isTrueOrFalse.ToLower() == "false")
                                        cnfgs.isMusicLoop = false;
                                }
                            }
                            musicRender.NavigateToString((cnfgs.isMusicLoop) ? musicHTMLRenderLoop(endResult) :
                                musicHTMLRender(endResult));
                        }
                        else if (cmdParams[0].ToLower().StartsWith("texture"))
                        {
                            textures.Clear();
                            for (int i = 1; i < cmdParams.Length; i++)
                            {
                                if (cmdParams[i].ToLower().StartsWith("id")){
                                    string texURL = cmdParams[i].Split('=')[1].Split('"', '"')[1];
                                    textures.Add(texURL);
                                }
                            }
                            LoadTextures();
                        }
                    }
                }
            }
            catch { }
        }
        private string CodeGolfFormat(){
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("#define T u_time");
            sb.AppendLine("#define R u_resolution");
            sb.AppendLine("#define M u_mouse");
            sb.AppendLine("#define K gl_FragColor");
            sb.AppendLine("#define L gl_FragCoord.xy");
            return $"uniform float u_time;uniform vec2 u_resolution;uniform vec2 u_mouse;\n{sb.ToString()}";
        }
        private string XSSCheck(string xssCheck)
        {
            string rt = "";
            string[] spl = xssCheck.Split(new string[] { "</script>", "</script" }, StringSplitOptions.None);
            for (int i = 0; i < spl.Length; i++)
                rt += spl[i];
            return rt;
        }
        public string CompileShaders(string vertexShader, string fragmentShader)
        {
            string[] txtArr = textures.ToArray();
            string textureUniforms = "";
            for (int i = 0; i < txtArr.Length; i++)
                textureUniforms += "u_texture" + i + ": {value: new THREE.TextureLoader().load('" + txtArr[i] + "')},";
            if (MUSIC != "")
            {
                string msc = "<html><head><meta http-equiv='X-UA-Compatible' content='IE=edge'></head><center></center><body></body><body><iframe width='0' height='0' src='https://www.youtube.com/embed/" + MUSIC + "?&autoplay=1;loop=1' frameborder='0' allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture' allowfullscreen></iframe></body></html>";
                musicRender.NavigateToString(msc);
            }
            else
                musicRender.NavigateToString("<html></html>");
            if (cnfgs.isInterpCommand)
                InlineCommands();
            string checkFragment = codeGolf.IsChecked?CodeGolfFormat()+fragmentShader
                :fragmentShader;
            return @"
<body>
	</body>
<style>
body { margin: 0; }
	canvas { width: 100%; height: 100% }
</style>
<script src='https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js'></script>
<script>
const vshader = `" + vertexShader + @" `
const fshader = `" + checkFragment + @" `


const scene = new THREE.Scene();
const camera = new THREE.OrthographicCamera( -1, 1, 1, -1, 0.1, 10 );
var isPaused = false;

const renderer = new THREE.WebGLRenderer();
renderer.setSize( window.innerWidth, window.innerHeight );
document.body.appendChild( renderer.domElement );

const clock = new THREE.Clock();

const uniforms = {
  " + textureUniforms + @"
  u_color: { value: new THREE.Color(0xff0000) },
  u_time: { value: 0.0 },
  u_mouse: { value:{ x:0.0, y:0.0 }},
  u_resolution: { value:{ x:0, y:0 }}
}

const geometry = new THREE.PlaneGeometry( 2, 2 );
const material = new THREE.ShaderMaterial( {
  uniforms: uniforms,
  vertexShader: vshader,
  fragmentShader: fshader
} );

const plane = new THREE.Mesh( geometry, material );
scene.add( plane );

camera.position.z = 1;

onWindowResize();
if ('ontouchstart' in window){
  document.addEventListener('touchmove', move);
}else{
  window.addEventListener( 'resize', onWindowResize, false );
  document.addEventListener('mousemove', move);
}

function move(evt){
  uniforms.u_mouse.value.x = (evt.touches) ? evt.touches[0].clientX : evt.clientX;
  uniforms.u_mouse.value.y = (evt.touches) ? evt.touches[0].clientY : evt.clientY;
}

animate();

function onWindowResize( event ) {
  const aspectRatio = window.innerWidth/window.innerHeight;
  let width, height;
  if (aspectRatio>=1){
    width = 1;
    height = (window.innerHeight/window.innerWidth) * width;
  }else{
    width = aspectRatio;
    height = 1;
  }
  camera.left = -width;
  camera.right = width;
  camera.top = height;
  camera.bottom = -height;
  camera.updateProjectionMatrix();
  renderer.setSize( window.innerWidth, window.innerHeight );
  uniforms.u_resolution.value.x = window.innerWidth;
  uniforms.u_resolution.value.y = window.innerHeight;
}

function animate() {
  requestAnimationFrame( animate );
uniforms.u_time.value += clock.getDelta();
if (!isPaused) {
    clock.start();
} else {
    clock.stop();
}
  renderer.render( scene, camera );
}
</script>
";
        }
        private async void ErrorCompileFrag()
        {
            var script = @"(function(){return 'HALLOOOO'})();";
            var result = await GLSLRender.GetMainFrame().EvaluateScriptAsync(script)
                .ContinueWith(t =>
                 {
                     var result1 = t.Result;
                     return (string)result1.ToString();
                 });
            MessageBox.Show(result);
        }
        private void CompileCodeShader()
        {
            errorLog.Clear();
            string compiledText = CompileShaders(XSSCheck(vertexCode.Text), XSSCheck(fragmentCode.Text));
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Task.Run(() =>
            {
                GLSLRender.LoadHtml(compiledText);
            });
            sw.Stop();
            TimeSpan ts = sw.Elapsed;
            SendTXTBoxMsg($"Executed in {ts.TotalMilliseconds} ms...");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CompileCodeShader();
            if (cnfgs.isPaused)
            {
                pauseUnpauseTimer.Source = new BitmapImage(new Uri(@"Assets/Extras/pause.png", UriKind.Relative));
                psTT.Content = "Pause timer";
                cnfgs.isPaused = !cnfgs.isPaused;
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void glslTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
        private void maximumMin1_Click(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState = WindowState.Maximized : WindowState = WindowState.Normal;
            maximumMin1.Content = (WindowState == WindowState.Normal) ? "🗖" : "⧉";
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.BorderThickness = (WindowState == WindowState.Maximized) ? this.BorderThickness = new System.Windows.Thickness(8) :
                this.BorderThickness = new System.Windows.Thickness(0);
            ResizeTextBoxes();
        }
        private void minBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void label1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        public string EntryPoint()
        {
            if (FRAGMENT_FILE != null)
                return FRAGMENT_FILE + "> ";
            return "> ";
        }
        private void TextEmpty()
        {
            glslConsole.Text += Environment.NewLine + EntryPoint();
            glslConsole.CaretIndex = glslConsole.Text.Length;
            glslConsole.ScrollToEnd();
        }
        public void SendTXTBoxMsg(string msg)
        {
            glslConsole.Text = glslConsole.Text + Environment.NewLine;
            glslConsole.Text += "\r" + msg + "\r\n" + Environment.NewLine;
            glslConsole.Text += EntryPoint();
            glslConsole.SelectionStart = glslConsole.Text.Length;
            glslConsole.SelectionLength = 0;
            glslConsole.ScrollToEnd();
        }
        public async void CameraPosition(string dimension, float pos)
        {
            if (GLSLRender.CanExecuteJavascriptInMainFrame)
            {
                switch (dimension)
                {
                    case "x":
                        JavascriptResponse responsex = await GLSLRender.EvaluateScriptAsync("camera.position.x +=" + pos);
                        if (responsex.Result != null)
                            SendTXTBoxMsg($"Shifted camera by {pos} in the X direction... (Camera X: {responsex.Result})");
                        break;
                    case "y":
                        JavascriptResponse responsey = await GLSLRender.EvaluateScriptAsync("camera.position.y +=" + pos);
                        if (responsey.Result != null)
                            SendTXTBoxMsg($"Shifted camera by {pos} in the Y direction... (Camera Y: {responsey.Result})");
                        break;
                    case "z":
                        JavascriptResponse responsez = await GLSLRender.EvaluateScriptAsync("camera.position.z +=" + pos);
                        if (responsez.Result != null)
                            SendTXTBoxMsg($"Shifted camera by {pos} in the Z direction... (Camera Z: {responsez.Result})");
                        break;
                    default:
                        SendTXTBoxMsg("Error: Please enter a valid dimension");
                        break;
                }

            }
        }
        public async void PauseTimer()
        {
            if (GLSLRender.CanExecuteJavascriptInMainFrame)
            {
                JavascriptResponse respause = await GLSLRender.EvaluateScriptAsPromiseAsync("isPaused = (!isPaused)");
                if (respause.Success)
                {
                    cnfgs.isPaused = !cnfgs.isPaused;
                    SendTXTBoxMsg(cnfgs.isPaused ? "Timer has been paused..." : "Timer has been unpaused");
                }
            }
        }
        public async Task<string> GetMacroList(string uri)
        {
            try {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                request.UserAgent = @"Mozilla/5.0 (Windows; Windows NT 6.1) AppleWebKit/534.23 (KHTML, like Gecko) Chrome/11.0.686.3 Safari/534.23";
                request.Method = "GET";

                //This is important for utilizing the data we got from the GET request and then 
                //deserializing the outputted JSON data from the GitHub
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream)) {
                    var deserialized = new JavaScriptSerializer();
                    dynamic d = deserialized.Deserialize<object>(await reader.ReadToEndAsync());
                    string res = "";
                    foreach (dynamic obj in d["tree"])
                        res += obj["path"].Split('.')[0] + Environment.NewLine;
                    return res;
                }
            }
            catch (Exception e) {
                return e.Message;
            }
        }

        public void Help()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("---List of help commands---");
            sb.AppendLine("\t-help -- shows a list of commands");
            sb.AppendLine("\t-run -- runs program");
            sb.AppendLine("\t-uniforms_list [--golf/--includes]* -- shows a list of GLSL set uniforms");
            sb.AppendLine("\t-clear -- clears the GLSL CMD");
            sb.AppendLine("\t-gen_vertex -- generates vertex template");
            sb.AppendLine("\t-texture [--add/--list/--remove] [texture*] -- adds texture, list texture list, or removes texture");
            sb.AppendLine("\t-pause -- pauses or unpauses timer");
            sb.AppendLine("\t-camera [x/y/z] [value] -- shifts camera position");
            sb.AppendLine("\t-fullscreen/fs -- sets viewport to fullscreen");
            sb.AppendLine("\t-music [--yt/--rm] -- sets background music to shader program");
            sb.AppendLine("\t-wallpaper -- sets current shader program as wallpaper (WARNING: CAN VASTLY REDUCE PERFORMANCE)");
            sb.AppendLine("\t-inline-commands -- disables/enables tag commands from file");
            sb.AppendLine("\t-gpu [--usage] -- prints GPU information");
            SendTXTBoxMsg(sb.ToString());
        }
        public void UniformsList()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("\tu_color -- vec3 - hex color: 0xff0000");
            stringBuilder.AppendLine("\tu_time -- float - returns time");
            stringBuilder.AppendLine("\tu_mouse -- vec2 - returns mouse position");
            stringBuilder.AppendLine("\tu_resolution -- vec2 - returns viewport resolution");
            stringBuilder.AppendLine("\tu_texture{i} -- sampler2D - returns sampled texture");
            SendTXTBoxMsg("---List of available GLSL uniforms---\n" + stringBuilder.ToString());
        }
        public void GenerateVertsTemplate()
        {
            string nl = Environment.NewLine;
            vertexCode.Text =
                "varying vec2 vUv;" + nl + "void main() {" + nl + "\tvUv = uv;" + nl +
                "\tgl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);" + nl +
                "}";
            SendTXTBoxMsg("Successfully generated vertex template!");
        }
        [Obsolete]
        private async void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            string[] lines = glslConsole.Text.Split('\n');
            string lastLine = lines[lines.Length - 1];
            string[] command = lastLine.Split('>');
            try
            {
                string[] args = command[1].Split(' ');
                if (e.Key == Key.Enter)
                {
                    try
                    {
                        if (String.IsNullOrWhiteSpace(command[1]))
                            TextEmpty();
                        else
                        {
                            if (args.Length <= 2)
                            {
                                string ah = command[1].TrimStart();
                                switch (ah)
                                {
                                    case "help":
                                        Help();
                                        hist.AddToHistory(command[1]);
                                        break;
                                    case "uniforms_list":
                                        UniformsList();
                                        hist.AddToHistory(command[1]);
                                        break;
                                    case "clear":
                                        hist.ClearHistory();
                                        glslConsole.Text = FRAGMENT_FILE != null ? FRAGMENT_FILE + "> " : "> ";
                                        glslConsole.SelectionStart = glslConsole.Text.Length;
                                        glslConsole.ScrollToEnd();
                                        break;
                                    case "gen_vertex":
                                    case "gen_verts":
                                        hist.AddToHistory(command[1]);
                                        GenerateVertsTemplate();
                                        break;
                                    case "run":
                                        hist.AddToHistory(command[1]);
                                        CompileCodeShader();
                                        SendTXTBoxMsg("Running program...");
                                        break;
                                    case "pause":
                                        hist.AddToHistory(command[1]);
                                        PauseTimer();
                                        break;
                                    case "fs":
                                    case "fullscreen":
                                        hist.AddToHistory(command[1]);
                                        GoFullscreen();
                                        TextEmpty();
                                        break;
                                    case "inline-commands":
                                        cnfgs.isInterpCommand = !cnfgs.isInterpCommand;
                                        inlineCommands.Header = cnfgs.isInterpCommand ? "Disable tag commands" : "Enable tag commands";
                                        SendTXTBoxMsg(cnfgs.isInterpCommand ? "Commands can be executed through file" : "Commands through file disabled");
                                        break;
                                    case "wallpaper":
                                        SetWallpaperBackground();
                                        SendTXTBoxMsg("Success! Set wallpaper...");
                                        break;
                                    case "jscode":
                                        SendTXTBoxMsg(await GLSLRender.GetSourceAsync());
                                        break;
                                    case "test":
                                        LoadTextures();
                                        break;
                                }
                            }
                            else
                            {
                                if (command[1].ToLower().StartsWith(" camera") == true)
                                {
                                    hist.AddToHistory(command[1]);
                                    CameraPosition(args[2], float.Parse(args[3]));
                                }
                                else if (command[1].ToLower().StartsWith(" texture"))
                                {
                                    hist.AddToHistory(command[1]);
                                    string ah = args[2].ToLower();
                                    if (ah == "--add" || ah == "-a")
                                    {
                                        if (textures.Count < 4)
                                        {
                                            textures.Add(args[3]);
                                            LoadTextures();
                                            SendTXTBoxMsg($"Added '{args[3]}' to texture list ({textures.Count}/4)");
                                        }
                                        else
                                            SendTXTBoxMsg("Error: Cannot add any more textures to the texture list!");
                                    }
                                    else if (ah == "--list" || ah == "-l")
                                    {
                                        string[] txtArr = textures.ToArray();
                                        string a = "";
                                        for (int i = 0; i < txtArr.Length - 1; i++)
                                            a += $"{txtArr[i]} (u_texture{i}) {Environment.NewLine}";
                                        a += $"{txtArr[txtArr.Length - 1]} (u_texture{txtArr.Length - 1}) {Environment.NewLine}";
                                        SendTXTBoxMsg($"--Texture List--{Environment.NewLine + a}");
                                    }
                                    else if (ah == "--remove" || ah == "-r")
                                    {
                                        textures.RemoveAt(Convert.ToInt32(args[3]));
                                        LoadTextures();
                                        SendTXTBoxMsg($"Successfully removed texture at index {args[3]}");
                                    }
                                }
                                else if (command[1].ToLower().StartsWith(" uniforms_list"))
                                {
                                    hist.AddToHistory(command[1]);
                                    if (args[2].ToLower() == "--golf"){
                                        StringBuilder s = new StringBuilder();
                                        s.AppendLine("\tT -- float - returns time");
                                        s.AppendLine("\tR -- vec2 - returns resolution");
                                        s.AppendLine("\tM -- vec2 - returns mouse coordinates");
                                        s.AppendLine("\tL -- vec2 - returns gl_FragCoord.xy");
                                        s.AppendLine("\tK -- vec4 - returns gl_FragColor");
                                        SendTXTBoxMsg(s.ToString());
                                    }
                                    else if (args[2].ToLower() == "--includes") {
                                        SendTXTBoxMsg(
                                        await GetMacroList("https://api.github.com/repos/mrdoob/three.js/git/trees/341dc35b05ac141fcccb3b9cbf4f9013560809b2")
                                        );
                                    }    
                                }
                                else if (command[1].ToLower().StartsWith(" music") == true)
                                {
                                    hist.AddToHistory(command[1]);
                                    string ah = args[2].ToLower();
                                    switch (ah)
                                    {
                                        case "--yt-add":
                                        case "--yt":
                                            MUSIC = args[3];
                                            SendTXTBoxMsg($"Saved URL link {args[3]}");
                                            break;
                                        case "--remove":
                                        case "--rm":
                                            MUSIC = (MUSIC != "") ? "" : MUSIC;
                                            SendTXTBoxMsg((MUSIC == "") ? "Successfully cleared the song from queue" : "Error: No song is specified to be cleared!");
                                            break;
                                    }
                                }
                                else if (command[1].ToLower().StartsWith(" gpu"))
                                {
                                    hist.AddToHistory(command[1]);
                                    if (args[2].ToLower() == "--usage")
                                    {
                                        glslConsole.IsReadOnly = true;
                                        SendTXTA("Please wait...");
                                        float numb = await GetGPUUsage();
                                        glslConsole.IsReadOnly = false;

                                        StringBuilder sb = new StringBuilder();
                                        using (var searcher = new ManagementObjectSearcher("select * from Win32_VideoController"))
                                        {
                                            foreach (ManagementObject obj in searcher.Get())
                                            {
                                                sb.AppendLine("GPU Name: " + obj["Name"]);
                                                sb.AppendLine("Driver Version: " + obj["DriverVersion"]);
                                            }
                                        }
                                        sb.AppendLine($"GPU Utilization percentage: {Math.Round(numb, 2)}%");
                                        SendTXTBoxMsg(sb.ToString());
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        SendTXTBoxMsg("Error: There was an error with parsing the command");
                    }
                }
                else if (e.Key == Key.F1)
                    hist.TraverseUp(glslConsole, FRAGMENT_FILE != null ? FRAGMENT_FILE + ">" : ">");
                else if (e.Key == Key.F2)
                    hist.TraverseDown(glslConsole, FRAGMENT_FILE != null ? FRAGMENT_FILE + ">" : ">");
            }
            catch
            {
                SendTXTBoxMsg("Error: There was an error with parsing the command");
            }
        }
        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            ResizeTextBoxes();
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void SaveAsProj()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "All frag|*.frag;"
            };
            if (FRAGMENT_FILE == null)
            {
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, fragmentCode.Text);
                    FRAGMENT_FILE = saveFileDialog.FileName;
                    label1.Content = $"ShadeD - {FRAGMENT_FILE}";
                    SendTXTBoxMsg($"Success! Saved project '{FRAGMENT_FILE}'");
                    //List
                    string[] directory = saveFileDialog.FileName.Split('\\');
                    string direc = "";
                    for (int i = 0; i < directory.Length - 1; i++)
                        direc += directory[i] + @"\";
                    ListDirectory(treeView, direc);
                }
            }
        }
        public void SendTXTA(string msg)
        {
            glslConsole.Text = glslConsole.Text + Environment.NewLine;
            glslConsole.Text += "\r" + msg + Environment.NewLine;
            glslConsole.SelectionStart = glslConsole.Text.Length;
            glslConsole.SelectionLength = 0;
            glslConsole.ScrollToEnd();
        }
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            SaveAsProj();
        }
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (FRAGMENT_FILE == null)
                SaveAsProj();
            else{
                File.WriteAllText(FRAGMENT_FILE, fragmentCode.Text);
                SendTXTBoxMsg($"Success! Current project '{FRAGMENT_FILE}' saved");
            }
        }
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            vertexCode.ShowLineNumbers = (!vertexCode.ShowLineNumbers);
            fragmentCode.ShowLineNumbers = (!fragmentCode.ShowLineNumbers);
        }
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "All frag|*.frag;"
            };
            bool? result = openFileDlg.ShowDialog();
            if (result == true)
            {
                fragmentCode.Text = File.ReadAllText(openFileDlg.FileName);
                label1.Content = $"ShadeD - {openFileDlg.FileName}";
                FRAGMENT_FILE = openFileDlg.FileName;
                string[] directory = openFileDlg.FileName.Split('\\');
                string direc = "";
                for (int i = 0; i < directory.Length - 1; i++)
                    direc += directory[i] + @"\";
                ListDirectory(treeView, direc);
                SendTXTBoxMsg($"Success! Opened project '{openFileDlg.FileName}'");
            }
        }
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            FRAGMENT_FILE = null;
            fragmentCode.Clear();
            vertexCode.Clear();
            treeView.Items.Clear();
            label1.Content = "ShadeD";
            SendTXTBoxMsg("Success! Created new project...");
        }
        private void pauseBtn_Click(object sender, RoutedEventArgs e)
        {
            PauseTimer();
            //pauseBtn.Content = isPaused ? "⏸" : "▶️";
            pauseUnpauseTimer.Source = cnfgs.isPaused ? new BitmapImage(new Uri(@"Assets/Extras/pause.png", UriKind.Relative)) :
                new BitmapImage(new Uri(@"Assets/Extras/playunpause.png", UriKind.Relative));
            psTT.Content = cnfgs.isPaused ? "Pause timer" : "Unpause timer";
        }
        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            cnfgs.isAutocomplete = !cnfgs.isAutocomplete;
            autoComptxt.Header = cnfgs.isAutocomplete ? autoComptxt.Header : "Enable Autocomplete";
        }
        private void isInFullscreen(KeyEventArgs e)
        {
            if (e.Key == Key.F || e.Key == Key.Escape)
            {
                if (cnfgs.isFullScreen)
                {
                    WindowState = WindowState.Maximized;
                    GLSLRender.SetValue(Grid.ColumnProperty, 1);
                    GLSLRender.SetValue(Grid.RowProperty, 1);
                    GLSLRender.Margin = new Thickness(2, 29, 5, 130);
                    GLSLRender.Height = Double.NaN;
                    GLSLRender.Width = Double.NaN;
                    cnfgs.isFullScreen = false;
                }
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            isInFullscreen(e);
        }
        private void flScr_Click(object sender, RoutedEventArgs e)
        {
            GoFullscreen();
        }
        [Obsolete]
        private void SetWallpaperBackground()
        {
            IntPtr progman = W32.FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;
            W32.SendMessageTimeout(progman,
                 0x052C,
                 new IntPtr(0),
                 IntPtr.Zero,
                 W32.SendMessageTimeoutFlags.SMTO_NORMAL,
                 1000,
                 out result);
            IntPtr workerw = IntPtr.Zero;
            W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = W32.FindWindowEx(tophandle,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            IntPtr.Zero);
                if (p != IntPtr.Zero)
                {
                    // Gets the WorkerW Window after the current one.
                    workerw = W32.FindWindowEx(IntPtr.Zero,
                                               tophandle,
                                               "WorkerW",
                                               IntPtr.Zero);
                }
                return true;
            }), IntPtr.Zero);
            IntPtr dc = W32.GetDCEx(workerw, IntPtr.Zero, (W32.DeviceContextValues)0x403);
            if (dc != IntPtr.Zero)
            {
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHdc(dc))
                {
                    //g.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.White),0,0,500,500);
                }
                W32.ReleaseDC(workerw, dc);
            }

            System.Windows.Forms.Form form = new System.Windows.Forms.Form();

            form.Load += new EventHandler((s, e) =>
            {
                // Move the form right next to the in demo 1 drawn rectangle
                form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                form.Width = (int)SystemParameters.MaximizedPrimaryScreenWidth - 10;
                form.Height = (int)SystemParameters.MaximizedPrimaryScreenHeight - 5;
                form.Left = 0;
                form.Top = 0;

                CefSharp.WinForms.ChromiumWebBrowser wb = new CefSharp.WinForms.ChromiumWebBrowser();
                wb.LoadHtml(CompileShaders(vertexCode.Text, fragmentCode.Text));
                wb.Dock = System.Windows.Forms.DockStyle.Fill;
                form.Controls.Add(wb);

                W32.SetParent(form.Handle, workerw);
            });
            // Start the Application Loop for the Form.
            System.Windows.Forms.Application.Run(form);
        }
        [Obsolete]
        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            SetWallpaperBackground();
            SendTXTBoxMsg("Success! Set wallpaper...");
        }
        private void LoadTextures()
        {
            int i = 0;
            texturesStack.Items.Clear();
            foreach (string tex in textures)
            {
                ListViewItem texture = new ListViewItem()
                {
                    Margin = new Thickness(5, 0, 10, 0),
                    ToolTip = tex,
                    BorderThickness = new Thickness(1, 1, 1, 1),
                    BorderBrush = Brushes.Gray
                };
                BitmapImage bitmapImage = new BitmapImage(new Uri(tex));
                Image tempImage = new Image()
                {
                    Source = bitmapImage,
                    Width = 100,
                    Height = 65
                };
                TextBlock tempTextBlock = new TextBlock();
                TextBlock textureName = new TextBlock()
                {
                    Text = $"u_texture{i}",
                    Margin = new Thickness(0, -25, 0, 0),
                    FontSize = 15
                };
                tempTextBlock.Inlines.Add(tempImage);
                //tempTextBlock.Inlines.Add($"u_texture{i}");
                tempTextBlock.Inlines.Add(textureName);

                texture.Content = tempTextBlock;
                texturesStack.Items.Add(texture);
                i++;
            }
        }
        private void inlineCommands_Click(object sender, RoutedEventArgs e)
        {
            cnfgs.isInterpCommand = !cnfgs.isInterpCommand;
            inlineCommands.Header = cnfgs.isInterpCommand ? "Disable tag commands" : "Enable tag commands";
            SendTXTBoxMsg(cnfgs.isInterpCommand ? "Commands can be executed through file" : "Commands through file disabled");
        }
        public static async Task<float> GetGPUUsage()
        {
            try
            {
                var category = new PerformanceCounterCategory("GPU Engine");
                var counterNames = category.GetInstanceNames();
                var gpuCounters = new List<PerformanceCounter>();
                var result = 0f;

                foreach (string counterName in counterNames)
                {
                    if (counterName.EndsWith("engtype_3D"))
                    {
                        foreach (PerformanceCounter counter in category.GetCounters(counterName))
                        {
                            if (counter.CounterName == "Utilization Percentage")
                            {
                                gpuCounters.Add(counter);
                            }
                        }
                    }
                }
                gpuCounters.ForEach(x =>
                {
                    _ = x.NextValue();
                });

                await Task.Delay(1000);

                gpuCounters.ForEach(x =>
                {
                    result += x.NextValue();
                });

                return result;
            }
            catch
            {
                return 0f;
            }
        }
        public void ListDirectory(TreeView treeView, string path)
        {
            treeView.Items.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView.Items.Add(CreateDirectoryNode(rootDirectoryInfo));
        }
        private static TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            var directoryNode = new TreeViewItem { Header = directoryInfo.Name };
            foreach (var directory in directoryInfo.GetDirectories())
                directoryNode.Items.Add(CreateDirectoryNode(directory));

            foreach (var file in directoryInfo.GetFiles())
            {
                BitmapImage bitmapImage = new BitmapImage(new Uri(@"pack://application:,,/Resources/fileTree.png"));
                Image tempImage = new Image()
                {
                    Source = bitmapImage,
                    Width = 16,
                    Height = 16
                };

                TextBlock tempTextBlock = new TextBlock();
                tempTextBlock.Inlines.Add(tempImage);
                tempTextBlock.Inlines.Add(file.Name);
                string[] fileNm = file.Name.Split('.');
                directoryNode.Items.Add(new TreeViewItem
                {
                    Header = tempTextBlock,
                    Name = fileNm[fileNm.Length - 1],
                    Tag = file.FullName
                });
            }
            return directoryNode;
        }
        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (treeView.Items.Count >= 0)
            {
                var tree = sender as TreeView;
                if (tree.SelectedItem != null)
                {
                    TreeViewItem tvi = treeView.SelectedItem as TreeViewItem;
                    try
                    {
                        string[] direc = tvi.Tag.ToString().Split('.');
                        if (direc[direc.Length - 1].ToLower() == "frag")
                        {
                            FRAGMENT_FILE = tvi.Tag.ToString();
                            TextEmpty();
                            label1.Content = $"ShadeD - {FRAGMENT_FILE}";
                            fragmentCode.Text = File.ReadAllText(tvi.Tag.ToString());
                        }
                    }
                    catch { }
                }
            }
        }
        private void isMusicLooped_Click(object sender, RoutedEventArgs e)
        {
            cnfgs.isMusicLoop = !cnfgs.isMusicLoop;
            isMusicLooped.IsChecked = (cnfgs.isMusicLoop) ? true : false;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            musicRender.NavigateToString("<html></html>");
        }
        private void genVertButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateVertsTemplate();
        }
        private void glslConsole_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                e.Handled = true;
                hist.TraverseUp(glslConsole, FRAGMENT_FILE != null ? FRAGMENT_FILE + ">" : ">");
            }
            if (e.Key == Key.Down)
            {
                e.Handled = true;
                hist.TraverseDown(glslConsole, FRAGMENT_FILE != null ? FRAGMENT_FILE + ">" : ">");
            }
        }
        private void tabGLSL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem ti = tabGLSL.SelectedItem as TabItem;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                errorTab.Foreground = ti.Name == "errorTab" ? Brushes.LightBlue : Brushes.White;
                consoleTab.Foreground = ti.Name == "consoleTab" ? Brushes.LightBlue : Brushes.White;
            }));
        }
        public void GetFileThroughMV3D(string path){
            if (File.Exists(path)){
                fragmentCode.Text = File.ReadAllText(path);
                ListDirectory(treeView, Directory.GetParent(path).FullName);
            }
        }
        private void wordWrap_Click(object sender, RoutedEventArgs e)
        {
            fragmentCode.WordWrap = !fragmentCode.WordWrap;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //Clear viewport
            GLSLRender.LoadHtml("<html></html>");
            musicRender.NavigateToString("<html></html>");
            texturesStack.Items.Clear();
            textures.Clear();
            SendTXTBoxMsg("Success! Viewport has been cleared...");
        }
        private void codeGolf_Click(object sender, RoutedEventArgs e){
            charCountGolf.Visibility = codeGolf.IsChecked?Visibility.Visible:
                Visibility.Hidden;
            charNumCount.Visibility = codeGolf.IsChecked ? Visibility.Visible :
                Visibility.Hidden;
            charNumCount.Text = codeGolf.IsChecked ? fragmentCode.Text.Count().ToString():
                "";
        }
        private void fragmentCode_TextChanged(object sender, EventArgs e)
        {
            if (codeGolf.IsChecked)
                charNumCount.Text = fragmentCode.Text.Count().ToString();
        }
        private void fragmentCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control){
                if (FRAGMENT_FILE == null)
                    SaveAsProj();
                else{
                    File.WriteAllText(FRAGMENT_FILE, fragmentCode.Text);
                    SendTXTBoxMsg($"Success! Current project '{FRAGMENT_FILE}' saved");
                }
            }
        }

        #region CONTEXT MENU FOR TEXT EDITORS
        private void MenuItem_Click_8(object sender, RoutedEventArgs e) {
            fragmentCode.Paste();
        }
        private void copyTxtBox_Click(object sender, RoutedEventArgs e) {
            fragmentCode.Copy();
        }
        private void cutTxtBox_Click(object sender, RoutedEventArgs e) {
            fragmentCode.Cut();
        }
        private void deleteTxtBox_Click(object sender, RoutedEventArgs e) {
            fragmentCode.Delete();
        }
        private void selectAllTxtBox_Click(object sender, RoutedEventArgs e) {
            fragmentCode.SelectAll();
        }
        #endregion
    }

    public class CustomMenuHandler : CefSharp.IContextMenuHandler
    {
        public void OnBeforeContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            model.Clear();
        }
        public bool OnContextMenuCommand(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            return false;
        }
        public void OnContextMenuDismissed(IWebBrowser browserControl, IBrowser browser, IFrame frame)
        {

        }
        public bool RunContextMenu(IWebBrowser browserControl, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }
    public class MyCompletionData : ICompletionData
    {
        public MyCompletionData(string text)
        {
            this.Text = text;
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public string Text { get; private set; }

        // Use this property if you want to show a fancy UIElement in the list.
        public object Content
        {
            get { return this.Text; }
        }

        public object Description
        {
            get { return "Sets a '" + this.Text + "'"; }
        }

        public double Priority => 0.3;

        public void Complete(TextArea textArea, ISegment completionSegment,
            EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, this.Text);
        }
    }
}
