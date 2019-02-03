using NativeLibraryLoader;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        private static readonly NativeLibrary s_sdl2Lib = LoadSdl2();
        private static NativeLibrary LoadSdl2()
        {
            string[] names;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                names = new[] { "SDL2.dll" };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                names = new[]
                {
                    "libSDL2-2.0.so",
                    "libSDL2-2.0.so.0",
                    "libSDL2-2.0.so.1",
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                names = new[]
                {
                    "libsdl2.dylib"
                };
            }
            else
            {
                Debug.WriteLine("Unknown SDL platform. Attempting to load \"SDL2\"");
                names = new[] { "SDL2.dll" };
            }

            NativeLibrary lib = new NativeLibrary(names);
            return lib;
        }

        /// <summary>
        /// Loads an SDL2 function by the given name.
        /// </summary>
        /// <typeparam name="T">The delegate type of the function to load.</typeparam>
        /// <param name="name">The name of the exported native function.</param>
        /// <returns>A delegate which can be used to invoke the native function.</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when no function with the given name is exported by SDL2.
        /// </exception>
        public static T LoadFunction<T>(string name)
        {
            try
            {
                return s_sdl2Lib.LoadFunction<T>(name);
            }
            catch
            {
                Debug.WriteLine(
                    $"Unable to load SDL2 function \"{name}\". " +
                    $"Attempting to call this function will cause an exception to be thrown.");
                return default(T);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* SDL_GetError_t();
        private static SDL_GetError_t s_sdl_getError = LoadFunction<SDL_GetError_t>("SDL_GetError");
        public static byte* SDL_GetError() => s_sdl_getError();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_ClearError_t();
        private static SDL_ClearError_t s_sdl_clearError = LoadFunction<SDL_ClearError_t>("SDL_ClearError");
        public static byte* SDL_ClearError() { s_sdl_clearError(); return null; }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_free_t(void* ptr);
        private static SDL_free_t s_sdl_free = LoadFunction<SDL_free_t>("SDL_free");
        public static void SDL_free(void* ptr) { s_sdl_free(ptr); }
    }
}
