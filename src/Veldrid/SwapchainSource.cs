using System;
using System.Runtime.InteropServices;
#if NET7_0_OR_GREATER
using System.Runtime.InteropServices.JavaScript;
#endif

namespace Veldrid
{
    /// <summary>
    /// A platform-specific object representing a renderable surface.
    /// A SwapchainSource can be created with one of several static factory methods.
    /// A SwapchainSource is used to describe a Swapchain (see <see cref="SwapchainDescription"/>).
    /// </summary>
    public abstract class SwapchainSource
    {
        internal SwapchainSource() { }

        internal abstract void GetSize(out uint width, out uint height);

        /// <summary>
        /// Creates a new SwapchainSource for a Win32 window.
        /// </summary>
        /// <param name="hwnd">The Win32 window handle.</param>
        /// <param name="hinstance">The Win32 instance handle.</param>
        /// <returns>A new SwapchainSource which can be used to create a <see cref="Swapchain"/> for the given Win32 window.
        /// </returns>
        public static SwapchainSource CreateWin32(IntPtr hwnd, IntPtr hinstance) => new Win32SwapchainSource(hwnd, hinstance);

        /// <summary>
        /// Creates a new SwapchainSource for a UWP SwapChain panel.
        /// </summary>
        /// <param name="swapChainPanel">A COM object which must implement the <see cref="Vortice.DXGI.ISwapChainPanelNative"/>
        /// or <see cref="Vortice.DXGI.ISwapChainBackgroundPanelNative"/> interface. Generally, this should be a SwapChainPanel
        /// or SwapChainBackgroundPanel contained in your application window.</param>
        /// <param name="logicalDpi">The logical DPI of the swapchain panel.</param>
        /// <returns>A new SwapchainSource which can be used to create a <see cref="Swapchain"/> for the given UWP panel.
        /// </returns>
        public static SwapchainSource CreateUwp(object swapChainPanel, float logicalDpi)
            => new UwpSwapchainSource(swapChainPanel, logicalDpi);

        /// <summary>
        /// Creates a new SwapchainSource from the given Xlib information.
        /// </summary>
        /// <param name="display">An Xlib Display.</param>
        /// <param name="window">An Xlib Window.</param>
        /// <returns>A new SwapchainSource which can be used to create a <see cref="Swapchain"/> for the given Xlib window.
        /// </returns>
        public static SwapchainSource CreateXlib(IntPtr display, IntPtr window) => new XlibSwapchainSource(display, window);

        /// <summary>
        /// Creates a new SwapchainSource from the given Wayland information.
        /// </summary>
        /// <param name="display">The Wayland display proxy.</param>
        /// <param name="surface">The Wayland surface proxy to map.</param>
        /// <returns>A new SwapchainSource which can be used to create a <see cref="Swapchain"/> for the given Wayland surface.
        /// </returns>
        public static SwapchainSource CreateWayland(IntPtr display, IntPtr surface) => new WaylandSwapchainSource(display, surface);


        /// <summary>
        /// Creates a new SwapchainSource for the given NSWindow.
        /// </summary>
        /// <param name="nsWindow">A pointer to an NSWindow.</param>
        /// <returns>A new SwapchainSource which can be used to create a Metal <see cref="Swapchain"/> for the given NSWindow.
        /// </returns>
        public static SwapchainSource CreateNSWindow(IntPtr nsWindow) => new NSWindowSwapchainSource(nsWindow);

        /// <summary>
        /// Creates a new SwapchainSource for the given UIView.
        /// </summary>
        /// <param name="uiView">The UIView's native handle.</param>
        /// <returns>A new SwapchainSource which can be used to create a Metal <see cref="Swapchain"/> or an OpenGLES
        /// <see cref="GraphicsDevice"/> for the given UIView.
        /// </returns>
        public static SwapchainSource CreateUIView(IntPtr uiView) => new UIViewSwapchainSource(uiView);

        /// <summary>
        /// Creates a new SwapchainSource for the given Android Surface.
        /// </summary>
        /// <param name="surfaceHandle">The handle of the Android Surface.</param>
        /// <param name="jniEnv">The Java Native Interface Environment handle.</param>
        /// <returns>A new SwapchainSource which can be used to create a Vulkan <see cref="Swapchain"/> or an OpenGLES
        /// <see cref="GraphicsDevice"/> for the given Android Surface.</returns>
        public static SwapchainSource CreateAndroidSurface(IntPtr surfaceHandle, IntPtr jniEnv)
            => new AndroidSurfaceSwapchainSource(surfaceHandle, jniEnv);

        /// <summary>
        /// Creates a new SwapchainSource for the given NSView.
        /// </summary>
        /// <param name="nsView">A pointer to an NSView.</param>
        /// <returns>A new SwapchainSource which can be used to create a Metal <see cref="Swapchain"/> for the given NSView.
        /// </returns>
        public static SwapchainSource CreateNSView(IntPtr nsView)
            => new NSViewSwapchainSource(nsView);

#if NET7_0_OR_GREATER
        public static SwapchainSource CreateHtml5Canvas(JSObject canvas) => new Html5CanvasSwapchainSource(canvas);
#endif
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

        internal unsafe override void GetSize(out uint width, out uint height)
        {
            RECT r;
            uint succeeded = GetClientRect(Hwnd, &r);
            if (succeeded == 0)
            {
                throw new VeldridException($"Failed to retrieve Win32 window size.");
            }
            width = (uint)(r.right - r.left);
            height = (uint)(r.bottom - r.top);
        }

        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static unsafe extern uint GetClientRect(
            IntPtr hWnd,
            RECT* lpRect);
    }

    internal class UwpSwapchainSource : SwapchainSource
    {
        public object SwapChainPanelNative { get; }
        public float LogicalDpi { get; }

        public UwpSwapchainSource(object swapChainPanelNative, float logicalDpi)
        {
            SwapChainPanelNative = swapChainPanelNative;
            LogicalDpi = logicalDpi;
        }

        internal override void GetSize(out uint width, out uint height)
        {
            throw new NotImplementedException();
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

        internal override void GetSize(out uint width, out uint height)
        {
            throw new NotImplementedException();
        }
    }

    internal class WaylandSwapchainSource : SwapchainSource
    {
        public IntPtr Display { get; }
        public IntPtr Surface { get; }

        public WaylandSwapchainSource(IntPtr display, IntPtr surface)
        {
            Display = display;
            Surface = surface;
        }

        internal override void GetSize(out uint width, out uint height)
        {
            throw new NotImplementedException();
        }
    }

    internal class NSWindowSwapchainSource : SwapchainSource
    {
        public IntPtr NSWindow { get; }

        public NSWindowSwapchainSource(IntPtr nsWindow)
        {
            NSWindow = nsWindow;
        }

        internal override void GetSize(out uint width, out uint height)
        {
            throw new NotImplementedException();
        }
    }

    internal class UIViewSwapchainSource : SwapchainSource
    {
        public IntPtr UIView { get; }

        public UIViewSwapchainSource(IntPtr uiView)
        {
            UIView = uiView;
        }

        internal override void GetSize(out uint width, out uint height)
        {
            throw new NotImplementedException();
        }
    }

    internal class AndroidSurfaceSwapchainSource : SwapchainSource
    {
        public IntPtr Surface { get; }
        public IntPtr JniEnv { get; }

        public AndroidSurfaceSwapchainSource(IntPtr surfaceHandle, IntPtr jniEnv)
        {
            Surface = surfaceHandle;
            JniEnv = jniEnv;
        }

        internal override void GetSize(out uint width, out uint height)
        {
            throw new NotImplementedException();
        }
    }

    internal class NSViewSwapchainSource : SwapchainSource
    {
        public IntPtr NSView { get; }

        public NSViewSwapchainSource(IntPtr nsView)
        {
            NSView = nsView;
        }

        internal override void GetSize(out uint width, out uint height)
        {
            throw new NotImplementedException();
        }
    }

#if NET7_0_OR_GREATER
    internal class Html5CanvasSwapchainSource : SwapchainSource
    {
        public JSObject Canvas { get; }

        public Html5CanvasSwapchainSource(JSObject canvas)
        {
            Canvas = canvas;
        }

        internal override void GetSize(out uint width, out uint height)
        {
            width = (uint)Canvas.GetPropertyAsInt32("width");
            height = (uint)Canvas.GetPropertyAsInt32("height");
        }
    }
#endif
}
