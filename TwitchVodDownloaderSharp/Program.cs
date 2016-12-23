using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace TwitchVodDownloaderSharp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SetDllPath();
            ServicePointManager.DefaultConnectionLimit = 20000;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        static void SetDllPath()
        {
            bool is64 = Environment.Is64BitProcess;
            if (is64 == true)
            {
                SetDllDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin64"));
            }
            else
            {
                SetDllDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin32"));
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);
    }
}
