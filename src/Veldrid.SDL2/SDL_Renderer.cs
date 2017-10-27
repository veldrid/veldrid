using System;

namespace Veldrid.Sdl2
{
    /// <summary>
    /// A transparent wrapper over a pointer representing an SDL Renderer object.
    /// </summary>
    public struct SDL_Renderer
    {
        /// <summary>
        /// The native SDL_Renderer pointer.
        /// </summary>
        public readonly IntPtr NativePointer;

        public SDL_Renderer(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(SDL_Renderer Sdl2Window) => Sdl2Window.NativePointer;
        public static implicit operator SDL_Renderer(IntPtr pointer) => new SDL_Renderer(pointer);
    }
}
