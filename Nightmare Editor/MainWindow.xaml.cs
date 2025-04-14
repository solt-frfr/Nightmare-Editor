using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // "Game Archive files (*.rbin)|*.rbin|Texture files(*.ctt)|*.ctt|Texture Archive files(*.l2d)|*.l2d|Model files(*.pmo)|*.pmo|All files (*.*)|*.*";
        private TextBox selectedTextBox;
        private TextBox selectedTextBox2;
        private TextBox selectedTextBox3;

        private List<string> flaggedFiles = new List<string>();
        private List<string> flaggedFiles2 = new List<string>();
        private List<string> flaggedFiles3 = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            InfoWindow.Visibility = Visibility.Collapsed;
            List<string> Paths = new List<string>();
            // Get all files in the folder
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\");
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\");
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added");
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\");
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\User-Added");
            string[] files = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\", "*.*", SearchOption.AllDirectories);

            // Iterate and print each file path
            foreach (string file in files)
            {
                string filetrim = file.Replace($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\", "");
                AddFile(filetrim);
            }
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current");
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Game Archive files (*.rbin)|*.rbin|Texture files(*.ctt)|*.ctt|Texture Archive files(*.l2d)|*.l2d|Model files(*.pmo)|*.pmo|All files (*.*)|*.*";
            file.Title = "Select a file to open...";
            file.ShowDialog();
            var filedata = file.OpenFile;
            if (!string.IsNullOrWhiteSpace(file.FileName))
            {
                if (Path.GetExtension(file.FileName) == ".rbin")
                {
                    try
                    {
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{Path.GetFileName(file.FileName)}");
                        AddFile(Path.GetFileName(file.FileName));
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileName(file.FileName)}", true);
                        Toolkit.RbinExtract(Path.GetFileName(file.FileName));
                    }
                    catch
                    {
                        Log.Text = "You attempted to open a file that already exists. Use \"Replace\" if this was your intention.";
                    }
                }
                else if (Path.GetExtension(file.FileName) == ".ctt")
                {
                    string[] files = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\", "*.*", SearchOption.AllDirectories);
                    File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\{files.Length}-{Path.GetFileName(file.FileName)}");
                    File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{files.Length}-{Path.GetFileName(file.FileName)}");
                    Toolkit.CTTUnpack($"{files.Length}-{Path.GetFileName(file.FileName)}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\");
                }
                else if (Path.GetExtension(file.FileName) == ".pmo" || Path.GetExtension(file.FileName) == ".l2d")
                {
                    string[] files = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\", "*.*", SearchOption.AllDirectories);
                    File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\{files.Length}-{Path.GetFileName(file.FileName)}", true);
                    File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{files.Length}-{Path.GetFileName(file.FileName)}", true);
                    Toolkit.ArcUnpack($"{files.Length}-{Path.GetFileName(file.FileName)}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\");
                }
                else
                {
                    string[] files = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\", "*.*", SearchOption.AllDirectories);
                    File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\{files.Length}-{Path.GetFileName(file.FileName)}", true);
                }
                if (Path.GetExtension(file.FileName) != ".rbin" && !File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\User-Added.rbin") && !File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\User-Added.rbin"))
                {
                    File.WriteAllText($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\User-Added.rbin", "");
                    File.WriteAllText($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\User-Added.rbin", "");
                }
            }
        }

        private void AddFile(string filename)
        {
            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
            {
                Text = filename,
                IsReadOnly = true,
                Width = 200,
                Height = 20,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020"),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#424242"),
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#f2f2f2"),
                Cursor = Cursors.Hand,
                Focusable = false,
            };
            newTextBox.MouseLeftButtonUp += TextBox_Click;
            newTextBox.PreviewMouseLeftButtonDown += TextBox_PreviewMouseLeftButtonDown;
            newTextBox.PreviewMouseRightButtonDown += TextBox_PreviewMouseLeftButtonDown;
            Files.Children.Add(newTextBox);
        }

        private void AddFile2(string filename)
        {
            string zeros = "";
            for (int i =  0; i < 4 - filename.Substring(0, filename.IndexOf('-')).Length; i++)
            {
                zeros += "0";
            }
            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
            {
                Name = "file" + zeros + filename.Substring(0, filename.IndexOf('-')),
                Text = filename,
                IsReadOnly = true,
                Width = 200,
                Height = 20,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020"),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#424242"),
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#f2f2f2"),
                Cursor = Cursors.Hand,
                Focusable = false,
            };
            newTextBox.MouseLeftButtonUp += TextBox2_Click;
            newTextBox.PreviewMouseLeftButtonDown += TextBox2_PreviewMouseLeftButtonDown;
            newTextBox.PreviewMouseRightButtonDown += TextBox2_PreviewMouseLeftButtonDown;
            newTextBox.MouseRightButtonUp += TextBox2_Click;
            Files2.Children.Add(newTextBox);
        }
        private void AddFile3(string filename)
        {
            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
            {
                Name = "file" + filename.Substring(0, filename.IndexOf('.')),
                Text = filename,
                IsReadOnly = true,
                Width = 200,
                Height = 20,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020"),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#424242"),
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#f2f2f2"),
                Cursor = Cursors.Hand,
                Focusable = false,
            };
            newTextBox.MouseLeftButtonUp += TextBox3_Click;
            newTextBox.PreviewMouseLeftButtonDown += TextBox3_PreviewMouseLeftButtonDown;
            newTextBox.PreviewMouseRightButtonDown += TextBox3_PreviewMouseLeftButtonDown;
            newTextBox.MouseRightButtonUp += TextBox3_Click;
            Files3.Children.Add(newTextBox);
            if (Files3.Children.Count <= 1)
            {
                TextBox3_PreviewMouseLeftButtonDown(newTextBox, null);
                TextBox3_Click(newTextBox, null);
            }
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox = tb;
                tb.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#F04080");
                Log.Text = $"Selected: {tb.Text}";
                if (tb.Text == "User-Added.rbin")
                {
                    rex.Visibility = Visibility.Collapsed;
                    pckall1.Visibility = Visibility.Collapsed;
                    pckall2.Visibility = Visibility.Collapsed;
                    pckall3.Visibility = Visibility.Collapsed;
                    flag2.Visibility = Visibility.Collapsed;
                    flag3.Visibility = Visibility.Collapsed;
                    remove2.Visibility = Visibility.Visible;
                }
                else
                {
                    rex.Visibility = Visibility.Visible;
                    pckall1.Visibility = Visibility.Visible;
                    pckall2.Visibility = Visibility.Visible;
                    pckall3.Visibility = Visibility.Visible;
                    flag2.Visibility = Visibility.Visible;
                    flag3.Visibility = Visibility.Visible;
                    remove2.Visibility = Visibility.Collapsed;
                }
            }
            foreach (TextBox textbox in Files.Children)
            {
                if (textbox != selectedTextBox)
                    textbox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020");
            }
        }

        private void TextBox_Click(object sender, EventArgs e)
        {
            InfoWindow.Visibility = Visibility.Collapsed;
            foreach (var child in Files.Children)
            {
                Files2.Children.Clear();
                Log.Text = "Loading...";
                if (child is TextBox textBox && textBox == selectedTextBox)
                {
                    Log.Text = "pass 1";
                    if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(textBox.Text)}\"))
                    {
                        Log.Text = $"Files in {textBox.Text}";
                        List<string> Paths = new List<string>();
                        // Get all files in the folder
                        string[] files = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(textBox.Text)}\", "*.*", SearchOption.AllDirectories);

                        // Iterate and print each file path
                        foreach (string file in files)
                        {
                            string filetrim = file.Replace($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(textBox.Text)}\", "");
                            if (!filetrim.Contains("\\") && !filetrim.Contains(".bmp") && !filetrim.Contains(".png") && !filetrim.Contains(".txt"))
                                AddFile2(filetrim);
                        }
                    }
                    break;
                }
            }
            var sorted = Files2.Children
                .OfType<TextBox>()
                .OrderBy(tb => tb.Name)
                .ToList();

            Files2.Children.Clear();

            foreach (var textBox in sorted)
            {
                Files2.Children.Add(textBox);
            }
        }
        private void TextBox2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox2 = tb;
                tb.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#F04080");
                Log.Text = $"Selected: {tb.Text}";
            }
            foreach (TextBox textbox in Files2.Children)
            {
                if (textbox != selectedTextBox2)
                    textbox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020");
            }
        }

        private void TextBox2_Click(object sender, EventArgs e)
        {
            InfoWindow.Visibility = Visibility.Collapsed;
            foreach (var child in Files2.Children)
            {
                Files3.Children.Clear();
                Log.Text = "Loading...";
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    if (!textBox.Text.Contains("\\") && textBox.Text.EndsWith(".ctt"))
                    {
                        Log.Text = $"Displaying {textBox.Text}";
                        AssignImage($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{textBox.Text}", 2);
                    }
                    else if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\"))
                    {
                        Log.Text = $"Files in {textBox.Text}";
                        List<string> Paths = new List<string>();
                        // Get all files in the folder
                        string[] files = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\", "*.*", SearchOption.AllDirectories);

                        // Iterate and print each file path
                        foreach (string file in files)
                        {
                            string filetrim = file.Replace($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\", "");
                            if (!filetrim.Contains("\\") && !filetrim.Contains(".bmp") && !filetrim.Contains(".png") && !filetrim.Contains(".txt"))
                            {
                                AddFile3(filetrim);
                            }
                        }
                    }
                    else
                    {
                        Log.Text = $"The file \"{textBox.Text}\" cannot be displayed.";
                    }
                    break;
                }
            }
        }

        private void TextBox3_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox3 = tb;
                tb.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#F04080");
                Log.Text = $"Selected: {tb.Text}";
            }
            foreach (TextBox textbox in Files3.Children)
            {
                if (textbox != selectedTextBox3)
                    textbox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020");
            }
        }

        private void TextBox3_Click(object sender, EventArgs e)
        {
            foreach (var child in Files3.Children)
            {
                InfoWindow.Visibility = Visibility.Collapsed;
                Log.Text = "Loading...";
                if (child is TextBox textBox && textBox == selectedTextBox3)
                {
                    Log.Text = $"The file \"{textBox.Text}\" cannot be displayed.";
                    if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\"))
                    {
                        List<string> Paths = new List<string>();
                        // Get all files in the folder
                        string[] files = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\", "*.*", SearchOption.AllDirectories);

                        // Iterate and print each file path
                        foreach (string file in files)
                        {
                            string filetrim = file.Replace($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\", "");
                            if (!filetrim.Contains("\\") && filetrim.EndsWith(".ctt"))
                            {
                                Log.Text = $"Displaying {textBox.Text}";
                                AssignImage(file, 3);
                            }
                        }
                    }
                    break;
                }
            }
        }

        private void AssignImage(string file, int from)
        {
            InfoWindow.Visibility = Visibility.Visible;
            string file2 = "";
            if (from == 2)
            {
                string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\", $"{selectedTextBox2.Text}.*.png", SearchOption.AllDirectories);
                FileName.Text = selectedTextBox.Text + "\\" + selectedTextBox2.Text;
                file2 = files2[0];
            }
            else if (from == 3)
            {
                string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\", $"{selectedTextBox3.Text}.*.png", SearchOption.AllDirectories);
                FileName.Text = selectedTextBox.Text + "\\" + selectedTextBox2.Text + "\\" + selectedTextBox3.Text;
                file2 = files2[0];
            }
            else
            {
                return;
            }
            int startIndex = file.Length + 1;
            int endIndex = file2.IndexOf(".png");
            FileFormat.Text = file2.Substring(startIndex, endIndex - startIndex);
            BitmapImage bitmap = new BitmapImage();
            using (FileStream fs = new FileStream(file2, FileMode.Open, FileAccess.Read))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = fs;
                bitmap.EndInit();
            }
            Texture.Source = bitmap;
            FileSize.Text = bitmap.Width.ToString() + "x" + bitmap.Height.ToString();
            if (bitmap.Width < TransRights.Width || bitmap.Height < TransRights.Height)
            {
                RenderOptions.SetBitmapScalingMode(Texture, BitmapScalingMode.HighQuality);
            }
            else
            {
                RenderOptions.SetBitmapScalingMode(Texture, BitmapScalingMode.NearestNeighbor);
            }
        }

        private void RemoveFile(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox)
                {
                    File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{textBox.Text}");
                    Files.Children.Remove(textBox);
                    break;

                }
            }
        }
        private void RemoveFile2(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files2.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\{textBox.Text}");
                    Files2.Children.Remove(textBox);
                    break;

                }
            }
        }

        private void Again_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox)
                {
                    File.Copy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{textBox.Text}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{textBox.Text}", true);
                    Toolkit.RbinExtract(textBox.Text);
                }
                break;
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\"))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\";
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void OpenFolder2_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\"))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\";
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void OpenFolder3_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\"))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\";
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void Replace2_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            foreach (var child in Files2.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    if (textBox.Text.EndsWith(".ctt"))
                    {
                        file.Filter = "Texture files(*.ctt)|*.ctt";
                    }
                    else if (textBox.Text.EndsWith(".l2d"))
                    {
                        file.Filter = "Texture Archive files(*.l2d)|*.l2d";
                    }
                    else if (textBox.Text.EndsWith(".pmo"))
                    {
                        file.Filter = "Model files(*.pmo)|*.pmo";
                    }
                    else if (textBox.Text.EndsWith(".rbin"))
                    {
                        file.Filter = "Game Archive files (*.rbin)|*.rbin";
                    }
                    else
                    {
                        file.Filter = $"{Path.GetExtension(textBox.Text)} files (*{Path.GetExtension(textBox.Text)})|*{Path.GetExtension(textBox.Text)}";
                    }
                    break;
                }
            }
            file.Title = "Select a file to open...";
            file.ShowDialog();
            var filedata = file.OpenFile;
            if (!string.IsNullOrWhiteSpace(file.FileName))
            {
                try
                {
                    if (Path.GetExtension(file.FileName) == ".rbin")
                    {
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{selectedTextBox2.Text}", true);
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{selectedTextBox2.Text}", true);
                        Toolkit.RbinExtract(Path.GetFileName(selectedTextBox2.Text));
                    }
                    else if (Path.GetExtension(file.FileName) == ".ctt")
                    {
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{selectedTextBox2.Text}", true);
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{selectedTextBox2.Text}", true);
                        Toolkit.CTTUnpack(Path.GetFileName(selectedTextBox2.Text), $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\");
                    }
                    else if (Path.GetExtension(file.FileName) == ".pmo" || Path.GetExtension(file.FileName) == ".l2d")
                    {
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{selectedTextBox2.Text}", true);
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{selectedTextBox2.Text}", true);
                        Toolkit.ArcUnpack(Path.GetFileName(selectedTextBox2.Text), $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\");
                    }
                    else
                    {
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{selectedTextBox2.Text}", true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Text = ex.Message;
                }
            }
        }

        private void Pack2_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files2.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    if (textBox.Text.EndsWith(".ctt"))
                    {
                        string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\", $"{selectedTextBox2.Text}.*.png", SearchOption.AllDirectories);
                        FileName.Text = selectedTextBox.Text + "\\" + selectedTextBox2.Text;
                        string file2 = files2[0];
                        File.Copy(file2, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{textBox.Text}.{FileFormat.Text}.png", true);
                        File.Copy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{textBox.Text}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\og.ctt", true);
                        Log.Text = "Packing...";
                        Toolkit.CTTPack(textBox.Text, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\", FileFormat.Text);
                        Log.Text = $"Packed {textBox.Text}!";
                    }
                    else if ((textBox.Text.EndsWith(".l2d") || textBox.Text.EndsWith(".pmo")) && Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\"))
                    {
                        List<string> embedded = new List<string>();
                        foreach (TextBox textBox2 in Files3.Children)
                        {
                            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\";
                            embedded.Add(path + textBox2.Text);
                        }
                        NewTools.ArcPack($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{textBox.Text}", embedded, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{textBox.Text}");
                    }
                    break;
                }

            }
        }
        private void Replace3_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            foreach (var child in Files3.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox3)
                {
                    if (textBox.Text.EndsWith(".ctt"))
                    {
                        file.Filter = "Texture files(*.ctt)|*.ctt";
                    }
                    else if (textBox.Text.EndsWith(".l2d"))
                    {
                        file.Filter = "Texture Archive files(*.l2d)|*.l2d";
                    }
                    else if (textBox.Text.EndsWith(".pmo"))
                    {
                        file.Filter = "Model files(*.pmo)|*.pmo";
                    }
                    else if (textBox.Text.EndsWith(".rbin"))
                    {
                        file.Filter = "Game Archive files (*.rbin)|*.rbin";
                    }
                    else
                    {
                        file.Filter = $"{Path.GetExtension(textBox.Text)} files (*{Path.GetExtension(textBox.Text)})|*{Path.GetExtension(textBox.Text)}";
                    }
                    break;
                }
            }
            file.Title = "Select a file to open...";
            file.ShowDialog();
            var filedata = file.OpenFile;
            if (!string.IsNullOrWhiteSpace(file.FileName))
            {
                try
                {
                    if (Path.GetExtension(file.FileName) == ".rbin")
                    {
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{selectedTextBox3.Text}", true);
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{selectedTextBox3.Text}", true);
                        Toolkit.RbinExtract(Path.GetFileName(selectedTextBox3.Text));
                    }
                    else if (Path.GetExtension(file.FileName) == ".ctt")
                    {
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{selectedTextBox3.Text}", true);
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{selectedTextBox2.Text}", true);
                        Toolkit.CTTUnpack(Path.GetFileName(selectedTextBox2.Text), $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\");
                    }
                    else if (Path.GetExtension(file.FileName) == ".pmo" || Path.GetExtension(file.FileName) == ".l2d")
                    {
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{selectedTextBox3.Text}", true);
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{selectedTextBox2.Text}", true);
                        Toolkit.ArcUnpack(Path.GetFileName(selectedTextBox2.Text), $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\");
                    }
                    else
                    {
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{selectedTextBox3.Text}", true);
                    }
                }
                catch (Exception ex)
                {
                    Log.Text = ex.Message;
                }
            }
        }

        private void Pack3_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files3.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox3)
                {
                    if (textBox.Text.EndsWith(".ctt"))
                    {
                        string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\", $"{selectedTextBox3.Text}.*.png", SearchOption.AllDirectories);
                        FileName.Text = selectedTextBox.Text + "\\" + selectedTextBox2.Text + "\\" + selectedTextBox3.Text;
                        string file2 = files2[0];
                        File.Copy(file2, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{textBox.Text}.{FileFormat.Text}.png", true);
                        File.Copy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{textBox.Text}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\og.ctt", true);
                        Log.Text = "Packing...";
                        Toolkit.CTTPack(textBox.Text, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\", FileFormat.Text);
                        Log.Text = $"Packed {textBox.Text}!";
                    }
                    else if ((textBox.Text.EndsWith(".l2d") || textBox.Text.EndsWith(".pmo")) && Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\"))
                    {
                        List<string> embedded = new List<string>();
                        foreach (TextBox textBox2 in Files3.Children)
                        {
                            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\";
                            embedded.Add(path + textBox2.Text);
                        }
                        NewTools.ArcPack($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{textBox.Text}", embedded, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{textBox.Text}");
                    }
                    break;
                }
            }
        }
        private void Flag2(object sender, RoutedEventArgs e)
        {
            string file1 = $@"{selectedTextBox.Text}";
            string file2 = $@"{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{selectedTextBox2.Text}";
            bool isin = false;
            if (!flaggedFiles.Contains(file1) && !selectedTextBox.Text.Contains("User-Added.rbin"))
            {
                flaggedFiles.Add(file1);
            }
            if (!flaggedFiles2.Contains(file2))
            {
                flaggedFiles2.Add(file2);
            }
            else
            {
                foreach (string fileref in flaggedFiles3)
                {
                    if (fileref.Contains($@"{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\"))
                    {
                        isin = true;
                        break;
                    }
                }
                if (isin)
                {
                    Log.Text = "You cannot unflag this file, as another file depends on it.";
                }
                else
                {
                    flaggedFiles2.Remove(file2);
                }
            }
            isin = false;
            foreach (string fileref in flaggedFiles2)
            {
                if (fileref.Contains($@"{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\"))
                {
                    isin = true;
                    break;
                }
            }
            if (!isin)
            {
                Log.Text = $"No flagged file references {selectedTextBox.Text}, removing from flagged list.";
                flaggedFiles.Remove(file1);
            }

        }
        private void Flag3(object sender, RoutedEventArgs e)
        {
            string file1 = $@"{selectedTextBox.Text}";
            string file2 = $@"{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{selectedTextBox2.Text}";
            string file3 = $@"{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{selectedTextBox3.Text}";
            if (!flaggedFiles.Contains(file1) && !selectedTextBox.Text.Contains("User-Added.rbin"))
            {
                flaggedFiles.Add(file1);
            }
            if (!flaggedFiles2.Contains(file2))
            {
                flaggedFiles2.Add(file2);
            }
            if (!flaggedFiles3.Contains(file3))
            {
                flaggedFiles3.Add(file3);
            }
            else
            {
                flaggedFiles3.Remove(file3);
            }
        }
        private void PackAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}