using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SharpShell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        public MainWindow()
        {
            InitializeComponent();

            Instance = this;
        }

        private void Click1(object sender, RoutedEventArgs e)
        {
            var process = Process.Start(new ProcessStartInfo("csharprepl"));

            IntPtr processWindowHandle = default;
            while (processWindowHandle == default)
            {
                processWindowHandle = process.MainWindowHandle;
            }
            HostConsole(processWindowHandle);
        }

        void Click2(object sender, RoutedEventArgs e)
        {
            AllocConsole();
            var consoleWindowHandle = GetConsoleWindow();
            HostConsole(consoleWindowHandle);

            new Thread(
                () =>
                {
                    const string CSharpReplDir = @"D:\API\.NET\CSharpRepl\CSharpRepl\bin\Debug\net6.0\";
                    var assemblies = Directory.EnumerateFiles(CSharpReplDir, "*.dll")
                        .Select(path => (Name: System.IO.Path.GetFileNameWithoutExtension(path), Assembly: Assembly.LoadFrom(path)))
                        .ToDictionary(t => t.Name, t => t.Assembly);
                    var csharpReplMain = assemblies["CSharpRepl"].EntryPoint;
                    csharpReplMain.Invoke(null, new object[] { new string[] { "-r", Assembly.GetExecutingAssembly().Location } });
                })
            { IsBackground = true }.Start();

            [DllImport("kernel32.dll")]
            static extern IntPtr GetConsoleWindow();

            [DllImport("kernel32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool AllocConsole();
        }

        void HostConsole(IntPtr consoleWindowHandle)
        {
            hostGrid.Width = 960;
            hostGrid.Height = 480;
            var consolePanel = new System.Windows.Forms.Panel() { Width = (int)hostGrid.Width, Height = (int)hostGrid.Height };
            windowsFormsHost.Child = consolePanel;

            const int GWL_STYLE = -16;
            const uint WS_VISIBLE = 0x10000000;
            SetParent(consoleWindowHandle, consolePanel.Handle);
            SetWindowLong(consoleWindowHandle, GWL_STYLE, WS_VISIBLE);
            MoveWindow(consoleWindowHandle, 0, 0, consolePanel.Width, consolePanel.Height, true);

            [DllImport("user32.dll")]
            static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

            [DllImport("user32.dll")]
            static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        }
    }
}
