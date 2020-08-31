using System.Runtime.InteropServices;

namespace Veldrid.StartupUtilities
{
    internal static class ShcoreNative
    {
        [DllImport("Shcore.dll")]
        internal static extern int SetProcessDpiAwareness(PROCESS_DPI_AWARENESS value);
    }

    internal enum PROCESS_DPI_AWARENESS
    {
        PROCESS_DPI_UNAWARE,
        PROCESS_SYSTEM_DPI_AWARE,
        PROCESS_PER_MONITOR_DPI_AWARE
    };
}
