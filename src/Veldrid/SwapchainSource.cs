using System;

namespace Veldrid
{
    public abstract class SwapchainSource
    {
        internal SwapchainSource() { }

        public static SwapchainSource CreateWin32(IntPtr hwnd, IntPtr hinstance) => new Win32SwapchainSource(hwnd, hinstance);
        public static SwapchainSource CreateXlib(IntPtr display, IntPtr window) => new XlibSwapchainSource(display, window);
        public static SwapchainSource CreateNSWindow(IntPtr nsWindow) => new NSWindowSwapchainSource(nsWindow);
    }

    internal class Win32SwapchainSource : SwapchainSource
    {
        public IntPtr Hwnd { get; }
        public IntPtr Hinstance { get; }

        public Win32SwapchainSource(IntPtr hwnd, IntPtr hinstance)
        {
            Hwnd = hwnd;
            Hinstance = hinstance;
        }
    }

    internal class XlibSwapchainSource : SwapchainSource
    {
        public IntPtr Display { get; }
        public IntPtr Window { get; }

        public XlibSwapchainSource(IntPtr display, IntPtr window)
        {
            Display = display;
            Window = window;
        }
    }

    internal class NSWindowSwapchainSource : SwapchainSource
    {
        public IntPtr NSWindow { get; }

        public NSWindowSwapchainSource(IntPtr nsWindow)
        {
            NSWindow = nsWindow;
        }
    }
}
