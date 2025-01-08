using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;

namespace CleanDesktop.WindowsService
{

    public partial class TestService : ServiceBase
    {
        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        public TestService()
        {
            InitializeComponent();
            this.CanStop = true; // службу можно остановить
        }
        public static void Log(string text)
        {
            File.AppendAllText(@"C:\\Users\\Ffix\\Desktop\\log.txt", text + "\n");
        }

        protected override void OnStart(string[] args)
        {
            Log("Service start!");
            while (true)
            {
                GetCursorPos(out POINT point);
                Log($"Mouse Position: X = {point.X}, Y = {point.Y}");
                Thread.Sleep(1000); // Проверяем каждые 100 мс
            }

        }

        protected override void OnStop()
        {
            Log("Service stop!");
        }

        internal void Test(string[] args)
        {
            OnStart(args);
        }
    }
}
