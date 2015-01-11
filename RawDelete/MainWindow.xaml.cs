using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RawDelete
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DirectoryInfo _baseDir;
        private string _rawFileType;

        private delegate void ExecutingEventHandler(string message);

        private event ExecutingEventHandler OnExecuting;

        public MainWindow()
        {
            InitializeComponent();
            this.OnExecuting += message => Dispatcher.Invoke(() =>
            {
                TextStatus.Text = message;
            });
        }

        private void ChooseFolder_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                CheckFolder(fbd.SelectedPath);
            }
        }

        private void CheckFolder(string path)
        {
            if (!Directory.Exists(path))
                return;

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                FolderPath.Text = path;
                _baseDir = new DirectoryInfo(path);

                var rawFormats = new string[]
                    {
                        "3fr", "ari", "arw", "bay", "crw", "cr2", "cap", "dcs", "dcr", "dng", "drf", "eip",
                        "erf", "fff", "iiq", "k25", "kdc", "mdc", "mef", "mos", "mrw", "nef", "nrw", "obm",
                        "orf", "pef", "ptx", "pxn", "r3d", "raf", "raw", "rwl", "rw2", "rwz", "sr2", "srf",
                        "srw", "x3f"
                    };

                string[] files = Directory.GetFiles(_baseDir.FullName);
                foreach (var file in files)
                {
                    if (file.EndsWith("jpg", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (file.EndsWith("db", StringComparison.OrdinalIgnoreCase))
                        continue;

                    foreach (var rawFormat in rawFormats)
                    {
                        if (file.EndsWith(rawFormat, StringComparison.OrdinalIgnoreCase))
                        {
                            RawFileType.Text = rawFormat;
                            Mouse.OverrideCursor = null;
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TextStatus.Text = String.Format("Fehler: {0}", ex.Message);
            }
            Mouse.OverrideCursor = null;
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            try
            {
                Mouse.OverrideCursor = Cursors.AppStarting;
                _rawFileType = RawFileType.Text;
                await ExecuteAsync();
            }
            catch (Exception ex)
            {
                TextStatus.Text = String.Format("Fehler: {0}", ex.Message);
            }
            Mouse.OverrideCursor = null;
            this.IsEnabled = true;
        }

        private async Task ExecuteAsync()
        {
            await Task.Run(() =>
            {
                if (!String.IsNullOrWhiteSpace(_rawFileType) && _baseDir != null)
                {
                    string[] files = Directory.GetFiles(_baseDir.FullName);

                    int counter = 1;
                    foreach (var file in files)
                    {
                        OnExecuting(String.Format("{0}% Analysiere {1}", (int)((double)100 / files.Length * counter), file));
                        if (file.EndsWith(String.Format(".{0}", _rawFileType), StringComparison.OrdinalIgnoreCase))
                        {
                            var jpgFileNameLC = file.Substring(0, file.Length - 3) + "jpg";
                            var jpgFileNameUP = file.Substring(0, file.Length - 3) + "JPG";
                            if (!files.Contains(jpgFileNameLC) && !files.Contains(jpgFileNameUP))
                            {
                                var fi = new FileInfo(file);

                                if (!Directory.Exists(_baseDir.FullName + @"\_RawDelete"))
                                {
                                    Directory.CreateDirectory(_baseDir.FullName + @"\_RawDelete");
                                }
                                OnExecuting(String.Format("{0}% Verschiebe {1}", ((double)100 / files.Length * counter), file));
                                File.Move(file, _baseDir.FullName + @"\_RawDelete\" + fi.Name);
                                //Give the filesystem some time to move the file
                                Thread.Sleep(100);
                            }
                        }
                        counter++;
                    }
                    OnExecuting("Fertig!");
                }
                else
                {
                    OnExecuting("Keinen Ordner ausgewählt oder keinen Raw-Dateinamen definiert.");
                } 
            });
        }

        private void FolderPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckFolder(FolderPath.Text);
        }

        private void FolderPath_OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (FolderPath.Text == "Oder Pfad zum Ordner hier eintragen...")
                FolderPath.Text = "";
        }

        private void FolderPath_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(FolderPath.Text))
                FolderPath.Text = "Oder Pfad zum Ordner hier eintragen...";
        }
    }
}
