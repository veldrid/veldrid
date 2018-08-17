using NativeLibraryLoader;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Veldrid.OpenGL.WGL
{
    internal static unsafe class WindowsNative
    {
        private const string User32LibName = "User32.dll";
        private const string Gdi32LibName = "Gdi32.dll";
        private const string Opengl32LibName = "Opengl32.dll";
        private const string Kernel32LibName = "Kernel32.dll";

        [DllImport(User32LibName)]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport(User32LibName)]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport(User32LibName, SetLastError = true)]
        public static extern int DestroyWindow(IntPtr hwnd);

        [DllImport(User32LibName)]
        public static extern IntPtr CreateWindowEx(
            uint dwExStyle,
            string lpClassName,
            string lpWindowName,
            uint dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);

        [DllImport(User32LibName)]
        public static extern IntPtr RegisterClass(WNDCLASS* lpWndClass);

        [DllImport(Gdi32LibName)]
        public static extern int DescribePixelFormat(
            IntPtr hdc,
            int iPixelFormat,
            uint nBytes,
            PIXELFORMATDESCRIPTOR* ppfd);

        [DllImport(Gdi32LibName)]
        public static extern int ChoosePixelFormat(IntPtr hdc, PIXELFORMATDESCRIPTOR* ppfd);

        [DllImport(Gdi32LibName, SetLastError = true)]
        public static extern int SetPixelFormat(
            IntPtr hdc,
            int iPixelFormat,
            PIXELFORMATDESCRIPTOR* ppfd);

        [DllImport(Gdi32LibName)]
        public static extern int SwapBuffers(IntPtr hdc);

        [DllImport(Opengl32LibName, SetLastError = true)]
        public static extern IntPtr wglCreateContext(IntPtr hdc);

        [DllImport(Opengl32LibName)]
        public static extern int wglDeleteContext(IntPtr wglrc);

        [DllImport(Opengl32LibName)]
        public static extern int wglMakeCurrent(IntPtr hdc, IntPtr hglrc);

        [DllImport(Opengl32LibName)]
        public static extern IntPtr wglGetCurrentContext();

        [DllImport(Opengl32LibName, CharSet = CharSet.Ansi)]
        public static extern IntPtr wglGetProcAddress(string lpszProc);

        [DllImport(Opengl32LibName)]
        public static extern int wglShareLists(IntPtr context1, IntPtr context2);

        [DllImport(Kernel32LibName, CharSet = CharSet.Ansi)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport(Kernel32LibName, CharSet = CharSet.Ansi)]
        public static extern int FreeLibrary(IntPtr hModule);

        [DllImport(Kernel32LibName, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport(Kernel32LibName)]
        public static extern IntPtr GetModuleHandle(IntPtr lpModuleName);

        public const uint PFD_DOUBLEBUFFER = 1;
        public const uint PFD_DRAW_TO_WINDOW = 4;
        public const uint PFD_SUPPORT_OPENGL = 32;

        public const int WGL_DRAW_TO_WINDOW_ARB = 0x2001;
        public const int WGL_ACCELERATION_ARB = 0x2003;
        public const int WGL_SUPPORT_OPENGL_ARB = 0x2010;
        public const int WGL_DOUBLE_BUFFER_ARB = 0x2011;
        public const int WGL_PIXEL_TYPE_ARB = 0x2013;
        public const int WGL_COLOR_BITS_ARB = 0x2014;
        public const int WGL_DEPTH_BITS_ARB = 0x2022;
        public const int WGL_STENCIL_BITS_ARB = 0x2023;
        public const int WGL_FULL_ACCELERATION_ARB = 0x2027;
        public const int WGL_TYPE_RGBA_ARB = 0x202B;

        public const int WGL_CONTEXT_MAJOR_VERSION_ARB = 0x2091;
        public const int WGL_CONTEXT_MINOR_VERSION_ARB = 0x2092;
        public const int WGL_CONTEXT_LAYER_PLANE_ARB = 0x2093;

        public const int WGL_CONTEXT_FLAGS_ARB = 0x2094;
        public const int WGL_CONTEXT_DEBUG_BIT_ARB = 0x0001;

        public const int WGL_CONTEXT_PROFILE_MASK_ARB = 0x9126;
        public const int WGL_CONTEXT_CORE_PROFILE_BIT_ARB = 0x00000001;
        public const int WGL_CONTEXT_ES_PROFILE_BIT_EXT = 0x00000004;

        public const int CW_USEDEFAULT = unchecked(((int)0x80000000));

        public const int CS_VREDRAW = 0x0001;
        public const int CS_HREDRAW = 0x0002;
        public const int CS_OWNDC = 0x0020;

        private static readonly object s_functionsLock = new object();
        private static WindowsExtensionCreationFunctions s_functions;
        public static WindowsExtensionCreationFunctions GetExtensionFunctions()
        {
            lock (s_functionsLock)
            {
                if (s_functions == null)
                {
                    IntPtr hwnd = CreateInvisibleWindow();
                    IntPtr hdc = GetDC(hwnd);
                    if (!CreateContextRegular(hdc, 0, 0, out IntPtr hglrc))
                    {
                        return new WindowsExtensionCreationFunctions();
                    }
                    wglMakeCurrent(hdc, hglrc);
                    IntPtr glLibHandle = LoadLibrary("Opengl32.dll");
                    Func<string, IntPtr> getProcAddress = name =>
                    {
                        IntPtr ret = wglGetProcAddress(name);
                        if (ret == IntPtr.Zero)
                        {
                            ret = GetProcAddress(glLibHandle, name);
                        }
                        return ret;
                    };
                    s_functions = new WindowsExtensionCreationFunctions(getProcAddress);
                    wglDeleteContext(hglrc);
                    ReleaseDC(hwnd, hdc);
                    DestroyWindow(hwnd);
                    FreeLibrary(glLibHandle);
                }

                return s_functions;
            }
        }

        public static bool CreateContextRegular(IntPtr hdc, uint depthBits, uint stencilBits, out IntPtr context)
        {
            PIXELFORMATDESCRIPTOR pfd;
            pfd.nSize = (ushort)Unsafe.SizeOf<PIXELFORMATDESCRIPTOR>();
            pfd.nVersion = 1;
            pfd.dwFlags = PFD_DOUBLEBUFFER | PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL;
            pfd.cColorBits = 32;
            pfd.cDepthBits = (byte)depthBits;
            pfd.cStencilBits = (byte)stencilBits;
            int format = ChoosePixelFormat(hdc, &pfd);
            if (format == 0)
            {
                context = IntPtr.Zero;
                return false;
            }
            int setFormatResult = SetPixelFormat(hdc, format, &pfd);
            if (setFormatResult == 0)
            {
                context = IntPtr.Zero;
                return false;
            }

            context = wglCreateContext(hdc);
            if (context == IntPtr.Zero)
            {
                return false;
            }

            return true;
        }

        public static bool CreateContextWithExtension(
            WindowsExtensionCreationFunctions extensionFuncs,
            GraphicsBackend backend,
            IntPtr hdc,
            bool debug,
            uint depthBits,
            uint stencilBits,
            int major,
            int minor,
            IntPtr shareContext,
            out IntPtr context)
        {
            int attribCount = 8;
            int* pixelFormatAttributes = stackalloc int[(attribCount * 2) + 1];
            int i = 0;

            pixelFormatAttributes[i++] = WGL_DRAW_TO_WINDOW_ARB;
            pixelFormatAttributes[i++] = 1;

            pixelFormatAttributes[i++] = WGL_SUPPORT_OPENGL_ARB;
            pixelFormatAttributes[i++] = 1;

            pixelFormatAttributes[i++] = WGL_DOUBLE_BUFFER_ARB;
            pixelFormatAttributes[i++] = 0;

            pixelFormatAttributes[i++] = WGL_ACCELERATION_ARB;
            pixelFormatAttributes[i++] = WGL_FULL_ACCELERATION_ARB;

            pixelFormatAttributes[i++] = WGL_PIXEL_TYPE_ARB;
            pixelFormatAttributes[i++] = WGL_TYPE_RGBA_ARB;

            pixelFormatAttributes[i++] = WGL_COLOR_BITS_ARB;
            pixelFormatAttributes[i++] = 32;

            pixelFormatAttributes[i++] = WGL_DEPTH_BITS_ARB;
            pixelFormatAttributes[i++] = (int)depthBits;

            pixelFormatAttributes[i++] = WGL_STENCIL_BITS_ARB;
            pixelFormatAttributes[i++] = (int)stencilBits;

            pixelFormatAttributes[i++] = 0; // Terminator

            int format;
            uint formatCount;
            int result = extensionFuncs.ChoosePixelFormatARB(hdc, pixelFormatAttributes, null, 1, &format, &formatCount);
            if (result == 0)
            {
                context = IntPtr.Zero;
                return false;
            }
            PIXELFORMATDESCRIPTOR pfd;
            pfd.nVersion = 1;
            pfd.nSize = (ushort)Unsafe.SizeOf<PIXELFORMATDESCRIPTOR>();
            SetPixelFormat(hdc, format, &pfd);

            int contextAttribCount = debug ? 4 : 3;
            int* contextAttribs = stackalloc int[(contextAttribCount * 2) + 1];
            i = 0;
            contextAttribs[i++] = WGL_CONTEXT_MAJOR_VERSION_ARB;
            contextAttribs[i++] = major;
            contextAttribs[i++] = WGL_CONTEXT_MINOR_VERSION_ARB;
            contextAttribs[i++] = minor;
            contextAttribs[i++] = WGL_CONTEXT_PROFILE_MASK_ARB;
            contextAttribs[i++] = backend == GraphicsBackend.OpenGL
                ? WGL_CONTEXT_CORE_PROFILE_BIT_ARB
                : WGL_CONTEXT_ES_PROFILE_BIT_EXT;
            if (debug)
            {
                contextAttribs[i++] = WGL_CONTEXT_FLAGS_ARB;
                contextAttribs[i++] = WGL_CONTEXT_DEBUG_BIT_ARB;
            }
            contextAttribs[i++] = 0; // Terminator

            context = extensionFuncs.CreateContextAttribsARB(hdc, shareContext, contextAttribs);
            return context != IntPtr.Zero;
        }

        public static WindowsExtensionCreationFunctions GetExtensionFunctions(Func<string, IntPtr> getProcAddress)
        {
            lock (s_functions)
            {
                if (s_functions == null)
                {
                    s_functions = new WindowsExtensionCreationFunctions(getProcAddress);
                }

                return s_functions;
            }
        }

        private static readonly object s_classLock = new object();
        private static IntPtr s_classAtom;
        private const string VeldridClassName = "VeldridWindowClass";

        private static void EnsureClassRegistered()
        {
            lock (s_classLock)
            {
                if (s_classAtom == IntPtr.Zero)
                {
                    NativeLibrary user32Lib = new NativeLibrary("User32.dll");
                    WNDCLASS windowClass;
                    windowClass.style = CS_HREDRAW | CS_VREDRAW | CS_OWNDC;
                    windowClass.lpfnWndProc = user32Lib.LoadFunction("DefWindowProcA");
                    IntPtr hinstance = GetModuleHandle(IntPtr.Zero);
                    windowClass.hInstance = hinstance;

                    byte[] dummyClassBytes = Encoding.ASCII.GetBytes(VeldridClassName);
                    fixed (byte* dummyClassBytesPtr = &dummyClassBytes[0])
                    {
                        windowClass.lpszClassName = (IntPtr)dummyClassBytesPtr;
                        s_classAtom = RegisterClass(&windowClass);
                        if (s_classAtom == IntPtr.Zero)
                        {
                            throw new VeldridException($"Failed to register OpenGL dummy window class.");
                        }
                    }
                }
            }
        }

        public static IntPtr CreateInvisibleWindow()
        {
            EnsureClassRegistered();
            IntPtr hinstance = GetModuleHandle(IntPtr.Zero);
            IntPtr ret = CreateWindowEx(
                0,
                VeldridClassName,
                string.Empty,
                0,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                IntPtr.Zero,
                IntPtr.Zero,
                hinstance,
                IntPtr.Zero);

            if (ret == IntPtr.Zero)
            {
                throw new VeldridException($"Failed to create dummy OpenGL window.");
            }

            return ret;
        }

        private static readonly object s_opengl32LibLock = new object();
        private static IntPtr s_opengl32Lib;
        internal static IntPtr GetOpengl32Lib()
        {
            lock (s_opengl32LibLock)
            {
                if (s_opengl32Lib == IntPtr.Zero)
                {
                    s_opengl32Lib = LoadLibrary("Opengl32.dll");
                }

                return s_opengl32Lib;
            }
        }

        private static readonly object s_versionLock = new object();
        private static (int Major, int Minor)? s_maxSupportedGLVersion;
        private static (int Major, int Minor)? s_maxSupportedGLESVersion;

        internal static (int Major, int Minor) GetMaxGLVersion(bool gles)
        {
            lock (s_versionLock)
            {
                (int Major, int Minor)? maxVer = gles ? s_maxSupportedGLESVersion : s_maxSupportedGLVersion;
                if (maxVer == null)
                {
                    maxVer = TestMaxVersion(gles);
                    if (gles) { s_maxSupportedGLESVersion = maxVer; }
                    else { s_maxSupportedGLVersion = maxVer; }
                }

                return maxVer.Value;
            }
        }

        private static (int Major, int Minor) TestMaxVersion(bool gles)
        {
            GraphicsBackend backend = gles ? GraphicsBackend.OpenGLES : GraphicsBackend.OpenGL;
            WindowsExtensionCreationFunctions extensions = GetExtensionFunctions();
            if (extensions.IsSupported)
            {
                IntPtr hwnd = CreateInvisibleWindow();
                IntPtr hdc = GetDC(hwnd);
                try
                {
                    (int, int)[] testVersions = gles
                        ? new[] { (3, 2), (3, 0) }
                        : new[] { (4, 6), (4, 3), (4, 0), (3, 3), (3, 0) };

                    foreach ((int major, int minor) in testVersions)
                    {
                        if (CreateContextWithExtension(extensions, backend, hdc, false, 0, 0, major, minor, IntPtr.Zero, out IntPtr context))
                        {
                            wglDeleteContext(context);
                            return (major, minor);
                        }
                    }
                }
                finally
                {
                    ReleaseDC(hwnd, hdc);
                    DestroyWindow(hwnd);
                }
            }

            return (0, 0);
        }
    }

    internal class WindowsExtensionCreationFunctions
    {
        public WindowsExtensionCreationFunctions() { IsSupported = false; }

        public WindowsExtensionCreationFunctions(
            Func<string, IntPtr> getProcAddress)
        {
            IntPtr chooseFunc = getProcAddress("wglChoosePixelFormatARB");
            IntPtr createFunc = getProcAddress("wglCreateContextAttribsARB");

            if (chooseFunc != IntPtr.Zero && createFunc != IntPtr.Zero)
            {
                IsSupported = true;
                ChoosePixelFormatARB = Marshal.GetDelegateForFunctionPointer<wglChoosePixelFormatARB>(chooseFunc);
                CreateContextAttribsARB = Marshal.GetDelegateForFunctionPointer<wglCreateContextAttribsARB>(createFunc);
            }
        }

        public bool IsSupported { get; }
        public wglChoosePixelFormatARB ChoosePixelFormatARB { get; }
        public wglCreateContextAttribsARB CreateContextAttribsARB { get; }
    }

    internal delegate int wglSwapIntervalEXT(int interval);

    internal unsafe delegate IntPtr wglCreateContextAttribsARB(IntPtr hDC, IntPtr hshareContext, int* attribList);

    internal unsafe delegate int wglChoosePixelFormatARB(
        IntPtr hdc,
        int* piAttribIList,
        float* pfAttribFList,
        uint nMaxFormats,
        int* piFormats,
        uint* nNumFormats);

    [StructLayout(LayoutKind.Sequential)]
    internal struct PIXELFORMATDESCRIPTOR
    {
        public ushort nSize;
        public ushort nVersion;
        public uint dwFlags;
        public byte iPixelType;
        public byte cColorBits;
        public byte cRedBits;
        public byte cRedShift;
        public byte cGreenBits;
        public byte cGreenShift;
        public byte cBlueBits;
        public byte cBlueShift;
        public byte cAlphaBits;
        public byte cAlphaShift;
        public byte cAccumBits;
        public byte cAccumRedBits;
        public byte cAccumGreenBits;
        public byte cAccumBlueBits;
        public byte cAccumAlphaBits;
        public byte cDepthBits;
        public byte cStencilBits;
        public byte cAuxBuffers;
        public byte iLayerType;
        public byte bReserved;
        public uint dwLayerMask;
        public uint dwVisibleMask;
        public uint dwDamageMask;
    }

    internal struct WNDCLASS
    {
        public uint style;
        public IntPtr lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public IntPtr hInstance;
        public IntPtr hIcon;
        public IntPtr hCursor;
        public IntPtr hbrBackground;
        public IntPtr lpszMenuName;
        public IntPtr lpszClassName;
    }
}
