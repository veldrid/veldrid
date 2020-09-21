namespace Veldrid
{
    public struct KeyEvent
    {
        public Key Key { get; }
        public bool Down { get; }
        public ModifierKeys Modifiers { get; }
        public bool Repeat { get; }

        public KeyEvent(Key key, bool down, ModifierKeys modifiers)
        : this(key, down, modifiers, false)
        {
        }

        public KeyEvent(Key key, bool down, ModifierKeys modifiers, bool repeat)
        {
            Key = key;
            Down = down;
            Modifiers = modifiers;
            Repeat = repeat;
        }

        public override string ToString() => $"{Key} {(Down ? "Down" : "Up")} [{Modifiers}] (repeat={Repeat})";
    }

    public enum Key
    {
        //
        // Summary:
        //     A key outside the known keys.
        Unknown = 0,
        //
        // Summary:
        //     The left shift key.
        ShiftLeft = 1,
        //
        // Summary:
        //     The left shift key (equivalent to ShiftLeft).
        LShift = 1,
        //
        // Summary:
        //     The right shift key.
        ShiftRight = 2,
        //
        // Summary:
        //     The right shift key (equivalent to ShiftRight).
        RShift = 2,
        //
        // Summary:
        //     The left control key.
        ControlLeft = 3,
        //
        // Summary:
        //     The left control key (equivalent to ControlLeft).
        LControl = 3,
        //
        // Summary:
        //     The right control key.
        ControlRight = 4,
        //
        // Summary:
        //     The right control key (equivalent to ControlRight).
        RControl = 4,
        //
        // Summary:
        //     The left alt key.
        AltLeft = 5,
        //
        // Summary:
        //     The left alt key (equivalent to AltLeft.
        LAlt = 5,
        //
        // Summary:
        //     The right alt key.
        AltRight = 6,
        //
        // Summary:
        //     The right alt key (equivalent to AltRight).
        RAlt = 6,
        //
        // Summary:
        //     The left win key.
        WinLeft = 7,
        //
        // Summary:
        //     The left win key (equivalent to WinLeft).
        LWin = 7,
        //
        // Summary:
        //     The right win key.
        WinRight = 8,
        //
        // Summary:
        //     The right win key (equivalent to WinRight).
        RWin = 8,
        //
        // Summary:
        //     The menu key.
        Menu = 9,
        //
        // Summary:
        //     The F1 key.
        F1 = 10,
        //
        // Summary:
        //     The F2 key.
        F2 = 11,
        //
        // Summary:
        //     The F3 key.
        F3 = 12,
        //
        // Summary:
        //     The F4 key.
        F4 = 13,
        //
        // Summary:
        //     The F5 key.
        F5 = 14,
        //
        // Summary:
        //     The F6 key.
        F6 = 15,
        //
        // Summary:
        //     The F7 key.
        F7 = 16,
        //
        // Summary:
        //     The F8 key.
        F8 = 17,
        //
        // Summary:
        //     The F9 key.
        F9 = 18,
        //
        // Summary:
        //     The F10 key.
        F10 = 19,
        //
        // Summary:
        //     The F11 key.
        F11 = 20,
        //
        // Summary:
        //     The F12 key.
        F12 = 21,
        //
        // Summary:
        //     The F13 key.
        F13 = 22,
        //
        // Summary:
        //     The F14 key.
        F14 = 23,
        //
        // Summary:
        //     The F15 key.
        F15 = 24,
        //
        // Summary:
        //     The F16 key.
        F16 = 25,
        //
        // Summary:
        //     The F17 key.
        F17 = 26,
        //
        // Summary:
        //     The F18 key.
        F18 = 27,
        //
        // Summary:
        //     The F19 key.
        F19 = 28,
        //
        // Summary:
        //     The F20 key.
        F20 = 29,
        //
        // Summary:
        //     The F21 key.
        F21 = 30,
        //
        // Summary:
        //     The F22 key.
        F22 = 31,
        //
        // Summary:
        //     The F23 key.
        F23 = 32,
        //
        // Summary:
        //     The F24 key.
        F24 = 33,
        //
        // Summary:
        //     The F25 key.
        F25 = 34,
        //
        // Summary:
        //     The F26 key.
        F26 = 35,
        //
        // Summary:
        //     The F27 key.
        F27 = 36,
        //
        // Summary:
        //     The F28 key.
        F28 = 37,
        //
        // Summary:
        //     The F29 key.
        F29 = 38,
        //
        // Summary:
        //     The F30 key.
        F30 = 39,
        //
        // Summary:
        //     The F31 key.
        F31 = 40,
        //
        // Summary:
        //     The F32 key.
        F32 = 41,
        //
        // Summary:
        //     The F33 key.
        F33 = 42,
        //
        // Summary:
        //     The F34 key.
        F34 = 43,
        //
        // Summary:
        //     The F35 key.
        F35 = 44,
        //
        // Summary:
        //     The up arrow key.
        Up = 45,
        //
        // Summary:
        //     The down arrow key.
        Down = 46,
        //
        // Summary:
        //     The left arrow key.
        Left = 47,
        //
        // Summary:
        //     The right arrow key.
        Right = 48,
        //
        // Summary:
        //     The enter key.
        Enter = 49,
        //
        // Summary:
        //     The escape key.
        Escape = 50,
        //
        // Summary:
        //     The space key.
        Space = 51,
        //
        // Summary:
        //     The tab key.
        Tab = 52,
        //
        // Summary:
        //     The backspace key.
        BackSpace = 53,
        //
        // Summary:
        //     The backspace key (equivalent to BackSpace).
        Back = 53,
        //
        // Summary:
        //     The insert key.
        Insert = 54,
        //
        // Summary:
        //     The delete key.
        Delete = 55,
        //
        // Summary:
        //     The page up key.
        PageUp = 56,
        //
        // Summary:
        //     The page down key.
        PageDown = 57,
        //
        // Summary:
        //     The home key.
        Home = 58,
        //
        // Summary:
        //     The end key.
        End = 59,
        //
        // Summary:
        //     The caps lock key.
        CapsLock = 60,
        //
        // Summary:
        //     The scroll lock key.
        ScrollLock = 61,
        //
        // Summary:
        //     The print screen key.
        PrintScreen = 62,
        //
        // Summary:
        //     The pause key.
        Pause = 63,
        //
        // Summary:
        //     The num lock key.
        NumLock = 64,
        //
        // Summary:
        //     The clear key (Keypad5 with NumLock disabled, on typical keyboards).
        Clear = 65,
        //
        // Summary:
        //     The sleep key.
        Sleep = 66,
        //
        // Summary:
        //     The keypad 0 key.
        Keypad0 = 67,
        //
        // Summary:
        //     The keypad 1 key.
        Keypad1 = 68,
        //
        // Summary:
        //     The keypad 2 key.
        Keypad2 = 69,
        //
        // Summary:
        //     The keypad 3 key.
        Keypad3 = 70,
        //
        // Summary:
        //     The keypad 4 key.
        Keypad4 = 71,
        //
        // Summary:
        //     The keypad 5 key.
        Keypad5 = 72,
        //
        // Summary:
        //     The keypad 6 key.
        Keypad6 = 73,
        //
        // Summary:
        //     The keypad 7 key.
        Keypad7 = 74,
        //
        // Summary:
        //     The keypad 8 key.
        Keypad8 = 75,
        //
        // Summary:
        //     The keypad 9 key.
        Keypad9 = 76,
        //
        // Summary:
        //     The keypad divide key.
        KeypadDivide = 77,
        //
        // Summary:
        //     The keypad multiply key.
        KeypadMultiply = 78,
        //
        // Summary:
        //     The keypad subtract key.
        KeypadSubtract = 79,
        //
        // Summary:
        //     The keypad minus key (equivalent to KeypadSubtract).
        KeypadMinus = 79,
        //
        // Summary:
        //     The keypad add key.
        KeypadAdd = 80,
        //
        // Summary:
        //     The keypad plus key (equivalent to KeypadAdd).
        KeypadPlus = 80,
        //
        // Summary:
        //     The keypad decimal key.
        KeypadDecimal = 81,
        //
        // Summary:
        //     The keypad period key (equivalent to KeypadDecimal).
        KeypadPeriod = 81,
        //
        // Summary:
        //     The keypad enter key.
        KeypadEnter = 82,
        //
        // Summary:
        //     The A key.
        A = 83,
        //
        // Summary:
        //     The B key.
        B = 84,
        //
        // Summary:
        //     The C key.
        C = 85,
        //
        // Summary:
        //     The D key.
        D = 86,
        //
        // Summary:
        //     The E key.
        E = 87,
        //
        // Summary:
        //     The F key.
        F = 88,
        //
        // Summary:
        //     The G key.
        G = 89,
        //
        // Summary:
        //     The H key.
        H = 90,
        //
        // Summary:
        //     The I key.
        I = 91,
        //
        // Summary:
        //     The J key.
        J = 92,
        //
        // Summary:
        //     The K key.
        K = 93,
        //
        // Summary:
        //     The L key.
        L = 94,
        //
        // Summary:
        //     The M key.
        M = 95,
        //
        // Summary:
        //     The N key.
        N = 96,
        //
        // Summary:
        //     The O key.
        O = 97,
        //
        // Summary:
        //     The P key.
        P = 98,
        //
        // Summary:
        //     The Q key.
        Q = 99,
        //
        // Summary:
        //     The R key.
        R = 100,
        //
        // Summary:
        //     The S key.
        S = 101,
        //
        // Summary:
        //     The T key.
        T = 102,
        //
        // Summary:
        //     The U key.
        U = 103,
        //
        // Summary:
        //     The V key.
        V = 104,
        //
        // Summary:
        //     The W key.
        W = 105,
        //
        // Summary:
        //     The X key.
        X = 106,
        //
        // Summary:
        //     The Y key.
        Y = 107,
        //
        // Summary:
        //     The Z key.
        Z = 108,
        //
        // Summary:
        //     The number 0 key.
        Number0 = 109,
        //
        // Summary:
        //     The number 1 key.
        Number1 = 110,
        //
        // Summary:
        //     The number 2 key.
        Number2 = 111,
        //
        // Summary:
        //     The number 3 key.
        Number3 = 112,
        //
        // Summary:
        //     The number 4 key.
        Number4 = 113,
        //
        // Summary:
        //     The number 5 key.
        Number5 = 114,
        //
        // Summary:
        //     The number 6 key.
        Number6 = 115,
        //
        // Summary:
        //     The number 7 key.
        Number7 = 116,
        //
        // Summary:
        //     The number 8 key.
        Number8 = 117,
        //
        // Summary:
        //     The number 9 key.
        Number9 = 118,
        //
        // Summary:
        //     The tilde key.
        Tilde = 119,
        //
        // Summary:
        //     The grave key (equivaent to Tilde).
        Grave = 119,
        //
        // Summary:
        //     The minus key.
        Minus = 120,
        //
        // Summary:
        //     The plus key.
        Plus = 121,
        //
        // Summary:
        //     The left bracket key.
        BracketLeft = 122,
        //
        // Summary:
        //     The left bracket key (equivalent to BracketLeft).
        LBracket = 122,
        //
        // Summary:
        //     The right bracket key.
        BracketRight = 123,
        //
        // Summary:
        //     The right bracket key (equivalent to BracketRight).
        RBracket = 123,
        //
        // Summary:
        //     The semicolon key.
        Semicolon = 124,
        //
        // Summary:
        //     The quote key.
        Quote = 125,
        //
        // Summary:
        //     The comma key.
        Comma = 126,
        //
        // Summary:
        //     The period key.
        Period = 127,
        //
        // Summary:
        //     The slash key.
        Slash = 128,
        //
        // Summary:
        //     The backslash key.
        BackSlash = 129,
        //
        // Summary:
        //     The secondary backslash key.
        NonUSBackSlash = 130,
        //
        // Summary:
        //     Indicates the last available keyboard key.
        LastKey = 131
    }
}