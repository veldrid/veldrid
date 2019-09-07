using System;

namespace Veldrid.Sdl2
{
    public interface IWindow
    {
        int Width { get; }
        int Height { get; }

        IntPtr SdlWindowHandle { get; }
    }
}
