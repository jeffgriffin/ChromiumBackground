using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChromiumBackground.Properties;

namespace ChromiumBackground
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = System.Windows.WindowState.Maximized;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            this.FramelessOnSourceInitialized(e);

            System.IntPtr handle = (new WindowInteropHelper(this)).Handle; //ignore everything
            HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WindowProc));

            IntPtr hWindow = FindWindow(null, this.Title); //move to back
            IntPtr hDesktop = FindWindow("ProgMan", null);
            SetParent(handle, hDesktop);
            try
            {
                var source = Properties.Settings.Default.UserSource;
                if (string.IsNullOrWhiteSpace(source))
                    source = Properties.Settings.Default.Source;
                _webControl.Source = new UriBuilder(source).Uri;
            }
            catch (Exception){}

            new PipeHandler().StartListening(
                message =>
                    {
                        var source = message;
                        Properties.Settings.Default.UserSource = source;
                        this.Dispatcher.Invoke(() => _webControl.Source = new Uri(source));
                    });

            base.OnSourceInitialized(e);
        }

        private const int WM_CLOSE = 0x0010;
        private const int WM_MOVE = 0x0003;
        private const int WM_ACTIVATEAPP = 0x001C;
        private const int WM_SIZE = 0x0005;
        private static System.IntPtr WindowProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            if (msg == WM_MOVE || msg == WM_CLOSE || msg == WM_ACTIVATEAPP || msg == WM_SIZE)
            {
                handled = true;
            }

            return (System.IntPtr)0;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)] public static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPTStr)] string lpClassName, [MarshalAs(UnmanagedType.LPTStr)] string lpWindowName); 
 
        [DllImport("user32.dll")] 
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //Set the window style to noactivate.
            var helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    }
}
