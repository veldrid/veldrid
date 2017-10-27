using System;

namespace Veldrid.Sdl2
{
    /// <summary>
    /// A transparent wrapper over a pointer representing an SDL Sdl2Window object.
    /// </summary>
    public struct SDL_Window
    {
        /// <summary>
        /// The native SDL_Window pointer.
        /// </summary>
        public readonly IntPtr NativePointer;

        public SDL_Window(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(SDL_Window Sdl2Window) => Sdl2Window.NativePointer;
        public static implicit operator SDL_Window(IntPtr pointer) => new SDL_Window(pointer);
    }
}
