using Veldrid.Sdl2;

namespace Veldrid.SampleGallery
{
    public static unsafe class Keyboard
    {
        private static byte* s_keys;
        private static int s_numKeys;

        public static void Refresh()
        {
            Sdl2Events.ProcessEvents();
            int numKeys = -1;
            s_keys = Sdl2Native.SDL_GetKeyboardState(&numKeys);
            s_numKeys = numKeys;
        }

        public static bool IsKeyDown(SDL_Scancode scancode)
        {
            return s_keys[(int)scancode] == 1;
        }
    }
}
