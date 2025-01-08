using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace CleanDesktop.ConsoleApp
{
    public class WindowHandler
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern long GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_STYLE = -16;
        private const long WS_OVERLAPPED = 0x00000000L;

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        // Структуры WinAPI
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public Rectangle ToRectangle() => new Rectangle(Left, Top, Right - Left, Bottom - Top);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }


        public static IntPtr GetTaskbarWindow()
        {
            var taskbarHandle = FindWindow("Shell_TrayWnd", null);            
            return taskbarHandle;
        }

        // Получить десктоп иконки
        public static IntPtr GetDesktopIcons()
        {
            IntPtr desktopPtr = FindWindow("Progman", null);
            IntPtr iconsPtr = IntPtr.Zero;

            // Сначала ищем внутри "Progman"
            if (desktopPtr != IntPtr.Zero)
            {
                IntPtr shellView = FindWindowEx(desktopPtr, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shellView != IntPtr.Zero)
                {
                    iconsPtr = FindWindowEx(shellView, IntPtr.Zero, "SysListView32", null);
                }
            }

            // Если не нашли, ищем в "WorkerW"
            if (iconsPtr == IntPtr.Zero)
            {
                IntPtr workerW = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "WorkerW", null);
                while (workerW != IntPtr.Zero)
                {
                    IntPtr shellView = FindWindowEx(workerW, IntPtr.Zero, "SHELLDLL_DefView", null);
                    if (shellView != IntPtr.Zero)
                    {
                        iconsPtr = FindWindowEx(shellView, IntPtr.Zero, "SysListView32", null);
                        if (iconsPtr != IntPtr.Zero)
                            break;
                    }

                    workerW = FindWindowEx(IntPtr.Zero, workerW, "WorkerW", null);
                }
            }
            return iconsPtr;
        }


       
        public static List<(string WindowTitle, bool IsFullscreen)> GetOpenWindows()
        {
            var windows = new List<(string, bool)>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        var builder = new StringBuilder(length + 1);
                        GetWindowText(hWnd, builder, builder.Capacity);

                        string title = builder.ToString();
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            bool isFullscreen = IsWindowFullscreen(hWnd);
                            windows.Add((title, isFullscreen));
                        }
                    }
                }
                return true; // Продолжить перечисление
            }, IntPtr.Zero);

            return windows;
        }
        private static bool IsUserWindow(IntPtr hWnd)
        {
            // Проверяем, имеет ли окно родителя (системные окна часто имеют родителя)
            if (GetParent(hWnd) != IntPtr.Zero)
                return false;

            // Проверяем стиль окна
            long style = GetWindowLong(hWnd, GWL_STYLE);
            if ((style & WS_OVERLAPPED) == 0)
                return false;

            // Исключаем окна без заголовка
            int length = GetWindowTextLength(hWnd);
            if (length == 0)
                return false;

            return true;
        }
        private static bool IsWindowFullscreen(IntPtr hWnd)
        {
            if (GetWindowRect(hWnd, out RECT windowRect))
            {
                IntPtr hMonitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
                if (hMonitor != IntPtr.Zero)
                {
                    var monitorInfo = new MONITORINFO { cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO)) };
                    if (GetMonitorInfo(hMonitor, ref monitorInfo))
                    {
                        Rectangle monitorBounds = monitorInfo.rcMonitor.ToRectangle();
                        Rectangle windowBounds = windowRect.ToRectangle();

                        // Сравнить размеры окна с размерами монитора
                        return monitorBounds.Equals(windowBounds);
                    }
                }
            }
            return false;
        }


        public static void HideDesktopIcons()
        {
            var data = new APPBARDATA();
            data.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));

            // Hide the taskbar or desktop icons by setting specific flags or using SHAppBarMessage
            SHAppBarMessage(ABM_GETSTATE, ref data);
            // Additional code to hide the desktop icons based on the flags
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public uint dwState;
            public uint dwTaskbarPos;
            public uint dwReserved;
        }
        [DllImport("shell32.dll")]
        public static extern int SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

        const uint ABM_GETSTATE = 0x00000004;
    }
}
