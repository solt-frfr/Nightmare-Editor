using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Pulsar;
using SevenZip;
using Nightmare_Editor;
using Nightmare_Editor.NewTools;
using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;


namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for Manager.xaml
    /// </summary>
    public partial class Manager : Window
    {
        /// This is largely copied from Pulsar. It's software also developed by me.
        private List<string> enabledmods = new List<string>();
        private bool isInitialized = false;

        public Manager()
        {
            InitializeComponent();
            ModsWindow(true);
            SettingsWindow.Visibility = Visibility.Collapsed;
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods");
            string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
            if (!System.IO.File.Exists(settingspath))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                Settings settings = new Settings();
                settings.DeployPath = "";
                settings.DefaultImage = 0;
                string jsonString = JsonSerializer.Serialize<Settings>(settings, jsonoptions);
                System.IO.File.WriteAllText(settingspath, jsonString);
            }
            string enabledmodspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\enabledmods.json";
            if (!System.IO.File.Exists(enabledmodspath))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize<List<string>>(new List<string>(), jsonoptions);
                System.IO.File.WriteAllText(enabledmodspath, jsonString);
            }
            Refresh();
            isInitialized = true;
        }

        private string[] CountFolders(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                string[] directories = Directory.GetDirectories(folderPath);
                int folderCount = directories.Length;
                return directories;
            }
            else
            {
                return null;
            }
        }

        public string CreateLinkImage(string link)
        {
            try
            {
                if (link.Contains("gamebanana.com"))
                {
                    return "Images/Gamebanana.png";
                }
                else if (link.Contains("github.com"))
                {
                    return "Images/Github.png";
                }
                else if (!string.IsNullOrWhiteSpace(link))
                {
                    return "Images/Web.png";
                }
                else
                {
                    return null;
                }
            }
            catch { return null; }
        }
        public void Refresh()
        {
            try
            {
                try
                {
                    enabledmods.Clear();
                }
                catch { }
                enabledmods = QuickJson(false, enabledmods, "enabledmods.json");
            }
            catch { }
            ModDataGrid.Items.Clear();
            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
            string[] griditems = CountFolders(path);
            Settings settings = new Settings();
            string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
            List<string> blacklist = new List<string>();
            if (System.IO.File.Exists(settingspath))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = System.IO.File.ReadAllText(settingspath);
                settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
                PathBox.Text = settings.DeployPath;
                DefPrevBox.SelectedIndex = settings.DefaultImage;
                Preview.Source = new BitmapImage(new Uri($"/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.Relative));
            }
            foreach (string modpath in griditems)
            {
                Meta mod = new Meta();
                string filepath = modpath + $@"\meta.json";
                if (!System.IO.File.Exists(filepath))
                {
                    string genid = modpath.Replace(path, "");
                    mod.Name = mod.ID = genid = genid.TrimStart('\\');
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = JsonSerializer.Serialize(mod, jsonoptions);
                    System.IO.File.WriteAllText(filepath, jsonString);
                }
                if (System.IO.File.Exists(filepath))
                {
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = System.IO.File.ReadAllText(filepath);
                    mod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                    if ((path + $@"\{mod.ID}" == modpath) && !ModDataGrid.Items.Contains(mod))
                    {
                        if (enabledmods.Contains(mod.ID))
                            mod.IsChecked = true;
                        else
                            mod.IsChecked = false;
                        mod.LinkImage = CreateLinkImage(mod.Link);
                        ModDataGrid.Items.Add(mod);
                    }
                }
            }
            ModDataGrid.Items.Refresh();
        }
        private void New_OnClick(object sender, RoutedEventArgs e)
        {
            Editor ew = new Editor();
            ew.Show();
            Close();
        }

        private void Folder_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}"))
                    {
                        try
                        {
                            ProcessStartInfo StartInformation = new ProcessStartInfo();
                            StartInformation.FileName = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}";
                            StartInformation.UseShellExecute = true;
                            Process process = Process.Start(StartInformation);
                        }
                        catch { }
                    }
                }
            }
        }

        private void Zip_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}"))
                    {
                        try
                        {
                            Misc.CopyDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}\{row.Name}", true);

                            var jsonoptions = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };
                            string jsonString = JsonSerializer.Serialize(row, jsonoptions);
                            string filepath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}\{row.Name}" + $@"\meta.json";
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
                                Filter = "Pulsar Archive (*.7z)|*.7z",
                                DefaultExt = ".7z",
                                FileName = $@"{row.ID}.7z"
                            };

                            if (saveFileDialog.ShowDialog() == true)
                            {
                                string filePath = saveFileDialog.FileName;
                                zcompressor.CompressDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}", filePath);
                            }
                            else
                            {
                                Console.WriteLine("Save file operation canceled.");
                            }
                            Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}\{row.Name}", true);
                        }
                        catch { }
                    }
                }
            }
            Refresh();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {

            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    if (MessageBox.Show($@"Are you sure you want to delete {row.Name}?",
                    $"Delete {row.Name}",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}", true);
                    }
                }
            }
            Refresh();
        }

        private void currentrow(object sender, SelectionChangedEventArgs e)
        {
            Meta row = (Meta)ModDataGrid.SelectedItem;
            try
            {
                if (string.IsNullOrWhiteSpace(row.Description))
                    DescBox.Text = "Quasar never worked for me, so I made my own. You're seeing this because this mod has no description, or no mod is selected.\n\nDon't see a mod? The ID and folder names must match.\n\nConfused about the buttons at the bottom? Hover over them for more info.";
                else
                    DescBox.Text = row.Description;
            }
            catch
            {
                DescBox.Text = "Quasar never worked for me, so I made my own. You're seeing this because this mod has no description, or no mod is selected.\n\nDon't see a mod? The ID and folder names must match.\n\nConfused about the buttons at the bottom? Hover over them for more info.";
            }
            try
            {
                if (System.IO.File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}\preview.webp"))
                {
                    BitmapImage bitmap = new BitmapImage();
                    using (FileStream fs = new FileStream($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}\preview.webp", FileMode.Open, FileAccess.Read))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = fs;
                        bitmap.EndInit();
                    }
                    Preview.Source = bitmap;
                }
                else
                {
                    Preview.Source = new BitmapImage(new Uri($"/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.Relative));
                }
            }
            catch
            {
                Preview.Source = new BitmapImage(new Uri($"/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.Relative));
            }
        }

        private void Mods_Click(object sender, RoutedEventArgs e)
        {
            var modsImage = (Image)ModsButton.Template.FindName("ModsImage", ModsButton);
            if (modsImage != null)
            {
                modsImage.Source = new BitmapImage(new Uri("/Images/ModsSel.png", UriKind.Relative));
            }
            var settingsImage = (Image)SettingsButton.Template.FindName("SettingsImage", SettingsButton);
            if (settingsImage != null)
            {
                settingsImage.Source = new BitmapImage(new Uri("/Images/SettingsUnsel.png", UriKind.Relative));
            }
            var downloadImage = (Image)DownloadButton.Template.FindName("DownloadImage", DownloadButton);
            if (downloadImage != null)
            {
                downloadImage.Source = new BitmapImage(new Uri("/Images/DownloadUnsel.png", UriKind.Relative));
            }
            ModsWindow(true);
            SettingsWindow.Visibility = Visibility.Collapsed;
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var modsImage = (Image)ModsButton.Template.FindName("ModsImage", ModsButton);
            if (modsImage != null)
            {
                modsImage.Source = new BitmapImage(new Uri("/Images/ModsUnsel.png", UriKind.Relative));
            }
            var settingsImage = (Image)SettingsButton.Template.FindName("SettingsImage", SettingsButton);
            if (settingsImage != null)
            {
                settingsImage.Source = new BitmapImage(new Uri("/Images/SettingsSel.png", UriKind.Relative));
            }
            var downloadImage = (Image)DownloadButton.Template.FindName("DownloadImage", DownloadButton);
            if (downloadImage != null)
            {
                downloadImage.Source = new BitmapImage(new Uri("/Images/DownloadUnsel.png", UriKind.Relative));
            }
            ModsWindow(false);
            SettingsWindow.Visibility = Visibility.Visible;
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "https://gamebanana.com/games/17208",
                    UseShellExecute = true
                });
            }
            catch { }
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Deploy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
                string jsonString = System.IO.File.ReadAllText(settingspath);
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
                if (!string.IsNullOrWhiteSpace(settings.DeployPath) && settings != null)
                {
                    DefPrevBox.SelectedIndex = settings.DefaultImage;
                    System.IO.File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\conflictlog.json");
                    if (MessageBox.Show($@"This will delete all files inside {settings.DeployPath}. Is this okay?",
                    $"Empty {settings.DeployPath}",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Directory.Delete(settings.DeployPath, true);
                        Directory.CreateDirectory(settings.DeployPath);
                        List<string> rbins = new List<string>();
                        foreach (string ID in enabledmods)
                        {
                            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ID}\";
                            string[] subdirectories = Directory.GetDirectories(path);
                            foreach (string subdir in subdirectories)
                            {
                                DirectoryInfo dir = new DirectoryInfo(subdir);
                                string rbin = dir.Name;
                                if (!rbins.Contains(rbin))
                                {
                                    rbins.Add(rbin);
                                }
                            }
                        }
                        bool stop = false;
                        foreach (string rbin in rbins)
                        {
                            if (!File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{rbin}.rbin") || !Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\{rbin}"))
                            {
                                stop = true;
                                MessageBox.Show($@"Missing {rbin}.rbin. Unpack it using the unpack button in the settings tab.");
                            }
                        }
                        if (stop)
                        {
                            return;
                        }
                        foreach (string ID in enabledmods)
                        {
                            Editor.BetterDirCopy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ID}\", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\pack\", false);
                        }
                        foreach (string rbin in rbins)
                        {
                            string file = rbin + ".rbin";
                            File.Copy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current\{file}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{file}", true);
                            try
                            {
                                Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileNameWithoutExtension(file)}", true);
                            }
                            catch { }
                            Editor.BetterDirCopy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\{Path.GetFileNameWithoutExtension(file)}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileNameWithoutExtension(file)}", false);
                            Editor.BetterDirCopy($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\pack\{Path.GetFileNameWithoutExtension(file)}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileNameWithoutExtension(file)}", true);
                            MessageBox.Show("A window called ''Kingdom Hearts 3D Romhacking Suite'' will appear.\nType '2', and then press Enter.\nOnce ''Done!'' appears, press any key.");
                            Toolkit.RbinPack(file, true);
                        }
                        MessageBox.Show($@"Succesfully deployed mods to {settings.DeployPath}!");
                    }
                }
                else
                {
                    MessageBox.Show($@"No output directory set. Set one in the settings tab.");
                }
            }
            catch { }
        }

        private void ModsWindow(bool sender)
        {
            if (sender == false)
            {
                Mods.Visibility = Visibility.Collapsed;
                ModContent.Visibility = Visibility.Collapsed;
            }
            else
            {
                Mods.Visibility = Visibility.Visible;
                ModContent.Visibility = Visibility.Visible;
            }
        }

        private void Path_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog modspath = new System.Windows.Forms.FolderBrowserDialog();
            if (modspath.ShowDialog() != null)
            {
                if (!string.IsNullOrWhiteSpace(modspath.SelectedPath))
                    PathBox.Text = modspath.SelectedPath;
            }
        }

        private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isInitialized) return;
            Settings settings = new Settings();
            settings.DeployPath = PathBox.Text;
            settings.DefaultImage = DefPrevBox.SelectedIndex;
            string filepath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";

            string jsonString = System.IO.File.ReadAllText(filepath);
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            jsonString = JsonSerializer.Serialize(settings, jsonoptions);
            System.IO.File.WriteAllText(filepath, jsonString);
            settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            Refresh();
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods"))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
            }
        }

        private void InstallArchive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var libpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
                SevenZip.SevenZipExtractor.SetLibraryPath(libpath);
                System.Windows.Forms.OpenFileDialog openarchive = new System.Windows.Forms.OpenFileDialog();
                openarchive.Filter = "Mod Archive (*.7z)|*.7z";
                openarchive.Title = "Select Mod Archive";
                string archivePath = null;
                string filename = null;
                string filetype = null;
                string firstDirectoryPath = null;
                string extpath = null;
                string newpath = null;
                string temppath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp";
                string outputFolder = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
                Directory.CreateDirectory(temppath);
                Directory.Delete(temppath, true);
                Directory.CreateDirectory(temppath);
                if (openarchive.ShowDialog() != null)
                {
                    archivePath = openarchive.FileName;
                    filename = System.IO.Path.GetFileNameWithoutExtension(archivePath);
                    filetype = System.IO.Path.GetExtension(archivePath);
                }
                HashSet<string> validNames = new HashSet<string>
                {
                    "_grpdef",
                    "cam",
                    "chara_boss",
                    "chara_d_obj",
                    "chara_e_obj",
                    "chara_enemy",
                    "chara_f_obj",
                    "chara_gim",
                    "chara_high",
                    "chara_npc",
                    "chara_pc",
                    "chara_wep",
                    "effect",
                    "event",
                    "font",
                    "game",
                    "item",
                    "map",
                    "menu",
                    "message",
                    "minigame",
                    "mission",
                    "setdata"
                };
                using (var extractor = new SevenZip.SevenZipExtractor(archivePath))
                {
                    foreach (var fileData in extractor.ArchiveFileData)
                    {
                        if (fileData.FileName.Contains("/") || fileData.FileName.Contains("\\"))
                        {
                            var parts = fileData.FileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                            firstDirectoryPath = parts[0].TrimEnd('/', '\\');
                            break;
                        }
                    }
                    if (validNames.Contains(firstDirectoryPath))
                        extpath = temppath;
                    else
                        extpath = temppath + $@"\" + firstDirectoryPath.ToLower();
                    newpath = outputFolder + $@"\" + filename.ToLower();
                    Directory.CreateDirectory(extpath);
                    extractor.ExtractArchive(temppath);
                }
                if (Directory.Exists(extpath))
                {
                    try
                    {
                        if (Directory.Exists(newpath))
                        {
                            bool moved = false;

                            if (MessageBox.Show($@"This will delete {newpath}. Okay?",
                                $"Delete {newpath}",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                Directory.Delete(newpath, true);
                                Directory.Move(extpath, newpath);
                                moved = true;
                            }
                            if (moved == false)
                            {
                                Directory.Delete(temppath, true);
                            }
                        }
                        else
                        {
                            Directory.Move(extpath, newpath);
                        }
                    }
                    catch { }
                }
                if (System.IO.File.Exists(newpath + $@"\meta.json"))
                {
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = System.IO.File.ReadAllText(newpath + $@"\meta.json");
                    Meta extmod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                    if (System.IO.File.Exists(newpath + $@"\preview.webp"))
                        extmod.ArchiveImage = true;
                    MakePack finish = new MakePack(extmod);
                    finish.ShowDialog();
                }
                else
                {
                    Meta extmod = new Meta();
                    if (validNames.Contains(firstDirectoryPath))
                        extmod.Name = filename;
                    else
                        extmod.Name = firstDirectoryPath;
                    extmod.ID = filename.ToLower();
                    if (System.IO.File.Exists(newpath + $@"\preview.webp"))
                        extmod.ArchiveImage = true;
                    MakePack finish = new MakePack(extmod);
                    finish.ShowDialog();
                }
            }
            catch { }
            Refresh();
        }


        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.CommandParameter is string url)
            {
                try
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch { }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.CheckBox checkBox)
            {
                var row = checkBox.DataContext as Meta;
                if (row != null)
                {
                    if (!enabledmods.Contains(row.ID))
                        enabledmods.Add(row.ID);
                    QuickJson(true, enabledmods, "enabledmods.json");
                    enabledmods = QuickJson(false, enabledmods, "enabledmods.json");
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.CheckBox checkBox)
            {
                var row = checkBox.DataContext as Meta;
                if (row != null)
                {
                    if (enabledmods.Contains(row.ID))
                        enabledmods.Remove(row.ID);
                    QuickJson(true, enabledmods, "enabledmods.json");
                    enabledmods = QuickJson(false, enabledmods, "enabledmods.json");
                }
            }
        }

        private List<string> QuickJson(bool write, List<string> what, string filename)
        {
            string root = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\";
            if (write)
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize(what, jsonoptions);
                System.IO.File.WriteAllText(root + filename, jsonString);
                return null;
            }
            else
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = System.IO.File.ReadAllText(root + filename);
                what = JsonSerializer.Deserialize<List<string>>(jsonString, jsonoptions);
                return what;
            }
        }

        public static string[] ListToArray(List<string> sender)
        {
            string[] send = new string[sender.Count];
            for (var i = 0; i < sender.Count; ++i)
                send[i] = sender[i];
            return send;
        }

        private void DefPrevBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;
            PathBox_TextChanged(null, null);
            Refresh();
        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            Meta row = (Meta)ModDataGrid.SelectedItem;
            MakePack edit = new MakePack(row);
            Preview.Source = new BitmapImage(new Uri($"/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.Relative));
            try
            {
                edit.ShowDialog();
            }
            catch { }
            Refresh();
        }

        private void File_Unpack(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Current");
            OpenFileDialog file = new OpenFileDialog();
            file.Filter = "Game Archive files (*.rbin)|*.rbin";
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
                        File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileName(file.FileName)}", true);
                        Toolkit.RbinExtract(Path.GetFileName(file.FileName));
                    }
                    catch
                    {
                        MessageBox.Show("You attempted to unpack an already unpacked file.");
                    }
                }
                else
                {
                    string[] files = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\", "*.*", SearchOption.AllDirectories);
                    File.Copy(file.FileName, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\User-Added\{files.Length}-{Path.GetFileName(file.FileName)}", true);
                }
            }
        }
    }
}

