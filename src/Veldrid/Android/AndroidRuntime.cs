using System;
using System.Runtime.InteropServices;

namespace Veldrid.Android
{
    /// <summary>
    /// Function imports from the Android runtime library (android.so).
    /// </summary>
    internal static class AndroidRuntime
    {
        private const string LibName = "android.so";

        [DllImport(LibName)]
        public static extern IntPtr ANativeWindow_fromSurface(IntPtr jniEnv, IntPtr surface);
        [DllImport(LibName)]
        public static extern int ANativeWindow_setBuffersGeometry(IntPtr aNativeWindow, int width, int height, int format);
        [DllImport(LibName)]
        public static extern void ANativeWindow_release(IntPtr aNativeWindow);
    }
}
