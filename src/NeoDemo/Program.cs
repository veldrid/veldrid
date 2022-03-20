using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Veldrid.NeoDemo
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            SetVersion();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            try
            {
                await new NeoDemo().Run();
            }
            catch (Exception ex)
            {
            }
            Console.WriteLine("End...");
        }

        unsafe static void SetVersion()
        {
            Sdl2.SDL_version version;
            Sdl2.Sdl2Native.SDL_GetVersion(&version);
        }

        private static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            var Ex = e.Exception;
            Console.WriteLine($"{Ex.Message} {Ex.StackTrace}");

        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var Ex = (Exception)e.ExceptionObject;
            Console.WriteLine($"{Ex.Message} {Ex.StackTrace}");
        }
    }
}
