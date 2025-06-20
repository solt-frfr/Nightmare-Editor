using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualBasic.Logging;
using Nightmare_Editor.NewTools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using static System.Net.Mime.MediaTypeNames;
using static Nightmare_Editor.NewTools.TXA;
using Microsoft.Win32;
using System.Text.Json;

namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for AnimWindow.xaml
    /// There's quite a few times where widths and heights are flipped. I'm sorry. I don't feel like fixing it either though.
    /// </summary>
    public partial class AnimWindow : Window
    {
        private TXA.TXAFile main;

        private bool loaded = false;
        private TextBox selectedTextBox;
        private TextBox selectedTextBox2;
        private TextBox selectedTextBox3;
        private TextBox selectedTextBox4;
        public AnimWindow(string file)
        {
            InitializeComponent();
            if (file.Contains($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base"))
            {
                MessageBox.Show("Woah there! Let's not edit our base files, those are important.");
            }
            else
            {
                TXA.TXAFile txa = TXA.Load(file);
                main = txa;
            }
            Import();
            InfoWindow.Visibility = Visibility.Collapsed;
            loaded = true;
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
            for (int i = 0; i < 4 - (Files2.Children.Count + 1).ToString().Length; i++)
            {
                zeros += "0";
            }
            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
            {
                Name = "file" + zeros + (Files2.Children.Count + 1).ToString(),
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
            for (int i = 0; i < 4 - (Files3.Children.Count + 1).ToString().Length; i++)
            {
                zeros += "0";
            }
            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
            {
                Name = "file" + zeros + (Files3.Children.Count + 1).ToString().Length,
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

        private void AddFile4()
        {
            string zeros = "";
            for (int i = 0; i < 4 - (Textures.Children.Count + 1).ToString().Length; i++)
            {
                zeros += "0";
            }
            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
            {
                Name = "Texture" + zeros + (Textures.Children.Count + 1).ToString().Length,
                Text = $@"Texture {Textures.Children.Count + 1}",
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
            newTextBox.MouseLeftButtonUp += TextBox4_Click;
            newTextBox.PreviewMouseLeftButtonDown += TextBox4_PreviewMouseLeftButtonDown;
            newTextBox.PreviewMouseRightButtonDown += TextBox4_PreviewMouseLeftButtonDown;
            Textures.Children.Add(newTextBox);
        }

        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox = tb;
                tb.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#F04080");
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
            InfoWindow.Visibility = Visibility.Collapsed;
            foreach (var child in Files.Children)
            {
                Files2.Children.Clear();
                if (child is TextBox textBox && textBox == selectedTextBox)
                {
                    foreach (AnimGroup group in main.Groups)
                    {
                        if (selectedTextBox.Text == group.Name)
                        {
                            foreach (Anim anim in group.Anims)
                            {
                                AddFile2(anim.Name);
                            }
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
                FileName.Items.Clear();
            }
            catch { }
            FileName.Items.Add($@"Destination Texture");
            for (int i = 0; i < main.Textures.Count; i++)
            {
                FileName.Items.Add($@"Texture {i + 1}");
            }
            try
            {
                Files3_Scroll.ScrollToTop();
            }
            catch { }
            InfoWindow.Visibility = Visibility.Collapsed;
            foreach (var child in Files2.Children)
            {
                Files3.Children.Clear();
                if (child is TextBox textBox && textBox == selectedTextBox2)
                {
                    foreach (AnimGroup group in main.Groups)
                    {
                        if (selectedTextBox.Text == group.Name)
                        {
                            foreach (Anim anim in group.Anims)
                            {
                                if (selectedTextBox2.Text == anim.Name)
                                {
                                    for (int i = 1; i <= anim.Frames.Count; i++)
                                    {
                                        AddFile3($"Frame {i}");
                                    }
                                    break;
                                }
                            }
                        }
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
            }
            foreach (TextBox textbox in Files3.Children)
            {
                if (textbox != selectedTextBox3)
                    textbox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020");
            }
        }

        private void TextBox3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Files3.Children.Count; i++)
            {
                InfoWindow.Visibility = Visibility.Collapsed;
                if (Files3.Children[i] is TextBox textBox && textBox == selectedTextBox3)
                {
                    AssignImage(i, 3);
                    break;
                }
            }
        }

        private void TextBox4_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox tb)
            {
                selectedTextBox4 = tb;
                tb.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#F04080");
            }
            foreach (TextBox textbox in Textures.Children)
            {
                if (textbox != selectedTextBox4)
                    textbox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020");
            }
        }

        private void TextBox4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Textures.Children.Count; i++)
            {
                InfoWindow.Visibility = Visibility.Collapsed;
                if (Textures.Children[i] is TextBox textBox && textBox == selectedTextBox4)
                {
                    AssignImage(i, 2);
                    break;
                }
            }
            Files3.Children.Clear();
        }

        private ImageSource ConvertToImageSource(SixLabors.ImageSharp.Image image)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.SaveAsPng(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = memoryStream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }

        private void AssignImage(int input, int from)
        {
            long texture = 0;
            bool foundimage = false;
            if (from == 2)
            {
                texture = input;
                InfoWindow.Visibility = Visibility.Visible;
                FileInfo.Visibility = Visibility.Collapsed;
                TexInfo.Visibility = Visibility.Visible;
                AddFrame.Visibility = Visibility.Collapsed;
                Texture.Source = ConvertToImageSource(main.DecodedTextures[(int)texture]);
                int j = 0;
                for (int k = 0; k < main.Textures.Count; k++)
                {
                    if (main.Textures[(int)texture].DestTexture == main.DestTextures[k].Name)
                    {
                        j = k;
                        break;
                    }
                }
                Tex.Text = main.Textures[(int)texture].DestTexture;
                int height = main.DestTextures[j].Texture[0x22] + (main.DestTextures[j].Texture[0x23] * 0x100);
                int width = main.DestTextures[j].Texture[0x20] + (main.DestTextures[j].Texture[0x21] * 0x100);
                for (int i = 0; i < main.Groups.Count; i++)
                {
                    if (main.Groups[i].DestTexture == main.DestTextures[j].Name)
                    {
                        height = main.Groups[i].DestWidth;
                        width = main.Groups[i].DestHeight;
                    }
                }
                TexSize.Text = $@"{width}x{height}";
                TexFormat.Text = ((CTT.Format)main.DestTextures[j].Texture[0x1C]).ToString();
            }
            else if (from == 3)
            {
                InfoWindow.Visibility = Visibility.Visible;
                FileInfo.Visibility = Visibility.Visible;
                TexInfo.Visibility = Visibility.Collapsed;
                AddFrame.Visibility = Visibility.Visible;
                foreach (var child in Files3.Children)
                {
                    if (child is TextBox textBox && textBox == selectedTextBox3)
                    {
                        foreach (AnimGroup group in main.Groups)
                        {
                            if (selectedTextBox.Text == group.Name)
                            {
                                Destination.Text = group.DestTexture;
                                foreach (Anim anim in group.Anims)
                                {
                                    if (selectedTextBox2.Text == anim.Name)
                                    {
                                        Length.Text = anim.Frames[input].Length.ToString();
                                        Length2.Text = anim.Frames[input].Length2.ToString();
                                        if (anim.Frames[input].Texture == 0)
                                        {
                                            for (int i = 0; i < main.DestTextures.Count; i++)
                                            {
                                                if (group.DestTexture == main.DestTextures[i].Name)
                                                {
                                                    foundimage = true;
                                                    byte[] source = new byte[main.DestTextures[i].Texture.Length - 0x80];
                                                    for (int j = 0; j < source.Length; j++)
                                                    {
                                                        source[j] = main.DestTextures[i].Texture[j + 0x80];
                                                    }
                                                    Texture.Source = ConvertToImageSource(CTT.Deswizzle(source, group.DestHeight, group.DestWidth, main.DestTextures[i].Texture[0x1C]));
                                                    FileName.SelectedIndex = 0;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            texture = anim.Frames[input].Texture;
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
                if (!foundimage)
                {
                    for (int i = 0; i < main.Adresses.Count; i++)
                    {
                        if (main.Adresses[i] == texture)
                        {
                            foundimage = true;
                            Texture.Source = ConvertToImageSource(main.DecodedTextures[i]);
                            FileName.SelectedIndex = i + 1;
                            break;
                        }
                    }
                }
                if (!foundimage)
                {
                    MessageBox.Show("The texture this frame used was removed.\nReverting to destination texture.");
                    foreach (var child in Files3.Children)
                    {
                        if (child is TextBox textBox && textBox == selectedTextBox3)
                        {
                            foreach (AnimGroup group in main.Groups)
                            {
                                if (selectedTextBox.Text == group.Name)
                                {
                                    foreach (Anim anim in group.Anims)
                                    {
                                        if (selectedTextBox2.Text == anim.Name)
                                        {
                                            anim.Frames[input].Texture = 0;
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    }
                    AssignImage(input, from);
                }
            }
        }


        private void Help_Click(object sender, RoutedEventArgs e)
        {
            BadApple(null, null);
            Help hw = new Help();
            hw.Show();
        }

        private void Load_Click(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
            open.Filter = "Nightmare Animation Project (*.json)|*.json";
            open.Title = "Select Project File";
            if (open.ShowDialog() != null)
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = System.IO.File.ReadAllText(open.FileName);
                main = JsonSerializer.Deserialize<TXAFile>(jsonString, jsonoptions);
                main.DecodedTextures = new List<SixLabors.ImageSharp.Image>();
                foreach (Texture texture in main.Textures)
                {
                    byte[] dest = Array.Empty<byte>();
                    for (int i = 0; i < main.DestTextures.Count; i++)
                    {
                        if (main.DestTextures[i].Name == texture.DestTexture)
                        {
                            dest = main.DestTextures[i].Texture;
                        }
                    }
                    int height = dest[0x22] + (dest[0x23] * 0x100);
                    int width = dest[0x20] + (dest[0x21] * 0x100);
                    SixLabors.ImageSharp.Image decode = CTT.Deswizzle(texture.Data, width, height, dest[0x1C]);
                    main.DecodedTextures.Add(decode);
                }
                Import();
            }
        }

        private void Import_Click(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to quit?\nAny unsaved progress will be lost.",
                    $"Import new TXA",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
                open.Filter = "Texture Animation (*.txa)|*.txa";
                open.Title = "Select TXA File";
                if (open.ShowDialog() != null)
                {
                    if (open.FileName.Contains($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base"))
                    {
                        MessageBox.Show("Woah there! Let's not edit our base files, those are important.");
                    }
                    else
                    {
                        TXA.TXAFile txa = TXA.Load(open.FileName);
                        main = txa;
                    }
                    Import();
                }
            }
        }

        private void Save_Click(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Nightmare Animation Project (*.json)|*.json";
            saveFileDialog.Title = "Save Project...";
            saveFileDialog.ShowDialog();

            if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize<TXAFile>(main, jsonoptions);
                System.IO.File.WriteAllText(saveFileDialog.FileName, jsonString);
            }   
        }

        private void Export_Click(object sender, MouseButtonEventArgs e)
        {
            byte[] file = TXA.Create(main);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Texture Animation (*.txa)|*.txa";
            saveFileDialog.Title = "Save TXA...";
            saveFileDialog.ShowDialog();
            if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                System.IO.File.WriteAllBytes(saveFileDialog.FileName, file);
            }
        }

        private void Reverse_Rebirth(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to quit?\nAny unsaved progress will be lost.",
                    $"Quit",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        private void RemoveFile3(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove this frame?\nIt may cause problems.",
                    $"Remove Frame",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int i = Files.Children.IndexOf(selectedTextBox);
                int j = Files2.Children.IndexOf(selectedTextBox2);
                int k = Files3.Children.IndexOf(selectedTextBox3);
                if (main.Groups[i].Anims[j].Frames.Count <= 1)
                {
                    MessageBox.Show("You can't remove the only frame.");
                }
                else
                {
                    main.Groups[i].Anims[j].Frames.Remove(main.Groups[i].Anims[j].Frames[k]);
                    Files3.Children.Clear();
                    for (int l = 0; l < main.Groups[i].Anims[j].Frames.Count; l++)
                    {
                        AddFile3($"Frame {l + 1}");
                    }
                    MessageBox.Show("Removed.");
                }
            }
        }

        private void RemoveTex(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to remove this texture?\nAll frames using this texture will revert to their Destination Texture.",
                    $"Remove Texture",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                int i = Textures.Children.IndexOf(selectedTextBox4);
                long texture = main.Adresses[i];
                main.Adresses.Remove(main.Adresses[i]);
                main.Textures.Remove(main.Textures[i]);
                main.DecodedTextures.Remove(main.DecodedTextures[i]);
                Textures.Children.Clear();
                foreach (long adress in main.Adresses)
                {
                    AddFile4();
                }
                foreach (AnimGroup group in main.Groups)
                {
                    foreach (Anim anim in group.Anims)
                    {
                        for (int j = 0; j < anim.Frames.Count; j++)
                        {
                            if (anim.Frames[j].Texture == texture)
                            {
                                anim.Frames[j].Texture = 0;
                            }
                        }
                    }
                }
                MessageBox.Show("Removed.");
            }
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            int i = Textures.Children.IndexOf(selectedTextBox4);
            int j = 0;
            for (int k = 0; k < main.Textures.Count; k++)
            {
                if (main.Textures[i].DestTexture == main.DestTextures[k].Name)
                {
                    j = k;
                    break;
                }
            }
            byte[] dest = main.DestTextures[j].Texture;
            MessageBox.Show("Please select a texture with the same width and height as the original.\nAn image with a different width and height may not be encoded correctly.", "New Texture Warning");
            System.Windows.Forms.OpenFileDialog openPng = new System.Windows.Forms.OpenFileDialog();
            openPng.Filter = "Texture (*.*)|*.*";
            openPng.Title = "Select New Texture";
            if (openPng.ShowDialog() != null)
            {
                if (System.IO.File.Exists(openPng.FileName))
                {
                    byte[] image = System.IO.File.ReadAllBytes(openPng.FileName);
                    byte[] textwheader = CTT.Swizzle(image, main.DestTextures[j].Texture[0x1C]);
                    byte[] text = new byte[dest.Length - 0x80];
                    for (int k = 0; k < text.Length; k++)
                    {
                        text[k] = textwheader[k + 0x80];
                    }
                    main.Textures[i].Data = text;
                    main.DecodedTextures[i] = SixLabors.ImageSharp.Image.Load(image);
                }
                AssignImage(i, 2);
            }
        }

        private void SaveTex_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Texture File|*.png";
            saveFileDialog.Title = "Save extracted texture...";
            saveFileDialog.ShowDialog();

            if (!string.IsNullOrWhiteSpace(saveFileDialog.FileName))
            {
                bool foundimage = false;
                int i = Textures.Children.IndexOf(selectedTextBox4);
                InfoWindow.Visibility = Visibility.Visible;
                FileInfo.Visibility = Visibility.Collapsed;
                TexInfo.Visibility = Visibility.Visible;
                main.DecodedTextures[i].SaveAsPng(saveFileDialog.FileName);
                int j = 0;
            }
        }

        private void MNN_Click(object sender, MouseButtonEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(Texture, BitmapScalingMode.NearestNeighbor);
        }

        private void MLS_Click(object sender, MouseButtonEventArgs e)
        {
            RenderOptions.SetBitmapScalingMode(Texture, BitmapScalingMode.HighQuality);
        }

        private void Import()
        {
            try
            {
                Files.Children.Clear();
                Files2.Children.Clear();
                Files3.Children.Clear();
                Textures.Children.Clear();
            }
            catch { }
            File.Text = main.Name;
            foreach (TXA.AnimGroup group in main.Groups)
            {
                AddFile(group.Name);
            }
            foreach (long adress in main.Adresses)
            {
                AddFile4();
            }
        }

        private void AddText_Click(object sender, MouseButtonEventArgs e)
        {
            PickText aw = new PickText(main);
            aw.OnPicked = (int i) =>
            {
                byte[] dest = main.DestTextures[i].Texture;
                byte[] text = new byte[dest.Length - 0x80];
                for (int j = 0; j < text.Length; j++)
                {
                    text[j] = dest[j + 0x80];
                }
                int height = dest[0x22] + (dest[0x23] * 0x100);
                int width = dest[0x20] + (dest[0x21] * 0x100);
                TXA.Texture texture = new TXA.Texture
                {
                    DestTexture = main.DestTextures[i].Name,
                    Data = text
                };
                main.Textures.Add(texture);
                main.DecodedTextures.Add(CTT.Deswizzle(text, width, height, dest[0x1C]));
                AddFile4();
                for (int j = 1; j < 0x3FFFFFFF; j++)
                {
                    if (!main.Adresses.Contains(j))
                    {
                        main.Adresses.Add(j);
                        break;
                    }
                }
                if (Textures.Children[Textures.Children.Count - 1] is TextBox tb)
                {
                    selectedTextBox4 = tb;
                    tb.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#F04080");
                    foreach (TextBox textbox in Textures.Children)
                    {
                        if (textbox != selectedTextBox4)
                            textbox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020");
                    }
                    TextBox4_Click(null, null);
                }
            };
            aw.ShowDialog();
        }

        private void BadApple(object sender, MouseButtonEventArgs e)
        {
            PickText aw = new PickText(main);
            aw.OnPicked = (int i) =>
            {
                byte[] dest = main.DestTextures[i].Texture;
                byte[] text = new byte[dest.Length - 0x80];
                for (int j = 0; j < text.Length; j++)
                {
                    text[j] = dest[j + 0x80];
                }
                int height = dest[0x22] + (dest[0x23] * 0x100);
                int width = dest[0x20] + (dest[0x21] * 0x100);
                TXA.Texture texture = new TXA.Texture
                {
                    DestTexture = main.DestTextures[i].Name,
                    Data = text
                };
                string[] files2 = Directory.GetFiles($@"C:\Users\solom\Downloads\frames\downscaled", $"*.png", SearchOption.AllDirectories);
                for (int k = 0; k < files2.Length; k++)
                {
                    if (System.IO.File.Exists(files2[k]))
                    {
                        byte[] image = System.IO.File.ReadAllBytes(files2[k]);
                        byte[] textwheader = CTT.Swizzle(image, dest[0x1C]);
                        byte[] text2 = new byte[dest.Length - 0x80];
                        for (int l = 0; l < text.Length; l++)
                        {
                            text2[l] = textwheader[l + 0x80];
                        }
                        main.Textures.Add(new Texture { Data = text2, DestTexture = main.DestTextures[i].Name });
                        main.DecodedTextures.Add(SixLabors.ImageSharp.Image.Load(image));
                        AddFile4();
                        for (int j = 1; j < 0x3FFFFFFF; j++)
                        {
                            if (!main.Adresses.Contains(j))
                            {
                                main.Adresses.Add(j);
                                break;
                            }
                        }
                    }
                }
                if (Textures.Children[Textures.Children.Count - 1] is TextBox tb)
                {
                    selectedTextBox4 = tb;
                    tb.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#F04080");
                    foreach (TextBox textbox in Textures.Children)
                    {
                        if (textbox != selectedTextBox4)
                            textbox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#202020");
                    }
                    TextBox4_Click(null, null);
                }
            };
            aw.ShowDialog();
        }

        private void AddFrame_Click(object sender, MouseButtonEventArgs e)
        {
            int i = Files.Children.IndexOf(selectedTextBox);
            int j = Files2.Children.IndexOf(selectedTextBox2);
            TXA.Frame frame = new TXA.Frame
            {
                Length = 0,
                Length2 = 0,
                Texture = 0
            };
            main.Groups[i].Anims[j].Frames.Add(frame);
            Files3.Children.Clear();
            for (int k = 0; k < main.Groups[i].Anims[j].Frames.Count; k++)
            {
                AddFile3($"Frame {k + 1}");
            }
        }

        private void FileName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int j = 0;
            if (FileName.SelectedIndex < 0)
            {
                return;
            }
            foreach (var child in Files3.Children)
            {
                if (child is TextBox textBox && textBox == selectedTextBox3)
                {
                    foreach (AnimGroup group in main.Groups)
                    {
                        if (selectedTextBox.Text == group.Name)
                        {
                            foreach (Anim anim in group.Anims)
                            {
                                if (selectedTextBox2.Text == anim.Name)
                                {
                                    for (int i = 0; i < Files3.Children.Count; i++)
                                    {
                                        if (i == Files3.Children.IndexOf(selectedTextBox3))
                                        {
                                            if (FileName.SelectedIndex == 0)
                                            {
                                                anim.Frames[i].Texture = 0;
                                            }
                                            else
                                            {
                                                anim.Frames[i].Texture = main.Adresses[FileName.SelectedIndex - 1];
                                            }
                                            j = i;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            }
            AssignImage(j, 3);
        }

        private void Length_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!loaded)
            {
                return;
            }
            try
            {
                if (int.Parse(Length.Text) >= 65535)
                {
                    Length.Text = 65535.ToString();
                }
            }
            catch { }
            int i = Files.Children.IndexOf(selectedTextBox);
            int j = Files2.Children.IndexOf(selectedTextBox2);
            int k = Files3.Children.IndexOf(selectedTextBox3);
            try
            {
                main.Groups[i].Anims[j].Frames[k].Length = int.Parse(Length.Text);
            }
            catch { }
        }

        private void Length2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!loaded)
            {
                return;
            }
            try
            {
                if (int.Parse(Length2.Text) >= 65535)
                {
                    Length2.Text = 65535.ToString();
                }
            }
            catch { }
            int i = Files.Children.IndexOf(selectedTextBox);
            int j = Files2.Children.IndexOf(selectedTextBox2);
            int k = Files3.Children.IndexOf(selectedTextBox3);
            try
            {
                main.Groups[i].Anims[j].Frames[k].Length2 = int.Parse(Length2.Text);
            }
            catch { }
        }

        private void NumbersOnly(object sender, TextCompositionEventArgs e)
        {
            if (int.TryParse(e.Text, out int value))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
