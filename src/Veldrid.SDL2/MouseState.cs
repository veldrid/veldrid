namespace Veldrid.Sdl2
{
    public readonly struct MouseState
    {
        public int X { get; }
        public int Y { get; }
        public MouseButton MouseDown { get; }

        public MouseState(int x, int y, MouseButton mouseDown)
        {
            X = x;
            Y = y;
            MouseDown = mouseDown;
        }

        public bool IsButtonDown(MouseButton button)
        {
            return (MouseDown & button) != 0;
        }
    }
}
