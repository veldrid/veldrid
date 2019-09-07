namespace Veldrid.Sdl2
{
    internal static class KeyMapper
    {
        public static MouseButton MapMouseButton(SDL_MouseButton button)
        {
            switch (button)
            {
                case SDL_MouseButton.Left:
                    return MouseButton.Left;
                case SDL_MouseButton.Middle:
                    return MouseButton.Middle;
                case SDL_MouseButton.Right:
                    return MouseButton.Right;
                case SDL_MouseButton.X1:
                    return MouseButton.Button1;
                case SDL_MouseButton.X2:
                    return MouseButton.Button2;
                default:
                    return MouseButton.Left;
            }
        }

        public static ModifierKeys MapModifierKeys(SDL_Keymod mod)
        {
            ModifierKeys mods = ModifierKeys.None;
            if ((mod & (SDL_Keymod.LeftShift | SDL_Keymod.RightShift)) != 0)
            {
                mods |= ModifierKeys.Shift;
            }
            if ((mod & (SDL_Keymod.LeftAlt | SDL_Keymod.RightAlt)) != 0)
            {
                mods |= ModifierKeys.Alt;
            }
            if ((mod & (SDL_Keymod.LeftControl | SDL_Keymod.RightControl)) != 0)
            {
                mods |= ModifierKeys.Control;
            }

            return mods;
        }

        public static Key MapKey(SDL_Keysym keysym)
        {
            switch (keysym.scancode)
            {
                case SDL_Scancode.SDL_SCANCODE_A:
                    return Key.A;
                case SDL_Scancode.SDL_SCANCODE_B:
                    return Key.B;
                case SDL_Scancode.SDL_SCANCODE_C:
                    return Key.C;
                case SDL_Scancode.SDL_SCANCODE_D:
                    return Key.D;
                case SDL_Scancode.SDL_SCANCODE_E:
                    return Key.E;
                case SDL_Scancode.SDL_SCANCODE_F:
                    return Key.F;
                case SDL_Scancode.SDL_SCANCODE_G:
                    return Key.G;
                case SDL_Scancode.SDL_SCANCODE_H:
                    return Key.H;
                case SDL_Scancode.SDL_SCANCODE_I:
                    return Key.I;
                case SDL_Scancode.SDL_SCANCODE_J:
                    return Key.J;
                case SDL_Scancode.SDL_SCANCODE_K:
                    return Key.K;
                case SDL_Scancode.SDL_SCANCODE_L:
                    return Key.L;
                case SDL_Scancode.SDL_SCANCODE_M:
                    return Key.M;
                case SDL_Scancode.SDL_SCANCODE_N:
                    return Key.N;
                case SDL_Scancode.SDL_SCANCODE_O:
                    return Key.O;
                case SDL_Scancode.SDL_SCANCODE_P:
                    return Key.P;
                case SDL_Scancode.SDL_SCANCODE_Q:
                    return Key.Q;
                case SDL_Scancode.SDL_SCANCODE_R:
                    return Key.R;
                case SDL_Scancode.SDL_SCANCODE_S:
                    return Key.S;
                case SDL_Scancode.SDL_SCANCODE_T:
                    return Key.T;
                case SDL_Scancode.SDL_SCANCODE_U:
                    return Key.U;
                case SDL_Scancode.SDL_SCANCODE_V:
                    return Key.V;
                case SDL_Scancode.SDL_SCANCODE_W:
                    return Key.W;
                case SDL_Scancode.SDL_SCANCODE_X:
                    return Key.X;
                case SDL_Scancode.SDL_SCANCODE_Y:
                    return Key.Y;
                case SDL_Scancode.SDL_SCANCODE_Z:
                    return Key.Z;
                case SDL_Scancode.SDL_SCANCODE_1:
                    return Key.Number1;
                case SDL_Scancode.SDL_SCANCODE_2:
                    return Key.Number2;
                case SDL_Scancode.SDL_SCANCODE_3:
                    return Key.Number3;
                case SDL_Scancode.SDL_SCANCODE_4:
                    return Key.Number4;
                case SDL_Scancode.SDL_SCANCODE_5:
                    return Key.Number5;
                case SDL_Scancode.SDL_SCANCODE_6:
                    return Key.Number6;
                case SDL_Scancode.SDL_SCANCODE_7:
                    return Key.Number7;
                case SDL_Scancode.SDL_SCANCODE_8:
                    return Key.Number8;
                case SDL_Scancode.SDL_SCANCODE_9:
                    return Key.Number9;
                case SDL_Scancode.SDL_SCANCODE_0:
                    return Key.Number0;
                case SDL_Scancode.SDL_SCANCODE_RETURN:
                    return Key.Enter;
                case SDL_Scancode.SDL_SCANCODE_ESCAPE:
                    return Key.Escape;
                case SDL_Scancode.SDL_SCANCODE_BACKSPACE:
                    return Key.BackSpace;
                case SDL_Scancode.SDL_SCANCODE_TAB:
                    return Key.Tab;
                case SDL_Scancode.SDL_SCANCODE_SPACE:
                    return Key.Space;
                case SDL_Scancode.SDL_SCANCODE_MINUS:
                    return Key.Minus;
                case SDL_Scancode.SDL_SCANCODE_EQUALS:
                    return Key.Plus;
                case SDL_Scancode.SDL_SCANCODE_LEFTBRACKET:
                    return Key.BracketLeft;
                case SDL_Scancode.SDL_SCANCODE_RIGHTBRACKET:
                    return Key.BracketRight;
                case SDL_Scancode.SDL_SCANCODE_BACKSLASH:
                    return Key.BackSlash;
                case SDL_Scancode.SDL_SCANCODE_SEMICOLON:
                    return Key.Semicolon;
                case SDL_Scancode.SDL_SCANCODE_APOSTROPHE:
                    return Key.Quote;
                case SDL_Scancode.SDL_SCANCODE_GRAVE:
                    return Key.Grave;
                case SDL_Scancode.SDL_SCANCODE_COMMA:
                    return Key.Comma;
                case SDL_Scancode.SDL_SCANCODE_PERIOD:
                    return Key.Period;
                case SDL_Scancode.SDL_SCANCODE_SLASH:
                    return Key.Slash;
                case SDL_Scancode.SDL_SCANCODE_CAPSLOCK:
                    return Key.CapsLock;
                case SDL_Scancode.SDL_SCANCODE_F1:
                    return Key.F1;
                case SDL_Scancode.SDL_SCANCODE_F2:
                    return Key.F2;
                case SDL_Scancode.SDL_SCANCODE_F3:
                    return Key.F3;
                case SDL_Scancode.SDL_SCANCODE_F4:
                    return Key.F4;
                case SDL_Scancode.SDL_SCANCODE_F5:
                    return Key.F5;
                case SDL_Scancode.SDL_SCANCODE_F6:
                    return Key.F6;
                case SDL_Scancode.SDL_SCANCODE_F7:
                    return Key.F7;
                case SDL_Scancode.SDL_SCANCODE_F8:
                    return Key.F8;
                case SDL_Scancode.SDL_SCANCODE_F9:
                    return Key.F9;
                case SDL_Scancode.SDL_SCANCODE_F10:
                    return Key.F10;
                case SDL_Scancode.SDL_SCANCODE_F11:
                    return Key.F11;
                case SDL_Scancode.SDL_SCANCODE_F12:
                    return Key.F12;
                case SDL_Scancode.SDL_SCANCODE_PRINTSCREEN:
                    return Key.PrintScreen;
                case SDL_Scancode.SDL_SCANCODE_SCROLLLOCK:
                    return Key.ScrollLock;
                case SDL_Scancode.SDL_SCANCODE_PAUSE:
                    return Key.Pause;
                case SDL_Scancode.SDL_SCANCODE_INSERT:
                    return Key.Insert;
                case SDL_Scancode.SDL_SCANCODE_HOME:
                    return Key.Home;
                case SDL_Scancode.SDL_SCANCODE_PAGEUP:
                    return Key.PageUp;
                case SDL_Scancode.SDL_SCANCODE_DELETE:
                    return Key.Delete;
                case SDL_Scancode.SDL_SCANCODE_END:
                    return Key.End;
                case SDL_Scancode.SDL_SCANCODE_PAGEDOWN:
                    return Key.PageDown;
                case SDL_Scancode.SDL_SCANCODE_RIGHT:
                    return Key.Right;
                case SDL_Scancode.SDL_SCANCODE_LEFT:
                    return Key.Left;
                case SDL_Scancode.SDL_SCANCODE_DOWN:
                    return Key.Down;
                case SDL_Scancode.SDL_SCANCODE_UP:
                    return Key.Up;
                case SDL_Scancode.SDL_SCANCODE_NUMLOCKCLEAR:
                    return Key.NumLock;
                case SDL_Scancode.SDL_SCANCODE_KP_DIVIDE:
                    return Key.KeypadDivide;
                case SDL_Scancode.SDL_SCANCODE_KP_MULTIPLY:
                    return Key.KeypadMultiply;
                case SDL_Scancode.SDL_SCANCODE_KP_MINUS:
                    return Key.KeypadMinus;
                case SDL_Scancode.SDL_SCANCODE_KP_PLUS:
                    return Key.KeypadPlus;
                case SDL_Scancode.SDL_SCANCODE_KP_ENTER:
                    return Key.KeypadEnter;
                case SDL_Scancode.SDL_SCANCODE_KP_1:
                    return Key.Keypad1;
                case SDL_Scancode.SDL_SCANCODE_KP_2:
                    return Key.Keypad2;
                case SDL_Scancode.SDL_SCANCODE_KP_3:
                    return Key.Keypad3;
                case SDL_Scancode.SDL_SCANCODE_KP_4:
                    return Key.Keypad4;
                case SDL_Scancode.SDL_SCANCODE_KP_5:
                    return Key.Keypad5;
                case SDL_Scancode.SDL_SCANCODE_KP_6:
                    return Key.Keypad6;
                case SDL_Scancode.SDL_SCANCODE_KP_7:
                    return Key.Keypad7;
                case SDL_Scancode.SDL_SCANCODE_KP_8:
                    return Key.Keypad8;
                case SDL_Scancode.SDL_SCANCODE_KP_9:
                    return Key.Keypad9;
                case SDL_Scancode.SDL_SCANCODE_KP_0:
                    return Key.Keypad0;
                case SDL_Scancode.SDL_SCANCODE_KP_PERIOD:
                    return Key.KeypadPeriod;
                case SDL_Scancode.SDL_SCANCODE_NONUSBACKSLASH:
                    return Key.NonUSBackSlash;
                case SDL_Scancode.SDL_SCANCODE_KP_EQUALS:
                    return Key.KeypadPlus;
                case SDL_Scancode.SDL_SCANCODE_F13:
                    return Key.F13;
                case SDL_Scancode.SDL_SCANCODE_F14:
                    return Key.F14;
                case SDL_Scancode.SDL_SCANCODE_F15:
                    return Key.F15;
                case SDL_Scancode.SDL_SCANCODE_F16:
                    return Key.F16;
                case SDL_Scancode.SDL_SCANCODE_F17:
                    return Key.F17;
                case SDL_Scancode.SDL_SCANCODE_F18:
                    return Key.F18;
                case SDL_Scancode.SDL_SCANCODE_F19:
                    return Key.F19;
                case SDL_Scancode.SDL_SCANCODE_F20:
                    return Key.F20;
                case SDL_Scancode.SDL_SCANCODE_F21:
                    return Key.F21;
                case SDL_Scancode.SDL_SCANCODE_F22:
                    return Key.F22;
                case SDL_Scancode.SDL_SCANCODE_F23:
                    return Key.F23;
                case SDL_Scancode.SDL_SCANCODE_F24:
                    return Key.F24;
                case SDL_Scancode.SDL_SCANCODE_MENU:
                    return Key.Menu;
                case SDL_Scancode.SDL_SCANCODE_LCTRL:
                    return Key.ControlLeft;
                case SDL_Scancode.SDL_SCANCODE_LSHIFT:
                    return Key.ShiftLeft;
                case SDL_Scancode.SDL_SCANCODE_LALT:
                    return Key.AltLeft;
                case SDL_Scancode.SDL_SCANCODE_RCTRL:
                    return Key.ControlRight;
                case SDL_Scancode.SDL_SCANCODE_RSHIFT:
                    return Key.ShiftRight;
                case SDL_Scancode.SDL_SCANCODE_RALT:
                    return Key.AltRight;
                default:
                    return Key.Unknown;
            }
        }
    }
}
