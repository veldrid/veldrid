using System.Collections.Generic;
using System.Numerics;
using Vd2;
using VdSdl2;

namespace TestApp
{
    public static class InputTracker
    {
        private static HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
        private static HashSet<Key> _newKeysThisFrame = new HashSet<Key>();

        private static HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
        private static HashSet<MouseButton> _newMouseButtonsThisFrame = new HashSet<MouseButton>();

        public static Vector2 MousePosition;

        public static void Init(Sdl2Window nw)
        {
            nw.KeyDown += (e) => KeyDown(e.Key);
            nw.KeyUp += (e) => KeyUp(e.Key);
            nw.MouseMove += (e) => MousePosition = e.MousePosition;
            nw.MouseDown += (e) => MouseDown(e.MouseButton);
            nw.MouseUp += (e) => MouseUp(e.MouseButton);
        }

        public static bool GetKey(Key key)
        {
            return _currentlyPressedKeys.Contains(key);
        }

        public static bool GetKeyDown(Key key)
        {
            return _newKeysThisFrame.Contains(key);
        }

        public static bool GetMouseButton(MouseButton button)
        {
            return _currentlyPressedMouseButtons.Contains(button);
        }

        public static bool GetMouseButtonDown(MouseButton button)
        {
            return _newMouseButtonsThisFrame.Contains(button);
        }

        public static void UpdateFrameInput()
        {
            _newKeysThisFrame.Clear();
            _newMouseButtonsThisFrame.Clear();
        }

        private static void MouseUp(MouseButton mouseButton)
        {
            _currentlyPressedMouseButtons.Remove(mouseButton);
            _newMouseButtonsThisFrame.Remove(mouseButton);
        }

        private static void MouseDown(MouseButton mouseButton)
        {
            if (_currentlyPressedMouseButtons.Add(mouseButton))
            {
                _newMouseButtonsThisFrame.Add(mouseButton);
            }
        }

        private static void KeyUp(Key key)
        {
            _currentlyPressedKeys.Remove(key);
            _newKeysThisFrame.Remove(key);
        }

        private static void KeyDown(Key key)
        {
            if (_currentlyPressedKeys.Add(key))
            {
                _newKeysThisFrame.Add(key);
            }
        }
    }
}
