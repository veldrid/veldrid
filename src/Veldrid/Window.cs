using System;
using Veldrid.Platform;

namespace Veldrid
{
    public interface Window
    {
        int Width { get; set; }
        int Height { get; set; }
        IntPtr Handle { get; }
        string Title { get; set; }
        WindowState WindowState { get; set; }
        bool Exists { get; }
        bool Visible { get; set; }

        event Action Resized;
        event Action Closing;
        event Action Closed;

        InputSnapshot GetInputSnapshot();
        void Close();
    }
}
