using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenCapture
{
    class Program
    {
        const string ImagesDir = "screenshotimgs";
        const int RollbackCount = 20;

        static void RollbackImages()
        {
            if (File.Exists(Path.Combine(ImagesDir, RollbackCount + ".png")))
            {
                File.Delete(Path.Combine(ImagesDir, RollbackCount + ".png"));
            }

            for (int i = RollbackCount - 1; i >= 0; i--)
            {
                var filename = Path.Combine(ImagesDir, i + ".png");
                var newFilename = Path.Combine(ImagesDir, (i + 1) + ".png");
                if (File.Exists(filename))
                {
                    File.Move(filename, newFilename);
                }
            }
        }

        static void RemoveOldestImage()
        {
            var files = Directory.GetFiles(ImagesDir, "*.png");
            if (files.Length > 20)
            {
                var filesByDate = files.OrderBy(f => File.GetLastWriteTime(f)).Take(files.Length - 20);
                foreach (var f in filesByDate) File.Delete(f);
            }
        }

        static void Main(string[] args)
        {
            var gameArg = "";
            if (args.Length > 0) gameArg = args[0];
    
            //var pName = "ed6_win3_DX9";
            var pName = "ff9";
            var uglyBorderSize = 4;
            var barHeight = 26;
            var sideTrim = 0;
            switch (gameArg)
            {
                case "ed6t3":
                    pName = "ed6_win3_DX9";
                    uglyBorderSize = 3;
                    barHeight = uglyBorderSize + 24;
                    break;
                case "ed7z":
                    pName = "ED_ZERO";
                    uglyBorderSize = 3;
                    barHeight = uglyBorderSize + 24;
                    break;
                case "ff9":
                    sideTrim = 190;
                    break;
                default:
                    if (args.Length > 1)
                        pName = args[1];
                    break;
            }
            Process p = null;

            try
            {
                var processes = Process.GetProcessesByName(pName);
                foreach (var proc in processes)
                {
                    if (proc.MainWindowHandle != IntPtr.Zero)
                    {
                        p = proc;
                        break;
                    }
                }
            }
            catch
            {
                return;
            }

            if (!Directory.Exists(ImagesDir))
            {
                Directory.CreateDirectory(ImagesDir);
            }

            PrintScreen ps = new PrintScreen();

            if (p == null)
            {
                foreach (var proc in Process.GetProcesses())
                {
                    Console.WriteLine(proc.ProcessName);
                }
            }

            //Console.WriteLine(FindWindow.GetProcessWindows(p.Id)[0]);
            var img = new Bitmap(ps.CaptureWindow(p.MainWindowHandle));

            //var uglyBorderSize = 3;
            //var barHeight = uglyBorderSize;
            //var lastColor = img.GetPixel(uglyBorderSize, uglyBorderSize);
            //var color = lastColor;
            //do
            //{
            //    lastColor = color;
            //    barHeight++;
            //    color = img.GetPixel(uglyBorderSize, barHeight);
            //} while (color == lastColor);

            var cropped = img.Clone(new Rectangle(uglyBorderSize + sideTrim, barHeight, img.Width - 2 * (uglyBorderSize + sideTrim), img.Height - barHeight - uglyBorderSize), PixelFormat.DontCare);

            //RollbackImages();

            var guid = Guid.NewGuid().ToString();
            Console.WriteLine("id:" + guid);

            cropped.Save(Path.Combine(ImagesDir, guid + ".png"), ImageFormat.Png);
            RemoveOldestImage();

            //ps.CaptureWindowToFile(p.MainWindowHandle, "ed6.png", ImageFormat.Png);
        }
    }
}
