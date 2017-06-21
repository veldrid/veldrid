using System;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        private delegate SDL_Window SDL_CreateWindow_t(string title, int x, int y, int w, int h, SDL_WindowFlags flags);
        private static SDL_CreateWindow_t s_sdl_createWindow = LoadFunction<SDL_CreateWindow_t>("SDL_CreateWindow");
        public static SDL_Window SDL_CreateWindow(string title, int x, int y, int w, int h, SDL_WindowFlags flags) => s_sdl_createWindow(title, x, y, w, h, flags);

        private delegate void SDL_DestroyWindow_t(SDL_Window window);
        private static SDL_DestroyWindow_t s_sdl_destroyWindow = LoadFunction<SDL_DestroyWindow_t>("SDL_DestroyWindow");
        public static void SDL_DestroyWindow(SDL_Window window) => s_sdl_destroyWindow(window);

        private delegate void SDL_GetWindowSize_t(SDL_Window window, int* w, int* h);
        private static SDL_GetWindowSize_t s_getWindowSize = LoadFunction<SDL_GetWindowSize_t>("SDL_GetWindowSize");
        public static void SDL_GetWindowSize(SDL_Window window, int* w, int* h) => s_getWindowSize(window, w, h);

        private delegate void SDL_GetWindowPosition_t(SDL_Window window, int* x, int* y);
        private static SDL_GetWindowPosition_t s_getWindowPosition = LoadFunction<SDL_GetWindowPosition_t>("SDL_GetWindowPosition");
        public static void SDL_GetWindowPosition(SDL_Window window, int* x, int* y) => s_getWindowPosition(window, x, y);

        private delegate void SDL_SetWindowPosition_t(SDL_Window window, int x, int y);
        private static SDL_SetWindowPosition_t s_setWindowPosition = LoadFunction<SDL_SetWindowPosition_t>("SDL_SetWindowPosition");
        public static void SDL_SetWindowPosition(SDL_Window window, int x, int y) => s_setWindowPosition(window, x, y);

        private delegate void SDL_SetWindowSize_t(SDL_Window window, int w, int h);
        private static SDL_SetWindowSize_t s_setWindowSize = LoadFunction<SDL_SetWindowSize_t>("SDL_SetWindowSize");
        public static void SDL_SetWindowSize(SDL_Window window, int w, int h) => s_setWindowSize(window, w, h);

        private delegate string SDL_GetWindowTitle_t(SDL_Window window);
        private static SDL_GetWindowTitle_t s_getWindowTitle = LoadFunction<SDL_GetWindowTitle_t>("SDL_GetWindowTitle");
        public static string SDL_GetWindowTitle(SDL_Window window) => s_getWindowTitle(window);

        private delegate void SDL_SetWindowTitle_t(SDL_Window window, string title);
        private static SDL_SetWindowTitle_t s_setWindowTitle = LoadFunction<SDL_SetWindowTitle_t>("SDL_SetWindowTitle");
        public static void SDL_SetWindowTitle(SDL_Window window, string title) => s_setWindowTitle(window, title);

        private delegate SDL_WindowFlags SDL_GetWindowFlags_t(SDL_Window window);
        private static SDL_GetWindowFlags_t s_getWindowFlags = LoadFunction<SDL_GetWindowFlags_t>("SDL_GetWindowFlags");
        public static SDL_WindowFlags SDL_GetWindowFlags(SDL_Window window) => s_getWindowFlags(window);

        private delegate void SDL_SetWindowBordered_t(SDL_Window window, uint bordered);
        private static SDL_SetWindowBordered_t s_setWindowBordered = LoadFunction<SDL_SetWindowBordered_t>("SDL_SetWindowBordered");
        public static void SDL_SetWindowBordered(SDL_Window window, uint bordered) => s_setWindowBordered(window, bordered);

        private delegate void SDL_MaximizeWindow_t(SDL_Window window);
        private static SDL_MaximizeWindow_t s_maximizeWindow = LoadFunction<SDL_MaximizeWindow_t>("SDL_MaximizeWindow");
        public static void SDL_MaximizeWindow(SDL_Window window) => s_maximizeWindow(window);

        private delegate void SDL_MinimizeWindow_t(SDL_Window window);
        private static SDL_MinimizeWindow_t s_minimizeWindow = LoadFunction<SDL_MinimizeWindow_t>("SDL_MinimizeWindow");
        public static void SDL_MinimizeWindow(SDL_Window window) => s_minimizeWindow(window);

        private delegate int SDL_SetWindowFullscreen_t(SDL_Window window, SDL_FullscreenMode mode);
        private static SDL_SetWindowFullscreen_t s_setWindowFullscreen = LoadFunction<SDL_SetWindowFullscreen_t>("SDL_SetWindowFullscreen");
        public static int SDL_SetWindowFullscreen(SDL_Window window, SDL_FullscreenMode mode) => s_setWindowFullscreen(window, mode);

        private delegate void SDL_ShowWindow_t(SDL_Window window);
        private static SDL_ShowWindow_t s_showWindow = LoadFunction<SDL_ShowWindow_t>("SDL_ShowWindow");
        public static void SDL_ShowWindow(SDL_Window window) => s_showWindow(window);

        private delegate void SDL_HideWindow_t(SDL_Window window);
        private static SDL_HideWindow_t s_hideWindow = LoadFunction<SDL_HideWindow_t>("SDL_HideWindow");
        public static void SDL_HideWindow(SDL_Window window) => s_hideWindow(window);
    }

    [Flags]
    public enum SDL_WindowFlags : uint
    {
        /// <summary>
        /// fullscreen window.
        /// </summary>
        Fullscreen = 0x00000001,
        /// <summary>
        /// window usable with OpenGL context.
        /// </summary>
        OpenGL = 0x00000002,
        /// <summary>
        /// window is visible.
        /// </summary>
        Shown = 0x00000004,
        /// <summary>
        /// window is not visible.
        /// </summary>
        Hidden = 0x00000008,
        /// <summary>
        /// no window decoration.
        /// </summary>
        Borderless = 0x00000010,
        /// <summary>
        /// window can be resized.
        /// </summary>
        Resizable = 0x00000020,
        /// <summary>
        /// window is minimized.
        /// </summary>
        Minimized = 0x00000040,
        /// <summary>
        /// window is maximized.
        /// </summary>
        Maximized = 0x00000080,
        /// <summary>
        /// window has grabbed input focus.
        /// </summary>
        InputGrabbed = 0x00000100,
        /// <summary>
        /// window has input focus.
        /// </summary>
        InputFocus = 0x00000200,
        /// <summary>
        /// window has mouse focus.
        /// </summary>
        MouseFocus = 0x00000400,
        FullScreenDesktop = (Fullscreen | 0x00001000),
        /// <summary>
        /// window not created by SDL.
        /// </summary>
        Foreign = 0x00000800,
        /// <summary>
        /// window should be created in high-DPI mode if supported.
        /// </summary>
        AllowHighDpi = 0x00002000,
        /// <summary>
        /// window has mouse captured (unrelated to InputGrabbed).
        /// </summary>
        MouseCapture = 0x00004000,
        /// <summary>
        /// window should always be above others.
        /// </summary>
        AlwaysOnTop = 0x00008000,
        /// <summary>
        /// window should not be added to the taskbar.
        /// </summary>
        SkipTaskbar = 0x00010000,
        /// <summary>
        /// window should be treated as a utility window.
        /// </summary>
        Utility = 0x00020000,
        /// <summary>
        /// window should be treated as a tooltip.
        /// </summary>
        Tooltip = 0x00040000,
        /// <summary>
        /// window should be treated as a popup menu.
        /// </summary>
        PopupMenu = 0x00080000
    }

    public enum SDL_FullscreenMode : uint
    {
        Windowed = 0,
        Fullscreen = 0x00000001,
        FullScreenDesktop = (Fullscreen | 0x00001000),
    }
}
