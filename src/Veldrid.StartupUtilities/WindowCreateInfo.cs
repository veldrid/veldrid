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
        
        public WindowCreateInfo(
            int x,
            int y,
            int windowWidth,
            int windowHeight,
            WindowState windowInitialState,
            string windowTitle)
        {
            X = x;
            Y = y;
            WindowWidth = windowWidth;
            WindowHeight = windowHeight;
            WindowInitialState = windowInitialState;
            WindowTitle = windowTitle;
        }
        
        public static WindowCreateInfo CreateCenteredWindow(
            int windowWidth,
            int windowHeight,
            WindowState windowInitialState,
            string windowTitle)
        {
            return new WindowCreateInfo
            {
                X = 0x2FFF0000,
                Y = 0x2FFF0000,
                WindowWidth =  windowWidth,
                WindowHeight = windowHeight,
                WindowInitialState = windowInitialState,
                WindowTitle = windowTitle
            };
        }
    }
}
