using System;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SDL_Keymod SDL_GetModState_t();
        private static SDL_GetModState_t s_sdl_getModState = Sdl2Native.LoadFunction<SDL_GetModState_t>("SDL_GetModState");
        /// <summary>
        /// Returns an OR'd combination of the modifier keys for the keyboard. See SDL_Keymod for details.
        /// </summary>
        public static SDL_Keymod SDL_GetModState() => s_sdl_getModState();
    }
}
