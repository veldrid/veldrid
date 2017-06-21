using NativeLibraryLoader;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        private static readonly NativeLibrary s_sdl2Lib = LoadSdl2();
        private static NativeLibrary LoadSdl2()
        {
            string name;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                name = "SDL2.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                name = "libSDL2-2.0.so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                name = "libsdl2.dylib";
            }
            else
            {
                Debug.WriteLine("Unknown SDL platform. Attempting to load \"SDL2\"");
                name = "SDL2";
            }

            NativeLibrary lib = new NativeLibrary(name);
            return lib;
        }

        private static T LoadFunction<T>(string name)
        {
            return s_sdl2Lib.LoadFunction<T>(name);
        }

        private delegate byte* SDL_GetError_t();
        private static SDL_GetError_t s_sdl_getError = LoadFunction<SDL_GetError_t>("SDL_GetError");
        public static byte* SDL_GetError() => s_sdl_getError();
    }
}
