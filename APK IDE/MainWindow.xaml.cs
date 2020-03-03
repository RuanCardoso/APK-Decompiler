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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Security.Principal;
using System.Windows.Threading;

namespace APK_IDE
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly OpenFileDialog SelectAPK = new OpenFileDialog(); // Create dialog box to select APK;

        private readonly System.Windows.Forms.FolderBrowserDialog SelectUF = new System.Windows.Forms.FolderBrowserDialog(); // create dialog box to select UnityFile;
        private readonly System.Windows.Forms.FolderBrowserDialog SelectFolder = new System.Windows.Forms.FolderBrowserDialog(); // Create dialog box to select Folder to recompile;

        private readonly Process p_Proc = new Process(); // create new process to run execute CMD arguments;
        private readonly ProcessStartInfo p_Info = new ProcessStartInfo(); // Create arguments to pass to p_Proc;
        private readonly BackgroundWorker p_Work = new BackgroundWorker(); // Create work in background plan;

        FileInfo infor; // Pass arguments FileInfor of drag and drop filer;
        DirectoryInfo inforDir; // pass arguments DirectoryInfor of drag an drop folder;

        FileSystemWatcher folder_Watcher = new FileSystemWatcher(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\APK IDE Debug\\Decompiled APKS"));
        FileSystemWatcher apk_Watcher = new FileSystemWatcher(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\APK IDE Debug\\Recompiled APKS"));

        private int w_index; // Operation to start in p_Work;
        private readonly string ToolsDir; // Directory contains tools neccesary of this program;
        private string s_Output; // Output of operations;
        private string ManualAPKName = "APK3.apk";

        private string RecentFileName;
        private string RecentDirName;

        private bool isLocal = false;
        private bool isSmartRecompile = true;
        private bool isSmartSign = true;

        public MainWindow()
        {
            InitializeComponent();
            if (CheckAdministrator() == false)
            {
                ToolsDir = string.Concat(Environment.CurrentDirectory, "\\APK IDE Tools"); // Concating "ToolsDir" with default directory of tools this program;
                CheckPathAndCreate(); // Create default directory in my documents;

                p_Work.DoWork += GetBackgroundWorker_DoWork; // event p_Work start operation;
                p_Work.RunWorkerCompleted += GetBackgroundWorker_RunWorkerCompleted; // event p_Work finish operation;

                ////Watcher////
                folder_Watcher.EnableRaisingEvents = true;
                folder_Watcher.Created += Folder_Watcher_Created;
                folder_Watcher.Changed += Folder_Watcher_Changed;

                apk_Watcher.EnableRaisingEvents = true;
                apk_Watcher.Created += Apk_Watcher_Created;
                apk_Watcher.Changed += Apk_Watcher_Changed;
                ///////////......

                MessageBox.Show("Beta Version! Bugs??? Send to my Gmail, with are descriptions of bug \"cardoso.ruan050322@gmail.com\"", "Version 0.1 Beta", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show("Run as administrator not suported", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private void Apk_Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!Directory.Exists(e.FullPath))
            {
                RecentFileName = e.FullPath;
            }
        }

        private void Apk_Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (!Directory.Exists(e.FullPath))
            {
                RecentFileName = e.FullPath;
            }
        }

        private void Folder_Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                RecentDirName = e.FullPath;
            }
        }

        private void Folder_Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                RecentDirName = e.FullPath;
            }
        }

        private void UpdateOutput()
        {
            StringBuilder RFN = new StringBuilder(string.Format("[FIL]-> {0}", RecentFileName));
            RFN.Replace(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "");

            StringBuilder RDN = new StringBuilder(string.Format("[DIR]-> {0}", RecentDirName));
            RDN.Replace(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "");

            FileN.Content = RFN;
            DirN.Content = RDN;
        }

        private bool CheckAdministrator()
        {
            WindowsPrincipal p_Window = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return p_Window.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void StartWork(int i_index) // Start operation p_Work with i_index;
        {
            w_index = i_index; // Pass arguments i_Index to w_Index;

            if (!p_Work.IsBusy)
            {
                p_Work.RunWorkerAsync();
            }
        }

        private void SignAPK() // Sign APK after Compilation;
        {
            StartWork(2); // Start sign based w_index << i_Index;
        }

        private void CheckPathAndCreate() // Create default directory does not exist;
        {
            if (!Directory.Exists(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\APK IDE Debug")))
            {
                Directory.CreateDirectory(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\APK IDE Debug"));
                MessageBox.Show("Configuring Files!");
                CreateSubDir();
                MessageBox.Show("Ready to Use after of Restart aplication, click ok and reopen application!");
                Application.Current.Shutdown();
            }
        }

        private void CreateSubDir() // Create Sub-Directories;
        {
            Directory.CreateDirectory(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\APK IDE Debug\\Decompiled APKS"));
            Directory.CreateDirectory(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "\\APK IDE Debug\\Recompiled APKS"));
        }

        private bool ConvertManualToAutoName()
        {
            if (AutoName.IsChecked == true)
            {
                ManualAPKName = SelectAPK.SafeFileName;

                if (!File.Exists(SelectAPK.FileName)) // if exist apk......
                {
                    MessageBox.Show("Select Original APK File", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if (NameAPKAfterDec.Text.Contains(".apk"))
                {
                    if (!File.Exists(SelectAPK.FileName)) // if exist apk......
                    {
                        MessageBox.Show("Select Original APK File", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return false;
                    }
                    else
                    {
                        ManualAPKName = NameAPKAfterDec.Text;
                        return true;
                    }
                }
                else
                {

                    if (!File.Exists(SelectAPK.FileName)) // if exist apk......
                    {
                        MessageBox.Show("Select Original APK File", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        return false;
                    }
                    else
                    {
                        NameAPKAfterDec.AppendText(".apk");
                        ManualAPKName = NameAPKAfterDec.Text;
                        return true;
                    }
                }
            }
        }

        private void SelectApkDialog() // Open Dialog SelectAPK;
        {
            SelectAPK.Filter = "APK Files (*.apk)|*.apk";

            if (SelectAPK.ShowDialog() == true)
            {
                SelectedAPKName.Content = SelectAPK.SafeFileName; // Label get FileName;
            }
        }

        private void Descompile() // Start decompilation;
        {
            if (ConvertManualToAutoName() == true)
            {
                Output.Text = "[Decompilation Started]\n[Waiting.......]\n\n"; // Output append informations of progress;

                ProgressBarPB.IsIndeterminate = true; // progress bar time inderteminate of operation;
                StartWork(0); // start p_Work based w_index << i_Index;
            }
        }

        private void GetBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null) // if errors?
            {
                Output.AppendText("[CODE:] 0x0 \n");
            }
            else
            {
                ProgressBarPB.IsIndeterminate = false; //.....................
                ProgressBarPB.Value = 100; // Operation completed progressbar full;

                if (w_index != 0) // Its not decompilation.....
                {
                    if (IsSign.IsChecked == true && w_index == 1) // Call methods to start, based and Checkbox(UI) and w_Index;
                    {
                        AnErrorOrSuccess("Recompiled with Success\n", "Error occurred in Recompilation", true);
                        SignAPK();
                    }
                    else if (w_index == 2) // Operation check signed have started.........
                    {
                        CheckSigned();
                    }
                    else if (w_index == 3) // End operation signed, check if sign is valid?..............
                    {
                        AnErrorOrSuccess("Signer checked with success", "Error occurred in check signer, disable IsLocal, if actived!", true);
                    }
                    else
                    {
                        AnErrorOrSuccess("Recompiled with Success, more Unsigned APK", "Error occurred in Recompilation", true);
                    }
                }
                else
                {
                    AnErrorOrSuccess("Decompiled with success", "Erro in decompilation", true);
                }
            }
            UpdateOutput();
        }

        private void AnErrorOrSuccess(string Sucess, string Error, bool OutputDetail = false)
        {
            if (s_Output.Length > 1)
            {
                if (OutputDetail)
                {
                    Output.AppendText(s_Output + "\n" + $"Code: [0x0000000F1] {Sucess}");
                }
                else
                {
                    Output.AppendText($"Code: [0x0000000F1] {Sucess}");
                }
            }
            else
            {
                Output.AppendText($"Code: [0x0000000F0] {Error}");
            }
        }

        private void GetBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // based on in w_index start operation;
            if (w_index == 0) // Decompile;
            {
                CMD_Start(p_Proc, "cmd.exe", false, true, true, true, true, "/c", p_Info, string.Format("cd {0}", ToolsDir), string.Format("apktool d -f \"{0}\" -o \"{1}\"", SelectAPK.FileName, string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), string.Format("\\APK IDE Debug\\Decompiled APKS\\{0}", ManualAPKName.Replace(".apk", ""))))); // Decompile
            }
            else if (w_index == 1) // Recompile;
            {
                if (isSmartRecompile == true)
                {
                    CMD_Start(p_Proc, "cmd.exe", false, true, true, true, true, "/c", p_Info, string.Format("cd {0}", ToolsDir), string.Format("apktool b -f \"{0}\" -o \"{1}\"", RecentDirName, string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), string.Format("\\APK IDE Debug\\Recompiled APKS\\{0}", ManualAPKName)))); //Recompile
                }
                else
                {
                    CMD_Start(p_Proc, "cmd.exe", false, true, true, true, true, "/c", p_Info, string.Format("cd {0}", ToolsDir), string.Format("apktool b -f \"{0}\" -o \"{1}\"", SelectFolder.SelectedPath, string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), string.Format("\\APK IDE Debug\\Recompiled APKS\\{0}", ManualAPKName)))); //Recompile
                }
            }
            else if (w_index == 2) // Start Sign APK;
            {
                if (isSmartSign)
                {
                    CMD_Start(p_Proc, "cmd.exe", false, true, true, true, true, "/c", p_Info, string.Format("cd {0}", ToolsDir), string.Format("apksigner sign --ks mrt.keystore --ks-key-alias alias_name --ks-pass pass:Beta18 --key-pass pass:Beta18 \"{0}\"", RecentFileName));
                }
                else
                {
                    CMD_Start(p_Proc, "cmd.exe", false, true, true, true, true, "/c", p_Info, string.Format("cd {0}", ToolsDir), string.Format("apksigner sign --ks mrt.keystore --ks-key-alias alias_name --ks-pass pass:Beta18 --key-pass pass:Beta18 \"{0}\"", string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), string.Format("\\APK IDE Debug\\Recompiled APKS\\{0}", ManualAPKName))));
                }
            }
            else if (w_index == 3) // Check Sign is Valid;
            {
                if (isLocal)
                {
                    if (isSmartSign)
                    {
                        CMD_Start(p_Proc, "cmd.exe", false, true, true, true, true, "/c", p_Info, string.Format("cd {0}", ToolsDir), string.Format("apksigner verify --verbose \"{0}\"", RecentFileName)); // Check Sign
                    }
                    else
                    {
                        CMD_Start(p_Proc, "cmd.exe", false, true, true, true, true, "/c", p_Info, string.Format("cd {0}", ToolsDir), string.Format("apksigner verify --verbose \"{0}\"", string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), string.Format("\\APK IDE Debug\\Recompiled APKS\\{0}", ManualAPKName)))); // Check Sign
                    }
                }
                else
                {
                    CMD_Start(p_Proc, "cmd.exe", false, true, true, true, true, "/c", p_Info, string.Format("cd {0}", ToolsDir), string.Format("apksigner verify --verbose \"{0}\"", SelectAPK.FileName)); // Check Sign
                }
            }
        }

        void CMD_Start(Process proc, string fileName, bool useShell, bool NoWindow, bool RedirectOutput, bool RedirectError, bool output, string key, ProcessStartInfo info, params string[] Arguments)
        {
            info.FileName = fileName; //........
            info.UseShellExecute = useShell; //.......
            info.CreateNoWindow = NoWindow;
            info.RedirectStandardOutput = RedirectOutput;
            info.RedirectStandardError = RedirectError;

            info.Arguments = key + string.Join("&", Arguments);

            proc.StartInfo = info;
            //proc.PriorityClass = ProcessPriorityClass.RealTime;
            proc.Start();

            if (output) //.....
            {
                s_Output = proc.StandardOutput.ReadToEnd();
            }
        }

        private void Recompile() // Start recompilation;
        {
            try
            {
                if (isSmartRecompile)
                {
                    if (!File.Exists(string.Concat(RecentDirName, "\\apktool.yml")))
                    {
                        MessageBox.Show("Smart decompile is actived, its necessary decompile only once", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    if (!File.Exists(string.Concat(SelectFolder.SelectedPath, "\\apktool.yml")))
                    {
                        MessageBox.Show("This Directory is not compatible with this tool, select a directory decompiled with this tool!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (ConvertManualToAutoName() == true)
                {
                    Output.Text = "[Operation Started]\n[Waiting.......]\n\n";
                    ProgressBarPB.IsIndeterminate = true;
                    StartWork(1);
                }
            }
            catch
            {
                MessageBox.Show("Directory Invalid");
            }
        }

        private void SelectFolderDialog()
        {
            SelectFolder.ShowNewFolderButton = false;
            SelectFolder.RootFolder = Environment.SpecialFolder.MyDocuments;
            if (SelectFolder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedFolderAPK.Content = SelectFolder.SelectedPath;
            }
        }

        private void CheckSigned() // Check if APK there signed;
        {
            StartWork(3);
        }

        private void StartHxD() // Inicia HxD
        {
            Hex_Form HF = new Hex_Form();
            if (HF.ShowDialog() == true)
            {
                //open
            }
        }

        private void GetUnityGameVersion() // Get Unity Game Version, to be used in IL2CPP Dumper;
        {
            SelectUF.RootFolder = Environment.SpecialFolder.MyDocuments;

            if (SelectUF.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Output.Text = string.Empty;
                string[] Files = Directory.GetFiles(SelectUF.SelectedPath, "*.*", SearchOption.AllDirectories);

                if (!Files.Any(x => x.Contains("unity_builtin_extra")))
                {
                    MessageBox.Show("Version of Unity Game Not Detected!");
                    return;
                }

                for (int i = 0; i < Files.Length; i++)
                {
                    if (Files[i].Contains("unity_builtin_extra"))
                    {
                        string VersionUnityGame = File.ReadLines(Files[i], Encoding.UTF8).ToList()[0];
                        IEnumerable<char> VersionToNumber = VersionUnityGame.Where(x => char.IsLetterOrDigit(x) || char.IsPunctuation(x));

                        foreach (var t in VersionToNumber)
                        {
                            Output.Text += t;
                        }

                        Output.AppendText(" This game is there version! Automatically send to IL2CPP Dumper");
                    }
                }
                MessageBox.Show("Take a few moments!");
            }
        }

        private void Output_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SelectAPK_MDC_Event(object sender, MouseButtonEventArgs e)
        {
            SelectApkDialog();
        }

        private void SelectFolder_MDC_Event(object sender, MouseButtonEventArgs e)
        {
            if (!isSmartRecompile)
            {
                SelectFolderDialog();
            }
        }

        private void MLB_Decompile_Event(object sender, MouseButtonEventArgs e)
        {
            Descompile();
        }

        private void SelectAPKName_Drop_Event(object sender, DragEventArgs e)
        {
            string[] eData = (string[])e.Data.GetData(DataFormats.FileDrop);
            infor = new FileInfo(eData[0]);

            if (eData.Length == 1)
            {
                if (infor.Extension == ".apk")
                {
                    SelectAPK.FileName = infor.FullName;
                    SelectedAPKName.Content = infor.Name;
                }
                else
                {
                    MessageBox.Show("Select a APK File!");
                }
            }
            else
            {
                MessageBox.Show("Multiples Files not suported! suported comming soon!");
            }
        }

        private void MLB_Recompile_Event(object sender, MouseButtonEventArgs e)
        {
            Recompile();
        }

        private void Drop_Recompile_Event(object sender, DragEventArgs e)
        {
            string[] eData = (string[])e.Data.GetData(DataFormats.FileDrop);
            inforDir = new DirectoryInfo(eData[0]);

            if (!isSmartRecompile)
            {
                if (eData.Length == 1)
                {
                    SelectFolder.SelectedPath = inforDir.FullName;

                    if (!File.Exists(string.Concat(SelectFolder.SelectedPath, "\\apktool.yml")))
                    {
                        MessageBox.Show("This directory not compatible with this tool");
                        return;
                    }
                    SelectedFolderAPK.Content = inforDir.Name;
                }
                else
                {
                    MessageBox.Show("Multiples Folders not suported! suported comming soon!");
                }
            }
        }

        private void OpenHxD_Event(object sender, MouseButtonEventArgs e)
        {
            StartHxD();
        }

        private void UnityVersion_Event(object sender, MouseButtonEventArgs e)
        {
            GetUnityGameVersion();
        }

        private void CheckSign_MLB_Event(object sender, MouseButtonEventArgs e)
        {
            Output.Text = string.Empty;
            if (!File.Exists(SelectAPK.FileName))
            {
                MessageBox.Show("Select a APK File", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            if (ConvertManualToAutoName() == true)
            {
                CheckSigned();
                MessageBox.Show("Take a few moments.....");
            }
        }

        private void Sher_But_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Comming Soon");
        }

        private void AutoName_Checked(object sender, RoutedEventArgs e)
        {
            NameAPKAfterDec.IsReadOnly = true;
            NameAPKAfterDec.Text = "Automatic Name";
        }

        private void AutoName_Unchecked(object sender, RoutedEventArgs e)
        {
            NameAPKAfterDec.IsReadOnly = false;
            NameAPKAfterDec.Text = "Name + Ext(.apk)";
        }

        private void IsLocal_Unchecked(object sender, RoutedEventArgs e)
        {
            isLocal = false;
        }

        private void IsLocal_Checked(object sender, RoutedEventArgs e)
        {
            isLocal = true;
        }

        private void SmartRecompile_Checked(object sender, RoutedEventArgs e)
        {
            isSmartRecompile = true;
            SelectedFolderAPK.Content = "Smart Recompile is actived!";
        }

        private void SmartRecompile_Unchecked(object sender, RoutedEventArgs e)
        {
            isSmartRecompile = false;
            SelectedFolderAPK.Content = "Drag and Drop or Double Click";
        }

        private void SmartSign_Checked(object sender, RoutedEventArgs e)
        {
            isSmartSign = true;
        }

        private void SmartSign_Unchecked(object sender, RoutedEventArgs e)
        {
            isSmartSign = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            p_Work.WorkerSupportsCancellation = true;
            if (p_Work.IsBusy)
            {
                p_Work.CancelAsync();
            }
        }

        private void Open_DnSpy_UAC_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(Environment.CurrentDirectory + "\\DnSpy\\dnSpy.exe"))
            {
                if (File.Exists(RecentDirName + @"\assets\bin\Data\Managed\Assembly-CSharp.dll"))
                {
                    CMD_Start(p_Proc, "cmd.exe", false, true, true, true, true, "/c", p_Info, string.Format("cd {0}", Environment.CurrentDirectory + "\\DnSpy"), string.Format("start DnSpy.exe \"{0}\"", RecentDirName + @"\assets\bin\Data\Managed\Assembly-CSharp.dll")); // Start DnSpy;
                }
                else
                {
                    MessageBox.Show("Descompile Unity Game to send DnSpy modification!");
                }
            }
            else
            {
                MessageBox.Show("Could not locate DnSpy");
            }
        }

        private void Open_Directory_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", RecentDirName);
        }

        private void Open_File_Def_Emulator_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(RecentFileName);
            }
            catch { }
        }

        private void Delete_File_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                File.Delete(RecentFileName);
            }
            catch { }
        }

        private void Delete_Directory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.Delete(RecentDirName, true);
            }
            catch { }
        }

        private void Install_APK_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MouseLeftDown_Move(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch { }
        }
    }
}
