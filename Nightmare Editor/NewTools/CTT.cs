﻿using System;
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
    /// <summary>
    /// Reimplemented CTT Encoding/Decoding.
    /// Use Decode to turn a CTT file into a PNG.
    /// Use Encode to turn a PNG and CTT file into a new CTT file.
    /// </summary>
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

        public static int GetFormat(string file)
        {
            byte[] header;
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(0, SeekOrigin.Begin);
                header = new byte[0x80];
                fs.Read(header, 0, 0x80);
            }
            return header[0x1C];
        }

        /// <summary>
        /// Decode a CTT texture into a PNG file.
        /// </summary>
        /// <param name="file">Filepath containing a CTT file.</param>
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
            if (format == "ETC1" || format == "ETC1A4")
            {
                File.Copy(file, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileName(file)}", true);
                Toolkit.CTTUnpack(Path.GetFileName(file), $@"{Path.GetDirectoryName(file)}\");
                return;
            }
            image.SaveAsPng(file + "." + format + ".png");
        }

        /// <summary>
        /// Encode a PNG file into a CTT texture.
        /// </summary>
        /// <param name="file">Filepath containing a CTT file to replace (must be a real file).</param>
        /// <param name="texture">Filepath containing a PNG texture to Encode into the CTT file.</param>
        public static void Encode(string file, string texture)
        {
            byte[] data = File.ReadAllBytes(texture);
            int formatByte = GetFormat(file);
            Format formatenum = (Format)formatByte;
            int startIndex = file.Length + 1;
            string format = formatenum.ToString();
            if ((int)formatenum >= 12)
            {
                File.Copy(texture, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileName(file)}.{format}.png", true);
                File.Copy(file, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileName(file)}", true);
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
                byte[] newData = RGBA5551unpack(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 3)
            {
                byte[] newData = RGB565unpack(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 4)
            {
                byte[] newData = RGBA4444unpack(rawData);
                var image = Assemble(newData, width, height, true);
                return image;
            }
            else if (format == 5)
            {
                byte[] newData = LA8unpack(rawData);
                var image = Assemble(newData, width, height, true);
                return image;
            }
            else if (format == 6)
            {
                byte[] newData = HILO8unpack(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 7)
            {
                byte[] newData = L8unpack(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 8)
            {
                byte[] newData = A8unpack(rawData);
                var image = Assemble(newData, width, height, true);
                return image;
            }
            else if (format == 9)
            {
                byte[] newData = LA4unpack(rawData);
                var image = Assemble(newData, width, height, true);
                return image;
            }
            else if (format == 10)
            {
                byte[] newData = L4unpack(rawData);
                var image = Assemble(newData, width, height, false);
                return image;
            }
            else if (format == 11)
            {
                byte[] newData = A4unpack(rawData);
                var image = Assemble(newData, width, height, true);
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
            if (format == 1)
            {
                newData = RGB888pack(image);
            }
            else if (format == 2)
            {
                newData = RGBA5551pack(image);
            }
            else if (format == 3)
            {
                newData = RGB565pack(image);
            }
            else if (format == 4)
            {
                newData = RGBA4444pack(image);
            }
            else if (format == 5)
            {
                newData = LA8pack(image);
            }
            else if (format == 6)
            {
                newData = HILO8pack(image);
            }
            else if (format == 7)
            {
                newData = L8pack(image);
            }
            else if (format == 8)
            {
                newData = A8pack(image);
            }
            else if (format == 9)
            {
                newData = LA4pack(image);
            }
            else if (format == 10)
            {
                newData = L4pack(image);
            }
            else if (format == 11)
            {
                newData = A4pack(image);
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
            int total = 0;
            if (format == 0)
            {
                total = height * width * 4;
            }
            else if (format == 1)
            {
                total = height * width * 3;
            }
            else if (format >= 2 && format <= 6)
            {
                total = height * width * 2;
            }
            else if (format >= 7 && format <= 9)
            {
                total = height * width * 1;
            }
            else if (format >= 10 && format <= 11)
            {
                total = height * width / 2;
            }
            else if (format >= 12 && format <= 13)
            {
                total = (height / 4) * (width / 4) * 8;
            }
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
            var image32 = image.CloneAs<Rgba32>();

            int height = image.Height;
            int width = image.Width;

            int bytesPerPixel = 4;

            int bytes = width * height * bytesPerPixel;

            int tilesPerRow = (width + tileSize - 1) / tileSize;
            int tilesPerCol = (height + tileSize - 1) / tileSize;

            byte[] newData = new byte[bytes + 0x80];
            byte[] header = CTTHeader(width, height, (int)Format.RGBA8888);

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

                                            Rgba32 rgba = new Rgba32();
                                            rgba = image32.DangerousGetPixelRowMemory(imgY).Span[imgX];
                                            newData[count + 3] = rgba.R;
                                            newData[count + 2] = rgba.G;
                                            newData[count + 1] = rgba.B;
                                            newData[count + 0] = rgba.A;
                                            count += 4;
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
        /// Converts RGBA5551 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA5551 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] RGBA5551unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i += 2)
            {
                ushort pixel = (ushort)(ogData[i] | (ogData[i + 1] << 8));

                int r5 = (pixel >> 11) & 0x1F;
                int g5 = (pixel >> 6) & 0x1F;
                int b5 = (pixel >> 1) & 0x1F;
                int a1 = pixel & 0x1;

                byte r8 = (byte)((r5 << 3) | (r5 >> 2));
                byte g8 = (byte)((g5 << 3) | (g5 >> 2));
                byte b8 = (byte)((b5 << 3) | (b5 >> 2));
                byte a8 = 0;
                if (a1 == 1)
                {
                    a8 = 0xFF;
                }

                newData[j++] = (byte)a8;
                newData[j++] = (byte)b8;
                newData[j++] = (byte)g8;
                newData[j++] = (byte)r8;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGB565 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGB565 byte array.</param>
        /// <returns>Returns a byte array containing raw RGB888 data.</returns>
        public static byte[] RGB565unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 3 / 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i += 2)
            {
                ushort pixel = (ushort)(ogData[i] | (ogData[i + 1] << 8));

                int r5 = (pixel >> 11) & 0x1F;
                int g6 = (pixel >> 5) & 0x3F;
                int b5 = pixel & 0x1F;

                byte r8 = (byte)((r5 << 3) | (r5 >> 2));
                byte g8 = (byte)((g6 << 2) | (g6 >> 4));
                byte b8 = (byte)((b5 << 3) | (b5 >> 2));

                newData[j++] = (byte)b8;
                newData[j++] = (byte)g8;
                newData[j++] = (byte)r8;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA4444 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA4444 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] RGBA4444unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i += 2)
            {
                ushort pixel = (ushort)(ogData[i] | (ogData[i + 1] << 8));

                int r4 = (pixel >> 12) & 0xF;
                int g4 = (pixel >> 8) & 0xF;
                int b4 = (pixel >> 4) & 0xF;
                int a4 = pixel & 0xF;

                byte r8 = (byte)(r4 << 4 | r4);
                byte g8 = (byte)(g4 << 4 | g4);
                byte b8 = (byte)(b4 << 4 | b4);
                byte a8 = (byte)(a4 << 4 | a4);

                newData[j++] = (byte)a8;
                newData[j++] = (byte)b8;
                newData[j++] = (byte)g8;
                newData[j++] = (byte)r8;
            }
            return newData;
        }

        /// <summary>
        /// Converts LA8 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw LA8 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] LA8unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i += 2)
            {
                newData[j++] = ogData[i + 1];
                newData[j++] = ogData[i];
                newData[j++] = ogData[i];
                newData[j++] = ogData[i];
            }
            return newData;
        }

        /// <summary>
        /// Converts HILO8 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw HILO8 byte array.</param>
        /// <returns>Returns a byte array containing raw RGB888 data.
        /// The red channel represents the Hi channel.
        /// The blue channel represents the Lo channel.</returns>
        public static byte[] HILO8unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 3 / 2];

            int j = 0;
            for (int i = 0; i < ogData.Length; i += 2)
            {
                newData[j++] = ogData[i + 1];
                j++;
                newData[j++] = ogData[i];
            }
            return newData;
        }

        /// <summary>
        /// Converts L8 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw L8 byte array.</param>
        /// <returns>Returns a byte array containing raw RGB888 data.</returns>
        public static byte[] L8unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 3];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                newData[j++] = ogData[i];
                newData[j++] = ogData[i];
                newData[j++] = ogData[i];
            }
            return newData;
        }

        /// <summary>
        /// Converts A8 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw A8 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] A8unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 4];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                newData[j++] = ogData[i];
                newData[j++] = 0x00;
                newData[j++] = 0x00;
                newData[j++] = 0x00;
            }
            return newData;
        }

        /// <summary>
        /// Converts LA4 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw LA4 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] LA4unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 4];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                int a4 = (ogData[i] >> 4) & 0xF;
                int gray4 = ogData[i] & 0xF;

                byte a8 = (byte)(a4 << 4 | a4);
                byte gray8 = (byte)(gray4 << 4 | gray4);

                newData[j++] = a8;
                newData[j++] = gray8;
                newData[j++] = gray8;
                newData[j++] = gray8;
            }
            return newData;
        }

        /// <summary>
        /// Converts L4 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw L4 byte array.</param>
        /// <returns>Returns a byte array containing raw RGB888 data.</returns>
        public static byte[] L4unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 6];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                int gray4_1 = (ogData[i] >> 4) & 0xF;
                int gray4_2 = ogData[i] & 0xF;

                byte gray8_1 = (byte)(gray4_1 << 4 | gray4_1);
                byte gray8_2 = (byte)(gray4_2 << 4 | gray4_2);

                newData[j++] = gray8_1;
                newData[j++] = gray8_1;
                newData[j++] = gray8_1;
                newData[j++] = gray8_2;
                newData[j++] = gray8_2;
                newData[j++] = gray8_2;
            }
            return newData;
        }

        /// <summary>
        /// Converts A4 bytes into RGBA8888 bytes.
        /// </summary>
        /// <param name="ogData">Raw A4 byte array.</param>
        /// <returns>Returns a byte array containing raw RGBA8888 data.</returns>
        public static byte[] A4unpack(byte[] ogData)
        {
            byte[] newData = new byte[ogData.Length * 8];

            int j = 0;
            for (int i = 0; i < ogData.Length; i++)
            {
                int a4_1 = (ogData[i] >> 4) & 0xF;
                int a4_2 = ogData[i] & 0xF;

                byte a8_1 = (byte)(a4_1 << 4 | a4_1);
                byte a8_2 = (byte)(a4_2 << 4 | a4_2);

                newData[j++] = a8_1;
                j += 3;
                newData[j++] = a8_2;
                j += 3;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into RGB888 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw RGB888 data, with a CTT Header.</returns>
        public static byte[] RGB888pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.RGB888);
            byte[] newData = new byte[((ogData.Length - 0x80) * 3 / 4) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }
            int j = 0x80;
            for (int i = 0x81; i < ogData.Length; i++)
            {
                newData[j++] = ogData[i++];
                newData[j++] = ogData[i++];
                newData[j++] = ogData[i++];
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into RGBA5551 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw RGBA5551 data, with a CTT Header.</returns>
        public static byte[] RGBA5551pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.RGBA5551);
            byte[] newData = new byte[((ogData.Length - 0x80) / 2) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int a1 = ogData[i + 0] >> 7;
                int b5 = ogData[i + 1] >> 3;
                int g5 = ogData[i + 2] >> 3;
                int r5 = ogData[i + 3] >> 3;

                ushort bytes = (ushort)((r5 << 11) | (g5 << 6) | (b5 << 1) | a1);

                newData[j++] = (byte)(bytes & 0xFF);
                newData[j++] = (byte)((bytes >> 8) & 0xFF);
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into RGB565 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw RGB565 data, with a CTT Header.</returns>
        public static byte[] RGB565pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.RGB565);
            byte[] newData = new byte[((ogData.Length - 0x80) / 2) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int b5 = ogData[i + 1] >> 3;
                int g6 = ogData[i + 2] >> 2;
                int r5 = ogData[i + 3] >> 3;

                ushort bytes = (ushort)((r5 << 11) | (g6 << 5) | b5);

                newData[j++] = (byte)(bytes & 0xFF);
                newData[j++] = (byte)((bytes >> 8) & 0xFF);
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into RGBA4444 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw RGBA4444 data, with a CTT Header.</returns>
        public static byte[] RGBA4444pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.RGBA4444);
            byte[] newData = new byte[((ogData.Length - 0x80) / 2) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int a4 = ogData[i + 0] >> 4;
                int b4 = ogData[i + 1] >> 4;
                int g4 = ogData[i + 2] >> 4;
                int r4 = ogData[i + 3] >> 4;

                ushort bytes = (ushort)((r4 << 12) | (g4 << 8) | (b4 << 4) | a4);

                newData[j++] = (byte)(bytes & 0xFF);
                newData[j++] = (byte)((bytes >> 8) & 0xFF);
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into LA8 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw LA8 data, with a CTT Header.</returns>
        public static byte[] LA8pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.LA8);
            byte[] newData = new byte[((ogData.Length - 0x80) / 2) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int a8 = ogData[i];
                int r = ogData[i + 3];
                int g = ogData[i + 2];
                int b = ogData[i + 1];
                byte gray8 = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                ushort bytes = (ushort)((gray8 << 8) | a8);

                newData[j++] = (byte)(bytes & 0xFF);
                newData[j++] = (byte)((bytes >> 8) & 0xFF);
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into HILO8 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw HILO8 data, with a CTT Header.</returns>
        public static byte[] HILO8pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.HILO8);
            byte[] newData = new byte[((ogData.Length - 0x80) / 3 * 2) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int lo = ogData[i + 1];
                int hi = ogData[i + 3];
                ushort bytes = (ushort)((hi << 8) | lo);

                newData[j++] = (byte)(bytes & 0xFF);
                newData[j++] = (byte)((bytes >> 8) & 0xFF);
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into L8 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw L8 data, with a CTT Header.</returns>
        public static byte[] L8pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.L8);
            byte[] newData = new byte[((ogData.Length - 0x80) / 4) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int r = ogData[i + 3];
                int g = ogData[i + 2];
                int b = ogData[i + 1];
                byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                newData[j++] = gray;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into A8 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw A8 data, with a CTT Header.</returns>
        public static byte[] A8pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.A8);
            byte[] newData = new byte[((ogData.Length - 0x80) / 4) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                newData[j++] = ogData[i];
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into LA4 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw LA4 data, with a CTT Header.</returns>
        public static byte[] LA4pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.LA4);
            byte[] newData = new byte[((ogData.Length - 0x80) / 4) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int a4 = ogData[i] >> 4;
                int r = ogData[i + 3];
                int g = ogData[i + 2];
                int b = ogData[i + 1];
                int gray4 = (int)(0.299 * r + 0.587 * g + 0.114 * b) >> 4;
                byte pixel = (byte)((a4 << 4) | gray4);

                newData[j++] = pixel;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into L4 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw L4 data, with a CTT Header.</returns>
        public static byte[] L4pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.L4);
            byte[] newData = new byte[((ogData.Length - 0x80) / 8) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 8)
            {
                int r_1 = ogData[i + 3];
                int g_1 = ogData[i + 2];
                int b_1 = ogData[i + 1];
                int r_2 = ogData[i + 7];
                int g_2 = ogData[i + 6];
                int b_2 = ogData[i + 5];
                byte gray4_1 = (byte)((int)(0.299 * r_1 + 0.587 * g_1 + 0.114 * b_1) >> 4);
                byte gray4_2 = (byte)((int)(0.299 * r_2 + 0.587 * g_2 + 0.114 * b_2) >> 4);
                byte pixels = (byte)((gray4_1 << 4) | gray4_2);

                newData[j++] = pixels;
            }
            return newData;
        }

        /// <summary>
        /// Converts RGBA8888 bytes into A4 bytes.
        /// </summary>
        /// <param name="ogData">Raw RGBA8888 byte array, with CTT Header.</param>
        /// <returns>Returns a byte array containing raw A4 data, with a CTT Header.</returns>
        public static byte[] A4pack(byte[] ogData)
        {
            ushort width = (ushort)(ogData[0x20] | (ogData[0x21] << 8));
            ushort height = (ushort)(ogData[0x22] | (ogData[0x23] << 8));
            byte[] header = CTTHeader(width, height, (int)Format.A4);
            byte[] newData = new byte[((ogData.Length - 0x80) / 8) + 0x80];

            for (int i = 0; i < 0x80; i++)
            {
                newData[i] = header[i];
            }

            int j = 0x80;
            for (int i = 0x80; i < ogData.Length; i += 4)
            {
                int a4_1 = ogData[i] >> 4;
                int a4_2 = ogData[i + 4] >> 4;
                byte pixels = (byte)((a4_1 << 4) | a4_2);

                newData[j++] = pixels;
            }
            return newData;
        }
    }
}
