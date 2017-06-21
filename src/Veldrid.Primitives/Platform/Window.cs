using System;
using System.Numerics;

namespace Veldrid.Platform
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
        Vector2 ScaleFactor { get; }
        Rectangle Bounds { get; }
        bool CursorVisible { get; set; }
        bool Focused { get; }

        event Action Resized;
        event Action Closing;
        event Action Closed;
        event Action FocusLost;
        event Action FocusGained;

        InputSnapshot GetInputSnapshot();
        Point ScreenToClient(Point p);
        Point ClientToScreen(Point p);
        void Close();
    }
}
