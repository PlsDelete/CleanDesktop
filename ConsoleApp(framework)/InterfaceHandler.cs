using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace CleanDesktop.ConsoleApp
{

    public class InterfaceHandler
    {
        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private static POINT lastCursorPos;

        public static void SetVisible(bool isVisible)
        {
            Task.WaitAll(
                Task.Run(() => SetVisibleIcons(isVisible)),
                Task.Run(() => SetVisibleTaskbar(isVisible)),
                Task.Run(() => SetVisibleCursor(isVisible))
                );
        }

        private static void SetVisibleIcons(bool isVisible)
        {
            //Получаем дескриптор рабочего стола
            IntPtr desktopIconsHandle = WindowHandler.GetDesktopIcons();
            SmoothHide.SmoothlyTransitionWindow(desktopIconsHandle, !isVisible);

            Console.WriteLine(isVisible ? "Иконки рабочего стола отображены." : "Иконки рабочего стола скрыты.");
        }

        private static void SetVisibleTaskbar(bool isVisible)
        {
            //Получаем дескриптор панели задач
            IntPtr taskbarHandle = WindowHandler.GetTaskbarWindow();

            //Скрыть панель задач
            SmoothHide.SmoothlyTransitionWindow(taskbarHandle, !isVisible);

            Console.WriteLine(isVisible ? "Панель задач отображена." : "Панель задач скрыта.");
        }

        private static void SetVisibleCursor(bool isVisible)
        {
            if (!isVisible)
            {
                if (lastCursorPos.X == 0 && lastCursorPos.Y == 0)
                {
                    GetCursorPos(out lastCursorPos);
                }
                SetCursorPos(2000, 0);
            }
            else
            {
                SetCursorPos(lastCursorPos.X, lastCursorPos.Y);
                lastCursorPos.X = 0;
                lastCursorPos.Y = 0;
            }

            Console.WriteLine(isVisible ? "Курсор отображен." : "Курсор скрыт.");

        }

        public static void ShowCurrsorPos()
        {
            POINT point = new POINT();
            GetCursorPos(out point);
            Console.WriteLine($"Cursoor pos: x - {point.X} y - {point.Y}");
        }
    }
}
