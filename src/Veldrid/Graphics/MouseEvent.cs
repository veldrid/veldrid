using OpenTK.Input;

namespace Veldrid.Graphics
{
    public struct MouseEvent
    {
        public MouseButton MouseButton { get; }
        public bool Down { get; }

        public MouseEvent(MouseButton key, bool down)
        {
            MouseButton = key;
            Down = down;
        }
    }
}