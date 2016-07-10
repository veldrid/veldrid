using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Veldrid.RenderDemo
{
    public static class BrowserUtil
    {
        public static void OpenBrowser(string ghUrl)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {ghUrl}"));
            }
            else
            {
                Process.Start("xdg-open", ghUrl);
            }
        }
    }
}
