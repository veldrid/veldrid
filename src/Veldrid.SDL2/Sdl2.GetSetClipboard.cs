using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* SDL_GetClipboardText_t();
        private static SDL_GetClipboardText_t s_sdl_getClipboardText = LoadFunction<SDL_GetClipboardText_t>("SDL_GetClipboardText");

        /// <summary>
        /// </summary>
        /// <returns>
        /// Pointer to UTF8 data. Has to be freed with <see cref="SDL_free(void*)"/>.
        /// </returns>
        public static byte* SDL_GetClipboardTextUtf8() => s_sdl_getClipboardText();

        public static string? SDL_GetClipboardText()
        {
            byte* utf8 = s_sdl_getClipboardText();
            if (utf8 == null)
            {
                return null;
            }
            try
            {
                return Marshal.PtrToStringUTF8((IntPtr)utf8);
            }
            finally
            {
                SDL_free(utf8);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_SetClipboardText_t(byte* text);
        private static SDL_SetClipboardText_t s_sdl_setClipboardText = LoadFunction<SDL_SetClipboardText_t>("SDL_SetClipboardText");
        public static int SDL_SetClipboardTextUtf8(byte* nullTerminatedUtf8Text) => s_sdl_setClipboardText(nullTerminatedUtf8Text);

        [SkipLocalsInit]
        public static int SDL_SetClipboardText(ReadOnlySpan<char> text)
        {
            Span<byte> buffer = stackalloc byte[4096];

            IntPtr ptr = Utilities.GetNullTerminatedUtf8(text, ref buffer);
            try
            {
                fixed (byte* utf8Ptr = buffer)
                {
                    return SDL_SetClipboardTextUtf8(utf8Ptr);
                }
            }
            finally
            {
                Utilities.FreeUtf8(ptr);
            }
        }
    }
}
