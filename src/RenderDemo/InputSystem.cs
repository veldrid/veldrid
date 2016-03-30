using OpenTK.Input;
using System.Collections.Generic;
using System.Numerics;
using Veldrid.Platform;
using System;

namespace Veldrid.RenderDemo
{
    public static class InputSystem
    {
        private static HashSet<Key> _currentlyPressedKeys = new HashSet<Key>();
        private static HashSet<Key> _newKeysThisFrame = new HashSet<Key>();
        private static HashSet<MouseButton> _currentlyPressedMouseButtons = new HashSet<MouseButton>();
        private static HashSet<MouseButton> _newMouseButtonsThisFrame = new HashSet<MouseButton>();

        public static Vector2 MousePosition;

        public static void UpdateFrameInput(InputSnapshot snapshot)
        {
            MousePosition = snapshot.MousePosition;
            foreach (var ke in snapshot.KeyEvents)
            {
                if (ke.Down)
                {
                    KeyDown(ke.Key);
                }
                else
                {
                    KeyUp(ke.Key);
                }
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
