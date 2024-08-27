using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using RealMasters;
using Microsoft.Win32.SafeHandles;
using Swed64;

class Program
{
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleOutputCP(uint wCodePageID);

    private const int STD_OUTPUT_HANDLE = -11;

    static void Main(string[] args)
    {
        // Create a new console window
        AllocConsole();
        SetConsoleOutputCP(65001); // Set code page to UTF-8 for correct character display

        // Redirect console output to the new console window
        IntPtr consoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);
        SafeFileHandle safeFileHandle = new SafeFileHandle(consoleHandle, ownsHandle: false);
        StreamWriter consoleWriter = new StreamWriter(new FileStream(safeFileHandle, FileAccess.Write)) { AutoFlush = true };
        Console.SetOut(consoleWriter);
        Console.SetError(consoleWriter);

        Console.WriteLine("Starting application...");

        try
        {
            Swed swed = new Swed("cs2");
            IntPtr client = swed.GetModuleBase("client.dll");

            Renderer renderer = new Renderer();
            Thread renderThread = new Thread(() =>
            {
                try
                {
                    renderer.Start().Wait();
                    Console.WriteLine("Renderer started.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Renderer start failed: {ex.Message}");
                }
            });
            renderThread.Start();

            Vector2 screenSize = renderer.screenSize;
            Console.WriteLine($"Screen size set to: {screenSize}");

            Aimbot aimbot = new Aimbot(swed, client, renderer, screenSize);
            Console.WriteLine("Aimbot created.");
            Thread aimbotThread = new Thread(() =>
            {
                try
                {
                    aimbot.RunAimbot();
                    Console.WriteLine("Aimbot running.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Aimbot failed: {ex.Message}");
                }
            });
            aimbotThread.Start();

            EspBox espBox = new EspBox(swed, client, renderer, screenSize);
            Console.WriteLine("ESP Box created.");
            Thread espThread = new Thread(() =>
            {
                try
                {
                    espBox.RunESP();
                    Console.WriteLine("ESP running.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ESP failed: {ex.Message}");
                }
            });
            espThread.Start();

            RadarHack radarHack = new RadarHack(swed, client, renderer);
            Console.WriteLine("Radar Hack created.");
            Thread radarThread = new Thread(() =>
            {
                try
                {
                    radarHack.RunRadar();
                    Console.WriteLine("Radar running.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Radar failed: {ex.Message}");
                }
            });
            radarThread.Start();

            Fov fov = new Fov(swed, client, renderer, screenSize);
            Console.WriteLine("FOV created.");
            Thread fovThread = new Thread(() =>
            {
                try
                {
                    fov.RunFov();
                    Console.WriteLine("FOV running.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FOV failed: {ex.Message}");
                }
            });
            fovThread.Start();

            AntiFlash antiFlash = new AntiFlash(swed, client, renderer);
            Console.WriteLine("AntiFlash created.");
            Thread antiFlashThread = new Thread(() =>
            {
                try
                {
                    antiFlash.RunAntiFlash();
                    Console.WriteLine("AntiFlash running.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AntiFlash failed: {ex.Message}");
                }
            });
            antiFlashThread.Start();

            // Keep the application running to monitor threads
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
        }
    }
}
