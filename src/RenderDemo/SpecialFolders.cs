using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Veldrid.RenderDemo
{
    public class SpecialFolders
    {
        public static string AppDataFolder
        {
            get
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
        }

        public static string VeldridConfigFolder
        {
            get
            {
                return Path.Combine(AppDataFolder, "veldrid");
            }
        }
    }
}