using CleanDesktop.ConsoleApp;
using System;
using System.Threading;
using System.Windows.Forms;

class Program
{
    private static bool IsUserActive = true;

    static void Main(string[] args)
    {
        while (true)
        {
            //InterfaceHandler.ShowCurrsorPos();
            CheckIdleTime();
            Thread.Sleep(100);
        }
    }

    private static void CheckIdleTime()
    {
        Console.WriteLine("Check Idle time...");
        int idleTime = UserActivity.GetIdleTimeInSeconds();
        Console.WriteLine($"Idle time: {idleTime}");
        if (idleTime > 5) // Если бездействие больше n cекунд
        {
            if (IsUserActive)
            {
                InterfaceHandler.SetVisible(false);
                IsUserActive = false;
            }            
        }
        else
        {
            if (!IsUserActive)
            {
                InterfaceHandler.SetVisible(true);
                IsUserActive = true;
            }            
        }
    }

    

}