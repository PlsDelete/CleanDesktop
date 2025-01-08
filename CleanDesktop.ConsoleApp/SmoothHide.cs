using System;
using System.Runtime.InteropServices;
using System.Threading;

class SmoothHide
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_LAYERED = 0x80000;
    private const int LWA_ALPHA = 0x2;

    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int Left;
        public int Right;
        public int Top;
        public int Bottom;
    }

    public static void SmoothlyTransitionWindow(IntPtr hWnd, bool hide)
    {
        if (hWnd == IntPtr.Zero)
        {
            string errStr = "Не удалось найти окно.";
            Console.WriteLine(errStr);
            return;
        }
        // Установить стиль окна для прозрачности
        int extendedStyle = (int)GetWindowLong(hWnd, GWL_EXSTYLE);
        SetWindowLong(hWnd, GWL_EXSTYLE, (IntPtr)(extendedStyle | WS_EX_LAYERED));

        // Установить начальную и конечную прозрачность
        byte startAlpha = hide ? (byte)255 : (byte)0;
        byte endAlpha = hide ? (byte)0 : (byte)255;

        int step = hide ? -5 : 5;

        for (int i = startAlpha; hide ? i >= endAlpha : i <= endAlpha; i += step)
        {
            SetLayeredWindowAttributes(hWnd, 0, (byte)i, LWA_ALPHA);
            Thread.Sleep(10); // Задержка для плавности
        }
    }


}