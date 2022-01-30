using System;
using Veldrid.Sdl2;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Veldrid
{
    /// <summary>
    /// Enumeration of valid key mods (possibly OR'd together).
    /// </summary>
    [Flags]
    public enum ModifierKeys
    {
        None = SDL_Keymod.None,
        LeftShift = SDL_Keymod.LeftShift,
        RightShift = SDL_Keymod.RightShift,
        LeftControl = SDL_Keymod.LeftControl,
        RightControl = SDL_Keymod.RightControl,
        LeftAlt = SDL_Keymod.LeftAlt,
        RightAlt = SDL_Keymod.RightAlt,
        LeftGui = SDL_Keymod.LeftGui,
        RightGui = SDL_Keymod.RightGui,
        Num = SDL_Keymod.Num,
        Caps = SDL_Keymod.Caps,
        Mode = SDL_Keymod.Mode,
        Reserved = SDL_Keymod.Reserved,
    }
}
