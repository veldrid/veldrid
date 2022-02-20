using System;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Veldrid.Sdl2
{
    /// <summary>
    /// Enumeration of valid key mods (possibly OR'd together).
    /// </summary>
    [Flags]
    public enum SDL_Keymod
    {
        None = 0x0000,
        LeftShift = 0x0001,
        RightShift = 0x0002,
        LeftControl = 0x0040,
        RightControl = 0x0080,
        LeftAlt = 0x0100,
        RightAlt = 0x0200,
        LeftGui = 0x0400,
        RightGui = 0x0800,
        Num = 0x1000,
        Caps = 0x2000,
        Mode = 0x4000,
        Reserved = 0x8000
    }
}
