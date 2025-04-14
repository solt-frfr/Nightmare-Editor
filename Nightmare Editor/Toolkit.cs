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
                    File.Copy(file, file.Replace($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\work\", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\base\"));
                }
            }
            File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}");
        }

        public static void CTTPack(string filename, string path, string format)
        {
            string toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\ETC.exe";
            if (format != "etc")
            {
                toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\DDD Toolkit.exe";
            }
            string inputFile = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}.{format}.png";
            string newFile = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}";
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

            NewTools.CombineFiles(newFile, $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\og.ctt", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\new.ctt", (int)new FileInfo(newFile).Length);
            File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\og.ctt");
            File.Delete(newFile);
            File.Delete(inputFile);
            File.Move($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\new.ctt", newFile);

            Toolkit.CTTUnpack(filename, path);
            File.SetLastWriteTime(path + filename, DateTime.Now);
            File.SetLastWriteTime(path + filename + "." + format + ".bmp", DateTime.Now);
            File.SetLastWriteTime(path + filename + "." + format + ".png", DateTime.Now);
        }

        public static void CTTUnpack(string filename, string path)
        {
            string toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\ETC.exe";
            string inputFile = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}";
            string format = "etc";
            byte[] sig565 = new byte[] { 0x43, 0x54, 0x52, 0x54, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x80, 0x00, 0x40, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            using (var stream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
            {
                byte[] header = new byte[48];
                stream.Read(header, 0, 48);
                if (header == sig565)
                {
                    format = "565";
                    toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\DDD Toolkit.exe";
                }
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

            if (File.Exists(inputFile + "." + format + ".png"))
            {
                File.Move(inputFile, path + Path.GetFileName(inputFile), true);
                File.Move(inputFile + "." + format + ".png", path + Path.GetFileName(inputFile) + "." + format + ".png", true);
                File.Move(inputFile + "." + format + ".bmp", path + Path.GetFileName(inputFile) + "." + format + ".bmp", true);
            }
            else
            {
                CTTUnpack(filename, path, true);
            }
        }

        public static void CTTUnpack(string filename, string path, bool force565)
        {
            if (!force565)
            {
                CTTUnpack(filename, path);
            }
            else
            {
                string toolkitPath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\DDD Toolkit.exe";
                string inputFile = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\DDD-Toolkit\{filename}";
                string format = "565";
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

                if (File.Exists(inputFile + "." + format + ".png"))
                {
                    File.Delete(inputFile);
                    File.Move(inputFile + "." + format + ".png", path + Path.GetFileName(inputFile) + "." + format + ".png", true);
                    File.Move(inputFile + "." + format + ".bmp", path + Path.GetFileName(inputFile) + "." + format + ".bmp", true);
                }
                else
                {
                    File.Delete(inputFile);
                    File.Delete(path + filename);
                    return;
                }
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