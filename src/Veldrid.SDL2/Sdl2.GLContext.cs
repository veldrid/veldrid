using System;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr SDL_GL_CreateContext_t(SDL_Window SDL2Window);
        private static SDL_GL_CreateContext_t s_gl_createContext = LoadFunction<SDL_GL_CreateContext_t>("SDL_GL_CreateContext");
        public static IntPtr SDL_GL_CreateContext(SDL_Window Sdl2Window) => s_gl_createContext(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr SDL_GL_GetProcAddress_t(string proc);
        private static SDL_GL_GetProcAddress_t s_getProcAddress = LoadFunction<SDL_GL_GetProcAddress_t>("SDL_GL_GetProcAddress");
        public static IntPtr SDL_GL_GetProcAddress(string proc)
        {
            return s_getProcAddress(proc);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr SDL_GL_GetCurrentContext_t();
        private static SDL_GL_GetCurrentContext_t s_gl_getCurrentContext = LoadFunction<SDL_GL_GetCurrentContext_t>("SDL_GL_GetCurrentContext");
        public static IntPtr SDL_GL_GetCurrentContext()
        {
            var ret = s_gl_getCurrentContext();
            return ret;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_GL_SwapWindow_t(SDL_Window SDL2Window);
        private static SDL_GL_SwapWindow_t s_gl_swapWindow = LoadFunction<SDL_GL_SwapWindow_t>("SDL_GL_SwapWindow");
        public static void SDL_GL_SwapWindow(SDL_Window Sdl2Window) => s_gl_swapWindow(Sdl2Window);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GL_SetAttribute_t(SDL_GLAttribute attr, int value);
        private static SDL_GL_SetAttribute_t s_gl_setAttribute = LoadFunction<SDL_GL_SetAttribute_t>("SDL_GL_SetAttribute");
        public static int SDL_GL_SetAttribute(SDL_GLAttribute attr, int value) => s_gl_setAttribute(attr, value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GL_GetAttribute_t(SDL_GLAttribute attr, int* value);
        private static SDL_GL_GetAttribute_t s_gl_getAttribute = LoadFunction<SDL_GL_GetAttribute_t>("SDL_GL_GetAttribute");
        public static int SDL_GL_GetAttribute(SDL_GLAttribute attr, int* value) => s_gl_getAttribute(attr, value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GL_MakeCurrent_t(SDL_Window SDL2Window, IntPtr context);
        private static SDL_GL_MakeCurrent_t s_gl_makeCurrent = LoadFunction<SDL_GL_MakeCurrent_t>("SDL_GL_MakeCurrent");
        public static int SDL_GL_MakeCurrent(SDL_Window Sdl2Window, IntPtr context) => s_gl_makeCurrent(Sdl2Window, context);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GL_SetSwapInterval_t(int interval);
        private static SDL_GL_SetSwapInterval_t s_gl_setSwapInterval = LoadFunction<SDL_GL_SetSwapInterval_t>("SDL_GL_SetSwapInterval");
        public static int SDL_GL_SetSwapInterval(int interval) => s_gl_setSwapInterval(interval);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_GL_DeleteContext_t(IntPtr context);
        private static SDL_GL_DeleteContext_t s_gl_deleteContext = LoadFunction<SDL_GL_DeleteContext_t>("SDL_GL_DeleteContext");
        public static void SDL_GL_DeleteContext(IntPtr context) => s_gl_deleteContext(context);
    }

    public enum SDL_GLAttribute
    {
        RedSize,
        GreenSize,
        BlueSize,
        AlphaSize,
        BufferSize,
        DoubleBuffer,
        DepthSize,
        StencilSize,
        AccumulationRedSize,
        AccumulationGreenSize,
        AccumulationBlueSize,
        AccumulationAlphaSize,
        GLStereo,
        MultisampleBuffers,
        MultisampleSamples,
        AcceleratedVisual,
        RetainedBacking,
        ContextMajorVersion,
        ContextMinorVersion,
        ContextEgl,
        ContextFlags,
        ContextProfileMask,
        ShareWithCurrentContext,
        FramebufferSrgbCapable,
        ContextReleaseBehavior
    }

    public enum SDL_GLContextFlag
    {
        Debug = 0x0001,
        ForwardCompatible = 0x0002,
        RobustAccess = 0x0004,
        ResetIsolatio = 0x0008,
    }

    public enum SDL_GLProfile
    {
        Core = 0x0001,
        Compatibility = 0x0002,
        ES = 0x0004
    }
}
