using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using Nightmare_Editor.NewTools;
using SixLabors.ImageSharp;
using static Nightmare_Editor.NewTools.TXA;

namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for PickText.xaml
    /// </summary>
    public partial class PickText : Window
    {
        public Action<int> OnPicked;
        public List<DestTexture> textures;
        public PickText(TXA.TXAFile txa)
        {
            InitializeComponent();
            textures = txa.DestTextures;
            foreach (TXA.DestTexture text in textures)
            {
                Drop.Items.Add(text.Name);
            }
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (Drop.SelectedIndex < 0)
            {
                Error.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                OnPicked?.Invoke(Drop.SelectedIndex);
                Close();
            }
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

        private void Drop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int height = textures[Drop.SelectedIndex].Texture[0x22] + (textures[Drop.SelectedIndex].Texture[0x23] * 0x100);
            int width = textures[Drop.SelectedIndex].Texture[0x20] + (textures[Drop.SelectedIndex].Texture[0x21] * 0x100);
            int format = textures[Drop.SelectedIndex].Texture[0x1C];

            byte[] textwheader = textures[Drop.SelectedIndex].Texture;
            byte[] text = new byte[textwheader.Length - 0x80];
            for (int j = 0; j < text.Length; j++)
            {
                text[j] = textwheader[j + 0x80];
            }

            Texture.Source = ConvertToImageSource(CTT.Deswizzle(text, width, height, format));
        }
    }
}
