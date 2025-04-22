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
using System.Diagnostics;
using Nightmare_Editor.NewTools;
using Newtonsoft;
using System.Text.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using SevenZip;
using Pulsar;


namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : Window
    {
        // "Game Archive files (*.rbin)|*.rbin|Texture files(*.ctt)|*.ctt|Texture Archive files(*.l2d)|*.l2d|Effect Files(*.fep)|*.fep|Model files(*.pmo)|*.pmo|Map files(*.pmp)|*.pmp|All files (*.*)|*.*";
        private TextBox selectedTextBox;
        private TextBox selectedTextBox2;
        private TextBox selectedTextBox3;

        private List<string> flaggedFiles = new List<string>();
        private List<string> flaggedFiles2 = new List<string>();
        private List<string> flaggedFiles3 = new List<string>();

        private bool windowSwap = false;
        private bool windowStore = false;

        private bool textureSwap = false;

        private List<string> allfiles = new List<string>();

        private List<string[]> textureLinks = new List<string[]>();

        private readonly string linkPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\textures.json";

        private List<TextBox> unfiltered = new List<TextBox>();

        public Editor()
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
            if (!File.Exists(linkPath))
            {
                QuickJson(true);
            }
            QuickJson(false);
        }

        private void QuickJson(bool write)
        {
            if (write)
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                TextureList texturelist = new TextureList();
                texturelist.Textures = textureLinks;
                string jsonString = JsonSerializer.Serialize<TextureList>(texturelist, jsonoptions);
                File.WriteAllText(linkPath, jsonString);
            }
            else
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = File.ReadAllText(linkPath);
                textureLinks = JsonSerializer.Deserialize<TextureList>(jsonString, jsonoptions).Textures;
            }
        }

        public static void BetterDirCopy(string sourceDir, string destDir, bool delete)
        {
            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                BetterDirCopy(dir, destSubDir, false);
            }
            if (delete)
            {
                Directory.Delete(sourceDir, true);
            }
        }

        private void FileOpen_Click(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current");
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Game Archive files (*.rbin)|*.rbin|Texture files(*.ctt)|*.ctt|Texture Archive files(*.l2d)|*.l2d|Effect Files(*.fep)|*.fep|Model files(*.pmo)|*.pmo|Map files(*.pmp)|*.pmp|All files (*.*)|*.*";
            file.Title = "Select a file to open...";
            file.ShowDialog();
            var filedata = file.OpenFile;
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\User-Added");
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added");
            File.WriteAllText($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\User-Added.rbin", "");
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
                    File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\{files.Length}-{Path.GetFileName(file.FileName)}", true);
                    CTT.Decode($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\{files.Length}-{Path.GetFileName(file.FileName)}");
                }
                else if (Path.GetExtension(file.FileName) == ".pmo" || Path.GetExtension(file.FileName) == ".l2d" || Path.GetExtension(file.FileName) == ".fep" || Path.GetExtension(file.FileName) == ".pmp")
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
                if (Path.GetExtension(file.FileName) != ".rbin" && !File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\User-Added.rbin"))
                {
                    File.WriteAllText($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\User-Added.rbin", "");
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
            string zeros = "";
            for (int i = 0; i < 4 - filename.Substring(0, filename.IndexOf('.')).Length; i++)
            {
                zeros += "0";
            }
            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
            {
                Name = "file" + zeros + filename.Substring(0, filename.IndexOf('.')),
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
                    remove.Visibility = Visibility.Collapsed;
                    replace.Visibility = Visibility.Collapsed;
                    flag2.Visibility = Visibility.Collapsed;
                    flag3.Visibility = Visibility.Collapsed;
                    remove2.Visibility = Visibility.Visible;
                }
                else
                {
                    rex.Visibility = Visibility.Visible;
                    remove.Visibility = Visibility.Visible;
                    replace.Visibility = Visibility.Visible;
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
            try
            {
                Files2_Scroll.ScrollToTop();
            }
            catch { }
            Sort.SelectedIndex = 0;
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
                            if (!filetrim.Contains("\\") && !filetrim.Contains(".bmp") && !filetrim.Contains(".png") && !filetrim.Contains(".txt") && !filetrim.Contains(".pnt"))
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
            unfiltered.Clear();

            foreach (var textBox in sorted)
            {
                Files2.Children.Add(textBox);
                unfiltered.Add(textBox);
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
            try
            {
                Files3_Scroll.ScrollToTop();
            }
            catch { }
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
                            if (!filetrim.Contains("\\") && !filetrim.Contains(".bmp") && !filetrim.Contains(".png") && !filetrim.Contains(".txt") && !filetrim.Contains(".pnt"))
                            {
                                AddFile3(filetrim);
                            }
                        }
                        var sorted = Files3.Children
                            .OfType<TextBox>()
                            .OrderBy(tb => tb.Name)
                            .ToList();

                        Files3.Children.Clear();
                        foreach (var textBox2 in sorted)
                        {
                            Files3.Children.Add(textBox2);
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
            if (Path.GetFileName(file) == selectedTextBox2.Text || Path.GetFileName(file) == selectedTextBox3.Text)
            {
                InfoWindow.Visibility = Visibility.Visible;
                string file2 = "";
                string truefile2 = "";
                try
                {
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
                    byte[] buffer = new byte[0x80];
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                    {
                        fs.Read(buffer, 0, buffer.Length);
                    }
                    byte formatByte = buffer[0x1C]; // read byte from file
                    NewTools.CTT.Format formatenum = (NewTools.CTT.Format)formatByte;
                    int startIndex = file.Length + 1;
                    int endIndex = truefile2.IndexOf(".png");
                    string format = formatenum.ToString();
                    FileFormat.Text = format;
                    truefile2 = file + $".{format}.png";
                    if (file2 != truefile2)
                    {
                        if (format == "ETC1" || format == "ETC1A4")
                        {
                            try
                            {
                                File.Move(file2, truefile2);
                                File.Delete(file2.Replace(".png", ".bmp"));
                            }
                            catch { }
                        }
                        else
                        {
                            try
                            {
                                File.Delete(file2);
                                NewTools.CTT.Decode(file);
                                File.Delete(file2.Replace(".png", ".bmp"));
                            }
                            catch { }
                        }
                    }
                    BitmapImage bitmap = new BitmapImage();
                    BitmapImage bitmap2 = new BitmapImage();
                    bool found = false;
                    using (FileStream fs = new FileStream(truefile2, FileMode.Open, FileAccess.Read))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = fs;
                        bitmap.EndInit();
                    }
                    if (textureSwap)
                    {
                        Texture.Source = bitmap;
                    }
                    else
                    {
                        TextureSmall.Source = bitmap;
                    }
                    FileSize.Text = bitmap.PixelWidth.ToString() + "x" + bitmap.PixelHeight.ToString();
                    FileLink.Text = truefile2;
                    foreach (var arr in textureLinks)
                    {
                        if (arr.Length >= 2 && arr[0] == FileName.Text)
                        {
                            using (FileStream fs = new FileStream(arr[1], FileMode.Open, FileAccess.Read))
                            {
                                bitmap2.BeginInit();
                                bitmap2.CacheOption = BitmapCacheOption.OnLoad;
                                bitmap2.StreamSource = fs;
                                bitmap2.EndInit();
                            }
                            if (textureSwap)
                            {
                                TextureSmall.Source = bitmap2;
                            }
                            else
                            {
                                Texture.Source = bitmap2;
                            }
                            FileLink.Text = arr[1];
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        if (textureSwap)
                        {
                            TextureSmall.Source = bitmap;
                        }
                        else
                        {
                            Texture.Source = bitmap;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("The .ctt file selected has no picture associated with it. Unpacking...");
                    NewTools.CTT.Decode(file);
                    AssignImage(file, from);
                }
            }
        }

        private void RemoveFile(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox)
                {
                    if (MessageBox.Show($"Do you wish to delete {textBox.Text}?",
                    $"Delete {textBox.Text}",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(textBox.Text)}", true);
                        Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\{Path.GetFileNameWithoutExtension(textBox.Text)}", true);
                        File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{textBox.Text}");
                        Files.Children.Remove(textBox);
                        break;
                    }
                }
            }
        }
        private void RemoveFile2(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files2.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    if (MessageBox.Show($"Do you wish to delete {textBox.Text}?",
                    $"Delete {textBox.Text}",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\{Path.GetFileNameWithoutExtension(textBox.Text)}"))
                            Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\{Path.GetFileNameWithoutExtension(textBox.Text)}", true);
                        File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\{textBox.Text}");
                        Files2.Children.Remove(textBox);
                        break;
                    }

                }
            }
        }

        private void Again_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in Files.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox)
                {
                    if (MessageBox.Show($"Do wish to extract {textBox.Text} again? This will replace all files inside.",
                    $"Re-Extract {textBox.Text}",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        File.Copy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{textBox.Text}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{textBox.Text}", true);
                        Toolkit.RbinExtract(textBox.Text);
                    }
                    break;
                }
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{selectedTextBox.Text}"))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\";
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void OpenFolder2_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{selectedTextBox2.Text}"))
            {
                string file = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{selectedTextBox2.Text}";
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = "explorer.exe";
                StartInformation.Arguments = $"/select,\"{file}\""; 
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void OpenFolder3_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{selectedTextBox3.Text}"))
            {
                string file = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{selectedTextBox3.Text}";
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = "explorer.exe";
                StartInformation.Arguments = $"/select,\"{file}\"";
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Game Archive files (*.rbin)|*.rbin";
            file.Title = "Select a file to open...";
            file.ShowDialog();
            var filedata = file.OpenFile;
            File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{selectedTextBox.Text}", true);
            File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{selectedTextBox.Text}", true);
            Toolkit.RbinExtract(Path.GetFileName(selectedTextBox.Text));
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
                    else if (textBox.Text.EndsWith(".fep"))
                    {
                        file.Filter = "Effect files(*.fep)|*.fep";
                    }
                    else if (textBox.Text.EndsWith(".pmo"))
                    {
                        file.Filter = "Model files(*.pmo)|*.pmo";
                    }
                    else if (textBox.Text.EndsWith(".pmp"))
                    {
                        file.Filter = "Map files(*.pmp)|*.pmp|";
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
                    else if (Path.GetExtension(file.FileName) == ".pmo" || Path.GetExtension(file.FileName) == ".l2d" || Path.GetExtension(file.FileName) == ".fep" || Path.GetExtension(file.FileName) == ".pmp")
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
                        string file2 = "";
                        bool found = false;
                        foreach (var arr in textureLinks)
                        {
                            if (arr.Length >= 2 && arr[0] == FileName.Text)
                            {
                                file2 = arr[1];
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\", $"{selectedTextBox2.Text}.*.png", SearchOption.AllDirectories);
                            file2 = files2[0];
                        }
                        FileName.Text = selectedTextBox.Text + "\\" + selectedTextBox2.Text;
                        Log.Text = "Packing...";
                        NewTools.CTT.Encode($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{textBox.Text}", file2);
                        Log.Text = $"Packed {textBox.Text}!";
                    }
                    else if ((textBox.Text.EndsWith(".l2d") || textBox.Text.EndsWith(".fep") || textBox.Text.EndsWith(".pmo") || textBox.Text.EndsWith(".pmp")) && Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\"))
                    {
                        List<string> embedded = new List<string>();
                        foreach (TextBox textBox2 in Files3.Children)
                        {
                            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\";
                            embedded.Add(path + textBox2.Text);
                        }
                        NewTools.L2D.Pack($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{textBox.Text}", embedded);
                        Log.Text = $"Packed {textBox.Text}!";
                    }
                    else
                    {
                        Log.Text = $"{textBox} is not an archive nor texture file, and cannot be packed.";
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
                    else if (textBox.Text.EndsWith(".fep"))
                    {
                        file.Filter = "Effect files(*.fep)|*.fep";
                    }
                    else if (textBox.Text.EndsWith(".pmo"))
                    {
                        file.Filter = "Model files(*.pmo)|*.pmo";
                    }
                    else if (textBox.Text.EndsWith(".pmp"))
                    {
                        file.Filter = "Map files(*.pmp)|*.pmp";
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
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{selectedTextBox3.Text}", true);
                        Toolkit.CTTUnpack(Path.GetFileName(selectedTextBox3.Text), $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\");
                    }
                    else if (Path.GetExtension(file.FileName) == ".pmo" || Path.GetExtension(file.FileName) == ".l2d" || Path.GetExtension(file.FileName) == ".fep" || Path.GetExtension(file.FileName) == ".pmp")
                    {
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{selectedTextBox3.Text}", true);
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{selectedTextBox3.Text}", true);
                        Toolkit.ArcUnpack(Path.GetFileName(selectedTextBox3.Text), $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\");
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
                        string file2 = "";
                        bool found = false;
                        foreach (var arr in textureLinks)
                        {
                            if (arr.Length >= 2 && arr[0] == FileName.Text)
                            {
                                file2 = arr[1];
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\", $"{selectedTextBox2.Text}.*.png", SearchOption.AllDirectories);
                            file2 = files2[0];
                        }
                        FileName.Text = selectedTextBox.Text + "\\" + selectedTextBox2.Text + "\\" + selectedTextBox3.Text;
                        Log.Text = "Packing...";
                        NewTools.CTT.Encode($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{textBox.Text}", file2);
                        Log.Text = $"Packed {textBox.Text}!";
                    }
                    else if ((textBox.Text.EndsWith(".l2d") || textBox.Text.EndsWith(".fep") || textBox.Text.EndsWith(".pmo" ) || textBox.Text.EndsWith(".pmp")) && Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\"))
                    {
                        List<string> embedded = new List<string>();
                        foreach (TextBox textBox2 in Files3.Children)
                        {
                            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{Path.GetFileNameWithoutExtension(textBox.Text)}\";
                            embedded.Add(path + textBox2.Text);
                        }
                        NewTools.L2D.Pack($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\{textBox.Text}", embedded);
                        Log.Text = $"Packed {textBox.Text}!";
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
            foreach(string file in allfiles)
            {
                if (file.EndsWith(".ctt"))
                {
                    Log.Text = $"Packing {file}...";
                    string file2 = "";
                    bool found = false;
                    foreach (var arr in textureLinks)
                    {
                        string texture = arr[0];
                        string[] temp1 = texture.Split('\\');
                        texture = "";
                        for (int i = 0; i < (temp1.Count()); i++)
                        {
                            if (i < (temp1.Count() - 1))
                            {
                                texture += $@"{Path.GetFileNameWithoutExtension(temp1[i])}\";
                            }
                            else
                            {
                                texture += temp1[i];
                            }
                        }
                        if (arr.Length >= 2 && texture == file)
                        {
                            file2 = arr[1];
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}\", $"{Path.GetFileName(file)}.*.png", SearchOption.AllDirectories);
                        file2 = files2[0];
                    }
                    NewTools.CTT.Encode($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{file}", file2);
                    Log.Text = $"Packed {file}!";
                }
                else if ((file.EndsWith(".l2d") || file.EndsWith(".fep") || file.EndsWith(".pmo") || file.EndsWith(".pmp")) && Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}\"))
                {
                    List<string> embedded = new List<string>();
                    string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}\", $"*.ctt", SearchOption.AllDirectories);
                    foreach (string packed in files2)
                    {
                        string packed2 = packed.Replace($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\", "");
                        if (Path.GetFileNameWithoutExtension(packed2.Split('\\')[1]) == Path.GetFileNameWithoutExtension(file.Split('\\')[1]))
                        {
                            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}\";
                            embedded.Add(path + Path.GetFileName(packed));
                        }
                    }
                    MessageBox.Show("A window called ''Kingdom Hearts 3D Romhacking Suite'' will appear.\nType '14', and then press Enter.\nOnce ''Done!'' appears, press any key.");
                    NewTools.L2D.Pack($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{file}", embedded);
                    Log.Text = $"Packed {file}!";
                }
                else if (file.EndsWith(".rbin"))
                {
                    File.Copy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{file}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{file}", true);
                    try
                    {
                        Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileNameWithoutExtension(file)}", true);
                    }
                    catch { }
                    BetterDirCopy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\{Path.GetFileNameWithoutExtension(file)}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileNameWithoutExtension(file)}", false);
                    BetterDirCopy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\pack\{Path.GetFileNameWithoutExtension(file)}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileNameWithoutExtension(file)}", true);
                    MessageBox.Show("A window called ''Kingdom Hearts 3D Romhacking Suite'' will appear.\nType '2', and then press Enter.\nOnce ''Done!'' appears, press any key.");
                    Toolkit.RbinPack(file, false);
                    Log.Text = $"Packed {file}!";
                }
                if (file.Length - file.Replace("\\", "").Length == 1)
                {
                    Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\pack\{Path.GetDirectoryName(file)}");
                    File.Copy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{file}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\pack\{file}", true);
                }
            }
        }

        private void ExpMod_Click(object sender, RoutedEventArgs e)
        {
            MakePack finish = new MakePack(new Meta());
            finish.ShowDialog();

            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = System.IO.File.ReadAllText($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\temp.json");
            Meta mod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
            foreach (string file in allfiles)
            {
                if (file.EndsWith(".ctt"))
                {
                    Log.Text = $"Packing {file}...";
                    string file2 = "";
                    bool found = false;
                    foreach (var arr in textureLinks)
                    {
                        string texture = arr[0];
                        string[] temp1 = texture.Split('\\');
                        texture = "";
                        for (int i = 0; i < (temp1.Count()); i++)
                        {
                            if (i < (temp1.Count() - 1))
                            {
                                texture += $@"{Path.GetFileNameWithoutExtension(temp1[i])}\";
                            }
                            else
                            {
                                texture += temp1[i];
                            }
                        }
                        if (arr.Length >= 2 && texture == file)
                        {
                            file2 = arr[1];
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}\", $"{Path.GetFileName(file)}.*.png", SearchOption.AllDirectories);
                        file2 = files2[0];
                    }
                    NewTools.CTT.Encode($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{file}", file2);
                    Log.Text = $"Packed {file}!";
                }
                else if ((file.EndsWith(".l2d") || file.EndsWith(".fep") || file.EndsWith(".pmo") || file.EndsWith(".pmp")) && Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}\"))
                {
                    List<string> embedded = new List<string>();
                    string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}\", $"*.ctt", SearchOption.AllDirectories);
                    foreach (string packed in files2)
                    {
                        string packed2 = packed.Replace($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\", "");
                        if (Path.GetFileNameWithoutExtension(packed2.Split('\\')[1]) == Path.GetFileNameWithoutExtension(file.Split('\\')[1]))
                        {
                            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetDirectoryName(file)}\{Path.GetFileNameWithoutExtension(file)}\";
                            embedded.Add(path + Path.GetFileName(packed));
                        }
                    }
                    MessageBox.Show("A window called ''Kingdom Hearts 3D Romhacking Suite'' will appear.\nType '14', and then press Enter.\nOnce ''Done!'' appears, press any key.");
                    NewTools.L2D.Pack($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{file}", embedded);
                    Log.Text = $"Packed {file}!";
                }
                if (file.Length - file.Replace("\\", "").Length == 1)
                {
                    Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{mod.ID}\{Path.GetDirectoryName(file)}");
                    File.Copy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{file}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{mod.ID}\{file}", true);
                }
            }
            File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\temp.json");
            ZipMod(mod);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            Help hw = new Help();
            hw.Show();
        }

        private void WindowSwap(object sender, RoutedEventArgs e)
        {
            windowSwap = !windowSwap;
            allfiles.Clear();
            allfiles.AddRange(flaggedFiles);
            allfiles.AddRange(flaggedFiles2);
            allfiles.AddRange(flaggedFiles3);
            allfiles.Sort();
            allfiles.Reverse();
            Queued.Children.Clear();
            foreach (string file in allfiles)
            {
                int count = file.Length - file.Replace("\\", "").Length;
                string hex = "#000000";
                if (count == 0)
                {
                    hex = "#FF0000";
                }
                else if (count == 1)
                {
                    hex = "#F04080";
                }
                else if (count == 2)
                {
                    hex = "#FF80B0";
                }
                System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
                {
                    Text = file,
                    IsReadOnly = true,
                    Width = file.Length * 10,
                    Height = 20,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Background = (SolidColorBrush)new BrushConverter().ConvertFromString(hex),
                    BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFromString("#4080f0"),
                    Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#f2f2f2"),
                    Cursor = Cursors.No,
                    Focusable = false,
                };
                Queued.Children.Add(newTextBox);
            }
            if (windowSwap)
            {
                Queue.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/unqueue.png"));
                if (InfoWindow.Visibility == Visibility.Visible)
                {
                    windowStore = true;
                }
                else
                {
                    windowStore = false;
                }
                Blackout.Visibility = Visibility.Visible;
                InfoWindow.Visibility = Visibility.Collapsed;
                QueueWindow.Visibility = Visibility.Visible;
            }
            else
            {
                Queue.Source = new BitmapImage(new Uri(@"pack://application:,,,/Images/queue.png"));
                if (windowStore)
                {
                    InfoWindow.Visibility = Visibility.Visible;
                }
                else
                {
                    InfoWindow.Visibility = Visibility.Collapsed;
                }
                Blackout.Visibility = Visibility.Collapsed;
                QueueWindow.Visibility = Visibility.Collapsed;
            }
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please select a texture with the same width and height as the original.\nAn image with a different width and height may not be encoded correctly.", "Texture Link Warning");
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Texture File (*.png)|*.png";
            file.Title = "Select a file to open...";
            file.ShowDialog();
            var filedata = file.OpenFile;
            if (!string.IsNullOrWhiteSpace(file.FileName))
            {
                FileLink.Text = file.FileName;
                BitmapImage bitmap = new BitmapImage();
                using (FileStream fs = new FileStream(file.FileName, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = fs;
                    bitmap.EndInit();
                }
                if (textureSwap)
                {
                    TextureSmall.Source = bitmap;
                }
                else
                {
                    Texture.Source = bitmap;
                }
                bool found = false;
                foreach (var arr in textureLinks)
                {
                    if (arr.Length >= 2 && arr[0] == FileName.Text)
                    {
                        arr[1] = file.FileName;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    textureLinks.Add([FileName.Text, file.FileName]);
                }
            }
            QuickJson(true);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            using Process fileopener = new Process();

            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + FileLink.Text + "\"";
            fileopener.Start();
        }

        private void Unlink_Click(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Do you wish to unlink this texture?",
                    "Texture Unlink",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var arr in textureLinks)
                {
                    if (arr.Length >= 2 && arr[0] == FileName.Text)
                    {
                        textureLinks.Remove(arr);
                        break;
                    }
                }
                int from = FileName.Text.Split('\\').Length;
                if (from == 2)
                {
                    string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\", $"{selectedTextBox2.Text}.*.png", SearchOption.AllDirectories);
                    FileLink.Text = files2[0];

                }
                else if (from == 3)
                {
                    string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(selectedTextBox.Text)}\{Path.GetFileNameWithoutExtension(selectedTextBox2.Text)}\", $"{selectedTextBox3.Text}.*.png", SearchOption.AllDirectories);
                    FileLink.Text = files2[0];
                }
                BitmapImage bitmap = new BitmapImage();
                using (FileStream fs = new FileStream(FileLink.Text, FileMode.Open, FileAccess.Read))
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = fs;
                    bitmap.EndInit();
                }
                Texture.Source = bitmap;
            }
            QuickJson(true);
        }

        private async void ScrollFileLink(object sender = null, TextChangedEventArgs e = null)
        {
            await Task.Delay(100);
            try
            {
                if (VisualTreeHelper.GetChild(FileLink, 0) is Decorator border &&
                    border.Child is ScrollViewer scrollViewer)
                {
                    scrollViewer.ScrollToRightEnd();
                }
            }
            catch { }
        }

        private void MNN_Click(object sender, MouseButtonEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(Texture, BitmapScalingMode.NearestNeighbor);
        }
        private void MLS_Click(object sender, MouseButtonEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(Texture, BitmapScalingMode.HighQuality);
        }
        private void SNN_Click(object sender, MouseButtonEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(TextureSmall, BitmapScalingMode.NearestNeighbor);
        }
        private void SLS_Click(object sender, MouseButtonEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(TextureSmall, BitmapScalingMode.HighQuality);
        }

        private void TextureSwap(object sender, RoutedEventArgs e)
        {
            textureSwap = !textureSwap;
            TextureTemp.Source = Texture.Source;
            Texture.Source = TextureSmall.Source;
            TextureSmall.Source = TextureTemp.Source;
            LocationTemp.Text = Location.Text;
            Location.Text = LocationSmall.Text;
            LocationSmall.Text = LocationTemp.Text;
        }

        private void FileLink_Click(object sender, MouseButtonEventArgs e)
        {
            ScrollFileLink();
        }

        private void Files2_Filter(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Files2.Children.Clear();
                string filter = "";
                if (Sort.SelectedIndex == 0)
                {
                    foreach (var textbox in unfiltered)
                    {
                        Files2.Children.Add(textbox);
                    }
                }
                else
                {
                    if (Sort.SelectedIndex == 1)
                    {
                        filter = ".ctt";
                    }
                    else if (Sort.SelectedIndex == 2)
                    {
                        filter = ".l2d";
                    }
                    else if (Sort.SelectedIndex == 3)
                    {
                        filter = ".fep";
                    }
                    else if (Sort.SelectedIndex == 4)
                    {
                        filter = ".pmo";
                    }
                    else if (Sort.SelectedIndex == 5)
                    {
                        filter = ".pmp";
                    }
                    foreach (var textbox in unfiltered)
                    {
                        if (textbox.Text.Contains(filter))
                        {
                            Files2.Children.Add(textbox);
                        }
                    }
                }
            }
            catch { }
        }

        private void Reverse_Rebirth(object sender, RoutedEventArgs e)
        {
            Manager mw = new Manager();
            mw.Show();
            Close();
        }

        private void ZipMod(Meta meta)
        {
            if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{meta.ID}"))
            {
                try
                {
                    Misc.CopyDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{meta.ID}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{meta.ID}\{meta.Name}", true);

                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = JsonSerializer.Serialize(meta, jsonoptions);
                    string filepath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{meta.ID}\{meta.Name}" + $@"\meta.json";
                    System.IO.File.WriteAllText(filepath, jsonString);
                    var libpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
                    SevenZipCompressor.SetLibraryPath(libpath);
                    SevenZipCompressor zcompressor = new SevenZipCompressor
                    {
                        ArchiveFormat = OutArchiveFormat.SevenZip,
                        CompressionLevel = CompressionLevel.High
                    };
                    Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Title = "Save Mod Archive",
                        Filter = "Mod Archive (*.7z)|*.7z",
                        DefaultExt = ".7z",
                        FileName = $@"{meta.ID}.7z"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string filePath = saveFileDialog.FileName;
                        zcompressor.CompressDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{meta.ID}", filePath);
                    }
                    else
                    {
                        Console.WriteLine("Save file operation canceled.");
                    }
                    Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{meta.ID}\{meta.Name}", true);
                }
                catch { }
            }
        }
    }
}