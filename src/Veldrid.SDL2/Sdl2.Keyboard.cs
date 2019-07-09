using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* SDL_GetKeyboardState_t(int* numkeys);
        private static SDL_GetKeyboardState_t s_sdl_getKeyboardState = LoadFunction<SDL_GetKeyboardState_t>("SDL_GetKeyboardState");
        public static byte* SDL_GetKeyboardState(int* numkeys) => s_sdl_getKeyboardState(numkeys);
    }
}
