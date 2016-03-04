using OpenTK.Input;

namespace Veldrid.Platform
{
    public struct KeyEvent
    {
        public Key Key { get; }
        public bool Down { get; }
        public ModifierKeys Modifiers { get; }

        public KeyEvent(Key key, bool down, ModifierKeys modifiers)
        {
            Key = key;
            Down = down;
            Modifiers = modifiers;
        }

        public override string ToString() => $"{Key} {(Down ? "Down" : "Up")} [{Modifiers}]";
    }
}