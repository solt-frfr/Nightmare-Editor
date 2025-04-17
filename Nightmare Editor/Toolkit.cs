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
using System.Diagnostics;
using System.Reflection;

namespace Nightmare_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public static class Toolkit
    {
        public static void RbinExtract(string filename)
        {
            string toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\DDD Toolkit.exe";
            string inputFile = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{toolkitPath}\" \"{inputFile}\"\"",
                UseShellExecute = true,
                WorkingDirectory = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\"
            };

            Debug.WriteLine(startInfo.FileName);
            Debug.WriteLine(startInfo.Arguments);
            Process process = new Process
            {
                StartInfo = startInfo
            };

            // Start the process
            process.Start();
            process.WaitForExit();
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\");
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\");
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(filename)}");
            Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(filename)}", true);
            Directory.Move($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{Path.GetFileNameWithoutExtension(filename)}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\{Path.GetFileNameWithoutExtension(filename)}");
            List<string> Paths = new List<string>();
            // Get all files in the folder
            string[] files = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\", "*.*", SearchOption.AllDirectories);

            // Iterate and print each file path
            foreach (string file in files)
            {
                string filetrim = file.Replace($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\", "");
                if (!filetrim.Contains(".bmp") && !filetrim.Contains(".png") && (filetrim.Length - filetrim.Replace("\\", "").Length <= 1))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file.Replace($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\")));
                    File.Copy(file, file.Replace($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\"), true);
                }
            }
            File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}");
        }

        public static void CTTPack(string filename, string path, string format)
        {
            string toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\ETC.exe";
            if (format != "ETC1" && format != "ETC1A4")
            {
                toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\DDD Toolkit.exe";
            }
            string inputFile = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}.{format}.png";
            string newFile = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}";
            File.Copy(newFile, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\og.ctt");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{toolkitPath}\" \"{inputFile}\"\"",
                UseShellExecute = true,
                WorkingDirectory = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\"
            };

            Debug.WriteLine(startInfo.FileName);
            Debug.WriteLine(startInfo.Arguments);
            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();

            NewTools.Misc.CombineFiles(newFile, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\og.ctt", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\new.ctt", (int)new FileInfo(newFile).Length);
            File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\og.ctt");
            File.Delete(newFile);
            File.Delete(inputFile);
            File.Move($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\new.ctt", newFile);

            Toolkit.CTTUnpack(filename, path);
            try
            {
                File.SetLastWriteTime(path + filename, DateTime.Now);
                File.SetLastWriteTime(path + filename + "." + format + ".bmp", DateTime.Now);
                File.SetLastWriteTime(path + filename + "." + format + ".png", DateTime.Now);
            }
            catch { }
        }

        public static void CTTUnpack(string filename, string path)
        {
            string toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\ETC.exe";
            string inputFile = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}";
            byte[] buffer = new byte[0x80];
            using (FileStream fs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, buffer.Length);
            }
            byte formatByte = buffer[0x1C]; // read byte from file
            NewTools.CTT.Format formatenum = (NewTools.CTT.Format)formatByte;
            string format = formatenum.ToString();
            if (format != "ETC1" && format != "ETC1A4")
            {
                toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\DDD Toolkit.exe";
            }
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{toolkitPath}\" \"{inputFile}\"\"",
                UseShellExecute = true,
                WorkingDirectory = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\"
            };

            Debug.WriteLine(startInfo.FileName);
            Debug.WriteLine(startInfo.Arguments);
            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();

            string[] files2 = Directory.GetFiles($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\", $"{filename}.*.png", SearchOption.AllDirectories);
            string file2 = files2[0];

            if (File.Exists(file2))
            {
                File.Move(inputFile, path + Path.GetFileName(inputFile), true);
                File.Move(file2, path + Path.GetFileName(inputFile) + "." + format + ".png", true);
                File.Move(file2.Replace(".png", ".bmp"), path + Path.GetFileName(file2.Replace(".png", ".bmp")), true);
            }
        }

        public static void ArcUnpack(string filename, string path)
        {
            string toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\DDD Toolkit.exe";
            string inputFile = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C \"\"{toolkitPath}\" \"{inputFile}\"\"",
                UseShellExecute = true,
                WorkingDirectory = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\"
            };

            Debug.WriteLine(startInfo.FileName);
            Debug.WriteLine(startInfo.Arguments);
            Process process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit();

            File.Delete(inputFile);
            if (Directory.Exists(path + Path.GetFileNameWithoutExtension(filename)))
                Directory.Delete(path + Path.GetFileNameWithoutExtension(filename), true);
            if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\" + Path.GetFileNameWithoutExtension(filename)))
                Directory.Move($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\" + Path.GetFileNameWithoutExtension(filename), path + Path.GetFileNameWithoutExtension(filename));
        }
    }
}