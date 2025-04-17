using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection;
using SixLabors.ImageSharp.ColorSpaces;


namespace Nightmare_Editor.NewTools
{
    public static class CTT
    {
        public enum Format
        {
            RGBA8888 = 0,
            RGB888 = 1,
            RGBA5551 = 2, 
            RGB565 = 3,
            RGBA4444 = 4,
            LA8 = 5,
            HILO8 = 6,
            L8 = 7,
            A8 = 8,
            LA4 = 9,
            L4 = 10,
            A4 = 11,
            ETC1 = 12,
            ETC1A4 = 13,
        }

        public static void Decode(string file)
        {
            byte[] header;
            byte[] data;

            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(0, SeekOrigin.Begin);
                header = new byte[0x80];
                fs.Read(header, 0, 0x80);
                fs.Seek(0x80, SeekOrigin.Begin);
                data = new byte[fs.Length - 0x80];
                fs.Read(data, 0, data.Length);
            }
            int height = header[0x22] + (header[0x23] * 0x100);
            int width = header[0x20] + (header[0x21] * 0x100);
            Format format1 = (Format)header[0x1C];
            string format = format1.ToString();
            var image = Deswizzle(data, width, height, (int)format1);
            image.SaveAsPng(file + "." + format + ".png");
        }
        public static void Encode(string file, string texture)
        {
            byte[] data = File.ReadAllBytes(texture);
            byte[] buffer = new byte[0x80];
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, buffer.Length);
            }
            byte formatByte = buffer[0x1C]; // read byte from file
            Format formatenum = (Format)formatByte;
            int startIndex = file.Length + 1;
            string format = formatenum.ToString();
            if (format == "ETC1" || format == "ETC1A4")
            {
                string[] files2 = Directory.GetFiles(Path.GetDirectoryName(file), $"{Path.GetFileName(file)}.*.png", SearchOption.AllDirectories);
                File.Copy(files2[0], $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileName(file)}.{format}.png", true);
                Toolkit.CTTPack(Path.GetFileName(file), $@"{Path.GetDirectoryName(file)}\", format);
                return;
            }
            var image = Swizzle(data, (int)formatenum);
            File.WriteAllBytes(file, image);
            Decode(file);
        }
        public static Image Deswizzle(byte[] rawData, int width, int height, int format)
        {
            if (format == 0)
            {
                var image = Assemble(rawData, width, height, true);
                return image;
            }
            else if (format == 1)
            {
                var image = Assemble(rawData, width, height, false);
                return image;
            }
            else if (format == 2)
            {
                var image = Assemble(rawData, width, height, true);
                return image;
            }
            else if (format == 3)
            {
                byte[] newData = RGB565(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 4)
            {
                var image = Assemble(rawData, width, height, true);
                return image;
            }
            else
            {
                return null;
            }
        }

        public static byte[] Swizzle(byte[] rawData, int format)
        {
            byte[] image = Dissasemble(rawData);
            byte[] newData;
            var temp = Image.Load(rawData);
            bool alpha = temp.PixelType.BitsPerPixel.Equals(32);
            if (format == 3)
            {
                newData = RGB565(image, alpha);
            }
            else
            {
                newData = image;
            }
            return newData;
        }
        public static Image Assemble(byte[] rawData, int width, int height, bool alpha)
        {
            const int tileSize = 8;
            const int subtiles = 2;
            const int minitiles = 2;
            const int miniSize = tileSize / subtiles / minitiles;  // 2
            int bytesPerPixel = 3;
            if (alpha)
            {
                bytesPerPixel = 4;
            }

            int tilesPerRow = (width + tileSize - 1) / tileSize;
            int tilesPerCol = (height + tileSize - 1) / tileSize;

            var image32 = new Image<Rgba32>(width, height);
            var image24 = new Image<Rgb24>(width, height);

            int tileSizeInPixels = tileSize * tileSize;
            int tileSizeInBytes = tileSizeInPixels * bytesPerPixel;

            int tileIndex = 0;

            for (int tileY = 0; tileY < tilesPerCol; tileY++)
            {
                for (int tileX = 0; tileX < tilesPerRow; tileX++)
                {
                    int tileBaseOffset = tileIndex * tileSizeInBytes;

                    for (int subY = 0; subY < subtiles; subY++)
                    {
                        for (int subX = 0; subX < subtiles; subX++)
                        {
                            for (int miniY = 0; miniY < minitiles; miniY++)
                            {
                                for (int miniX = 0; miniX < minitiles; miniX++)
                                {
                                    for (int py = 0; py < miniSize; py++)
                                    {
                                        for (int px = 0; px < miniSize; px++)
                                        {
                                            // Compute relative pixel position in tile
                                            int localX = subX * (minitiles * miniSize) + miniX * miniSize + px;
                                            int localY = subY * (minitiles * miniSize) + miniY * miniSize + py;

                                            int imgX = tileX * tileSize + localX;
                                            int imgY = tileY * tileSize + localY;

                                            if (imgX >= width || imgY >= height)
                                                continue;

                                            int pixelIndexInTile =
                                                (((subY * subtiles + subX) * minitiles * minitiles) +
                                                 (miniY * minitiles + miniX)) * (miniSize * miniSize)
                                                + (py * miniSize + px);

                                            int byteOffset = tileBaseOffset + pixelIndexInTile * bytesPerPixel;

                                            byte r = 0x00;
                                            byte g = 0x00;
                                            byte b = 0x00;
                                            byte a = 0x00;

                                            if (alpha)
                                            {
                                                r = rawData[byteOffset + 3];
                                                g = rawData[byteOffset + 2];
                                                b = rawData[byteOffset + 1];
                                                a = rawData[byteOffset + 0];
                                                image32.DangerousGetPixelRowMemory(imgY).Span[imgX] = new Rgba32(r, g, b, a);
                                            }
                                            else
                                            {
                                                r = rawData[byteOffset + 2];
                                                g = rawData[byteOffset + 1];
                                                b = rawData[byteOffset + 0];
                                                image24.DangerousGetPixelRowMemory(imgY).Span[imgX] = new Rgb24(r, g, b);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    tileIndex++;
                }
            }
            if (alpha)
            {
                return image32;
            }
            else
            {
                return image24;
            }
        }

        public static byte[] CTTHeader(int width, int height, int format)
        {
            int bpp = 4;
            if (format == 0)
            {
                bpp = 4;
            }
            else if (format == 1)
            {
                bpp = 3;
            }
            else if (format == 2 || format == 3 || format == 4)
            {
                bpp = 2;
            }
            int total = height * width;
            byte[] header = new byte[0x80];

            header[0x00] = 0x43;
            header[0x01] = 0x54;
            header[0x02] = 0x52;
            header[0x03] = 0x54;
            header[0x08] = 0x10;
            header[0x0C] = 0x80;
            header[0x14] = (byte)(total & 0xFF);
            header[0x15] = (byte)((total >> 8) & 0xFF);
            header[0x16] = (byte)((total >> 16) & 0xFF);
            header[0x17] = (byte)((total >> 24) & 0xFF);
            header[0x1C] = (byte)format;
            header[0x20] = (byte)(width & 0xFF);
            header[0x21] = (byte)((width >> 8) & 0xFF);
            header[0x22] = (byte)(height & 0xFF);
            header[0x23] = (byte)((height >> 8) & 0xFF);
            return header;
        }

        public static byte[] Dissasemble(byte[] rawData)
        {
            const int tileSize = 8;
            const int subtiles = 2;
            const int minitiles = 2;
            const int miniSize = tileSize / subtiles / minitiles;  // 2

            var image = Image.Load(rawData);
            var image24 = image.CloneAs<Rgb24>();
            var image32 = image.CloneAs<Rgba32>();

            bool alpha = image.PixelType.BitsPerPixel.Equals(32);
            int height = image.Height;
            int width = image.Width;

            int bytesPerPixel = 3;
            if (alpha)
            {
                bytesPerPixel = 4;
            }

            int bytes = width * height * bytesPerPixel;

            int tilesPerRow = (width + tileSize - 1) / tileSize;
            int tilesPerCol = (height + tileSize - 1) / tileSize;

            byte[] newData = new byte[bytes + 0x80];
            byte[] header = CTTHeader(width, height, 0);

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int count = 0x80;
            int tileSizeInPixels = tileSize * tileSize;
            int tileSizeInBytes = tileSizeInPixels * bytesPerPixel;

            int tileIndex = 0;

            for (int tileY = 0; tileY < tilesPerCol; tileY++)
            {
                for (int tileX = 0; tileX < tilesPerRow; tileX++)
                {
                    int tileBaseOffset = tileIndex * tileSizeInBytes;

                    for (int subY = 0; subY < subtiles; subY++)
                    {
                        for (int subX = 0; subX < subtiles; subX++)
                        {
                            for (int miniY = 0; miniY < minitiles; miniY++)
                            {
                                for (int miniX = 0; miniX < minitiles; miniX++)
                                {
                                    for (int py = 0; py < miniSize; py++)
                                    {
                                        for (int px = 0; px < miniSize; px++)
                                        {
                                            // Compute relative pixel position in tile
                                            int localX = subX * (minitiles * miniSize) + miniX * miniSize + px;
                                            int localY = subY * (minitiles * miniSize) + miniY * miniSize + py;

                                            int imgX = tileX * tileSize + localX;
                                            int imgY = tileY * tileSize + localY;

                                            if (imgX >= width || imgY >= height)
                                                continue;

                                            int pixelIndexInTile =
                                                (((subY * subtiles + subX) * minitiles * minitiles) +
                                                 (miniY * minitiles + miniX)) * (miniSize * miniSize)
                                                + (py * miniSize + px);

                                            int byteOffset = tileBaseOffset + pixelIndexInTile * bytesPerPixel;

                                            if (alpha)
                                            {
                                                Rgba32 rgba = new Rgba32();
                                                rgba = image32.DangerousGetPixelRowMemory(imgY).Span[imgX];
                                                newData[count + 3] = rgba.R;
                                                newData[count + 2] = rgba.G;
                                                newData[count + 1] = rgba.B;
                                                newData[count + 0] = rgba.A;
                                                count += 4;
                                            }
                                            else
                                            {
                                                Rgb24 rgb = new Rgb24();
                                                rgb = image24.DangerousGetPixelRowMemory(imgY).Span[imgX];
                                                newData[count + 2] = rgb.R;
                                                newData[count + 1] = rgb.G;
                                                newData[count + 0] = rgb.B;
                                                count += 3;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    tileIndex++;
                }
            }
            return newData;
        }

        /// <summary>
        /// Converts RGB565 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGB565 byte array.</param>
        /// <returns>Returns a byte array containing raw RGB888 data.</returns>
        public static byte[] RGB565(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 3 / 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                ushort pixel = (ushort)(ogData[i] | (ogData[i + 1] << 8));

                int r5 = (pixel >> 11) & 0x1F;
                int g6 = (pixel >> 5) & 0x3F;
                int b5 = pixel & 0x1F;

                byte r8 = (byte)((r5 << 3) | (r5 >> 2));
                byte g8 = (byte)((g6 << 2) | (g6 >> 4));
                byte b8 = (byte)((b5 << 3) | (b5 >> 2));
                i++;
                newData[j++] = (byte)b8;
                newData[j++] = (byte)g8;
                newData[j++] = (byte)r8;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGB888 bytes or RGBA8888 bytes into RGB565 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGB888 or RGBA8888 byte array.</param>
        /// <param name="alpha">Whether the data is RGB888 or RGBA8888. If RGBA8888, the data contains an alpha channel, and this bool should be marked as true.</param>
        /// <returns>Returns a byte array containing raw RGB565 data.</returns>
        public static byte[] RGB565(byte[] ogData, bool alpha)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.RGB565);
            if (alpha)
            {
                byte[] newData = new byte[((ogData.Length - 0x80) / 2) + 0x80];

                for (int i = 0; i < 0x80; i++)
                {
                    newData[i] = header[i];
                }

                int j = 0x80;
                for (int i = 0x80; i < ogData.Length; i++)
                {
                    int b5 = ogData[i + 1] >> 3;
                    int g6 = ogData[i + 2] >> 2;
                    int r5 = ogData[i + 3] >> 3;

                    ushort bytes = (ushort)((r5 << 11) | (g6 << 5) | b5);

                    i++;
                    i++;
                    i++;

                    newData[j++] = (byte)(bytes & 0xFF);
                    newData[j++] = (byte)((bytes >> 8) & 0xFF);
                }
                return newData;
            }
            else
            {
                byte[] newData = new byte[((ogData.Length - 0x80) * 2 / 3) + 0x80];

                for (int i = 0x80; i < 0x80; i++)
                {
                    newData[i] = header[i];
                }

                int j = 0;
                for (int i = 0; i < ogData.Length; i++)
                {
                    int b5 = ogData[i + 0] >> 3;
                    int g6 = ogData[i + 1] >> 2;
                    int r5 = ogData[i + 2] >> 3;

                    ushort bytes = (ushort)((r5 << 11) | (g6 << 5) | b5);

                    i++;
                    i++;
                    i++;

                    newData[j++] = (byte)(bytes & 0xFF);
                    newData[j++] = (byte)((bytes >> 8) & 0xFF);
                }
                return newData;
            }
        }
    }
}
