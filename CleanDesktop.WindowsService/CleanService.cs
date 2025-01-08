using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Timers;

namespace CleanDesktop.WindowsService
{

    public partial class CleanService : ServiceBase
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSEnumerateSessions(
        IntPtr hServer,
        int Reserved,
        int Version,
        ref IntPtr ppSessionInfo,
        ref int pCount);
        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pMemory);
        [DllImport("kernel32.dll")]
        public static extern uint WTSGetActiveConsoleSessionId();


        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public int SessionId; // Идентификатор сессии
            public int State;     // Состояние сессии
            [MarshalAs(UnmanagedType.LPStr)]
            public string pWinStationName; // Имя сессии
        }


        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private const int WTS_CURRENT_SERVER_HANDLE = 0;
        private const int WTS_ACTIVE = 0x00000000;


        private System.Timers.Timer _timer;
        public CleanService()
        {
            InitializeComponent();
            this.CanStop = true; // службу можно остановить
        }
        internal void TestStartupAndStop(string[] args)
        {




            //while (true)
            //{
            //    CheckIdleTime(null, null);
            //    Thread.Sleep(1000);

            //}



            //SetVisibleShortcuts();
            this.OnStart(args);
            //this.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            Log("Service start!");
            CheckIdleTime();
            _timer = new System.Timers.Timer(1000); // Проверяем каждую секунду
            _timer.Elapsed += CheckIdleTime;
            _timer.Start();
        }

        protected override void OnStop()
        {
            Log("Service stop!");
            _timer.Stop();
            _timer.Dispose();
        }

        private void CheckIdleTime()
        {
            var query = "SELECT * FROM Win32_ComputerSystem";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject obj in searcher.Get())
            {
                Console.WriteLine($"UserName: {obj["UserName"]}");
            }
        }















        private void CheckIdleTime(object sender, ElapsedEventArgs e)
        {
            Log("CheckIdleTime");
            uint idleTime = GetIdleTimeInSeconds();
            if (idleTime > 5) // Если бездействие больше n cекунд
            {
                Log("Hide!");
                SetVisibleShortcuts(false);
            }
            else
            {
                Log("Show!");
                SetVisibleShortcuts(true);
            }
            Log($"Idle time: {idleTime}");
        }

        private int GetIdleTime()
        {
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            GetLastInputInfo(ref lastInputInfo);
            uint idleTime = (uint)Environment.TickCount - lastInputInfo.dwTime;
            return (int)(idleTime / 60000); // Время в секундах
        }

        private void SetVisibleShortcuts(bool isVisible)
        {
            // Найти окно рабочего стола
            IntPtr desktopPtr = FindWindow("Progman", null);
            IntPtr iconsPtr = GetDesktopIconsWindow(desktopPtr);

            if (iconsPtr != IntPtr.Zero)
            {
                switch (isVisible)
                {
                    case true:
                        ShowWindow(iconsPtr, SW_SHOW);
                        break;
                    case false:
                        ShowWindow(iconsPtr, SW_HIDE);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Log("Не удалось найти окно значков рабочего стола.");
            }
        }


        private static IntPtr GetDesktopIconsWindow(IntPtr desktopPtr)
        {
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




        [DllImport("kernel32.dll")]
        private static extern uint GetTickCount();
        private static uint GetIdleTimeInSeconds()
        {
            uint sessionId = WTSGetActiveConsoleSessionId();
            Console.WriteLine("Active Session ID: " + sessionId);

            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint idleTimeMilliseconds = (uint)Environment.TickCount - lastInputInfo.dwTime;

                // Учитываем возможное переполнение
                if (idleTimeMilliseconds > int.MaxValue)
                {
                    idleTimeMilliseconds = (uint)(Environment.TickCount - int.MinValue);
                }

                return idleTimeMilliseconds / 1000; // Время бездействия в секундах
            }
            return 0;



            //LASTINPUTINFO lastInputInfo = new LASTINPUTINFO
            //{
            //    cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO))
            //};

            //if (GetLastInputInfo(ref lastInputInfo))
            //{
            //    uint tickCount = GetTickCount();
            //    uint idleTime = tickCount - lastInputInfo.dwTime;
            //    return (int)(idleTime / 1000); // Переводим в секунды
            //}
            //else
            //{
            //    throw new Exception("Не удалось получить информацию о последнем действии.");
            //}
        }

        private static int GetActiveUserSession()
        {
            IntPtr pSessionInfo = IntPtr.Zero;
            int sessionCount = 0;

            try
            {
                if (WTSEnumerateSessions(IntPtr.Zero, 0, 1, ref pSessionInfo, ref sessionCount))
                {
                    int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                    IntPtr currentSession = pSessionInfo;

                    for (int i = 0; i < sessionCount; i++)
                    {
                        WTS_SESSION_INFO sessionInfo = Marshal.PtrToStructure<WTS_SESSION_INFO>(currentSession);

                        if (sessionInfo.State == WTS_ACTIVE)
                        {
                            return sessionInfo.SessionId;
                        }

                        // Сдвигаем указатель на следующую структуру
                        currentSession = IntPtr.Add(currentSession, dataSize);
                    }
                }
                else
                {
                    Console.WriteLine("WTSEnumerateSessions вернула ошибку.");
                }
            }
            finally
            {
                if (pSessionInfo != IntPtr.Zero)
                {
                    WTSFreeMemory(pSessionInfo); // Используйте правильную функцию для освобождения памяти
                }
            }

            return -1; // Активная сессия не найдена
        }





        public static void Log(string text)
        {
            File.AppendAllText(@"C:\\Users\\Ffix\\Desktop\\log.txt", text + "\n");
        }
    }
}
