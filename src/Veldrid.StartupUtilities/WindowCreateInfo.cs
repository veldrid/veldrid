namespace Veldrid.StartupUtilities
{
    public struct WindowCreateInfo
    {
        public int X;
        public int Y;
        public int WindowWidth;
        public int WindowHeight;
        public WindowState WindowInitialState;
        public string WindowTitle;
        public bool HiDpi;

        public WindowCreateInfo(
            int x,
            int y,
            int windowWidth,
            int windowHeight,
            WindowState windowInitialState,
            string windowTitle)
            : this(x, y, windowWidth, windowHeight, windowInitialState, windowTitle, false)
        {
        }

        public WindowCreateInfo(
            int x,
            int y,
            int windowWidth,
            int windowHeight,
            WindowState windowInitialState,
            string windowTitle,
            bool hiDpi)
        {
            X = x;
            Y = y;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            WindowInitialState = windowInitialState;
            WindowTitle = windowTitle;
            HiDpi = hiDpi;
        }
    }
}
