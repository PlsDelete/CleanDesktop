using System.Runtime.InteropServices;

namespace CleanDesktop.ConsoleApp
{
    public class UserActivity
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);


        public static int GetIdleTimeInSeconds()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO))
            };

            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint tickCount = GetTickCount();
                uint idleTime = tickCount - lastInputInfo.dwTime;
                return (int)(idleTime / 1000); // Переводим в секунды
            }
            else
            {
                Console.WriteLine("Не удалось получить информацию о последнем действии.");
                return 0;
            }
        }

    }
}
