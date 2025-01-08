using System;
using System.ServiceProcess;

namespace CleanDesktop.WindowsService
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                //new CleanService()
                new TestService()
            };

            if (Environment.UserInteractive)
            {
                // Запуск в интерактивном режиме для отладки
                TestService service = new TestService();
                service.Test(args);

            }
            else
            {
                // Запуск как служба
                ServiceBase.Run(ServicesToRun);
            }
            
        }
    }
}
