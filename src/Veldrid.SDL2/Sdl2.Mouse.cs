using System;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        public const int SDL_QUERY = -1;
        public const int SDL_DISABLE = 0;
        public const int SDL_ENABLE = 1;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_ShowCursor_t(int toggle);
        private static SDL_ShowCursor_t s_sdl_showCursor = LoadFunction<SDL_ShowCursor_t>("SDL_ShowCursor");
        public static int SDL_ShowCursor(int toggle) => s_sdl_showCursor(toggle);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_WarpMouseInWindow_t(SDL_Window window, int x, int y);
        private static SDL_WarpMouseInWindow_t s_sdl_warpMouseInWindow = LoadFunction<SDL_WarpMouseInWindow_t>("SDL_WarpMouseInWindow");
        public static void SDL_WarpMouseInWindow(SDL_Window window, int x, int y) => s_sdl_warpMouseInWindow(window, x, y);

    }
}
