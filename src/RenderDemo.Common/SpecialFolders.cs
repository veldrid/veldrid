using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Veldrid.RenderDemo
{
    public class SpecialFolders
    {
        public static string GetAppDataFolder()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Environment.GetEnvironmentVariable("APPDATA");
            }
            else
            {
                return Environment.GetEnvironmentVariable("HOME");
            }
        }

        public static string VeldridConfigFolder
        {
            get
            {
                string appDatafolder = GetAppDataFolder();
                if (string.IsNullOrEmpty(appDatafolder) || !Directory.Exists(appDatafolder))
                {
                    appDatafolder = AppContext.BaseDirectory;
                }

                return Path.Combine(appDatafolder, "veldrid");
            }
        }
    }
}