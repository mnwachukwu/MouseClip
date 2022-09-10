using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MouseClip
{
    public class MouseClip
    {
        private static bool Messaged { get; set; }

        private static string ProcessName { get; set; }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        public static Rect Window;

        private static void Main(params string[] args)
        {
            ProcessName = args.FirstOrDefault();

            if (string.IsNullOrEmpty(ProcessName))
            {
                Console.WriteLine("No process name provided");
                return;
            }

            Console.WriteLine($"Waiting for {ProcessName}.exe to be opened or focused..." + Environment.NewLine);

            while (true)
            {
                var process = Process.GetProcessesByName(ProcessName).FirstOrDefault();

                if (process != null && ProcessIsFocused(ProcessName))
                {
                    var ptr = process.MainWindowHandle;
                    GetWindowRect(ptr, ref Window);
                    Cursor.Clip = new(new(Window.Left, Window.Top), new(Window.Width, Window.Height));

                    if (!Messaged)
                    {
                        Messaged = true;
                        Console.WriteLine($"{ProcessName}.exe opened or refocused. Clipping mouse.");
                        Cursor.Position = new(Window.Width / 2, Window.Height / 2);
                    }
                }
                else
                {
                    Cursor.Clip = new();

                    if (Messaged)
                    {
                        Messaged = false;
                        Console.WriteLine($"{ProcessName}.exe closed or lost focus. Unclipping mouse.");
                    }
                }

                Thread.Sleep(100);
            }
        }

        public static bool ProcessIsFocused(string processName)
        {
            if (string.IsNullOrEmpty(processName))
            {
                return false;
            }

            var runningProcesses = Process.GetProcessesByName(processName);
            var activeWindowHandle = GetForegroundWindow();

            return runningProcesses.Any(process => process.MainWindowHandle.Equals(activeWindowHandle));
        }
    }
}
