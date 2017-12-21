using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_GetVersion_t(SDL_version* version);
        private static SDL_GetVersion_t s_getVersion = LoadFunction<SDL_GetVersion_t>("SDL_GetVersion");
        public static void SDL_GetVersion(SDL_version* version) => s_getVersion(version);
    }

    public struct SDL_version
    {
        public byte major;
        public byte minor;
        public byte patch;
    }
}
