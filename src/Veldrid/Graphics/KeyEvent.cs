using OpenTK.Input;

namespace Veldrid.Graphics
{
    public struct KeyEvent
    {
        public Key Key { get; }
        public bool Down { get; }

        public KeyEvent(Key key, bool down)
        {
            Key = key;
            Down = down;
        }
    }
}