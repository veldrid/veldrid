using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    public static unsafe partial class Sdl2Native
    {
        private delegate void SDL_PumpEvents_t();
        private static SDL_PumpEvents_t s_sdl_pumpEvents = LoadFunction<SDL_PumpEvents_t>("SDL_PumpEvents");
        public static void SDL_PumpEvents() => s_sdl_pumpEvents();

        private delegate int SDL_PollEvent_t(SDL_Event* @event);
        private static SDL_PollEvent_t s_sdl_pollEvent = LoadFunction<SDL_PollEvent_t>("SDL_PollEvent");
        public static int SDL_PollEvent(SDL_Event* @event) => s_sdl_pollEvent(@event);
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SDL_Event
    {
        [FieldOffset(0)]
        public SDL_EventType type;

        [FieldOffset(0)]
        private Bytex56 __padding;
        private unsafe struct Bytex56 { private fixed byte bytes[56]; }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_WindowEvent
    {
        public SDL_EventType type;        /**< ::SDL_WINDOWEVENT */
        public uint timestamp;
        public uint windowID;    /**< The associated window */
        public SDL_WindowEventID @event;        /**< ::SDL_WindowEventID */
        private byte padding1;
        private byte padding2;
        private byte padding3;
        public int data1;       /**< event dependent data */
        public int data2;       /**< event dependent data */
    }

    public enum SDL_WindowEventID : byte
    {
        None,           /**< Never used */
        Shown,          /**< Window has been shown */
        Hidden,         /**< Window has been hidden */
        Exposed,        /**< Window has been exposed and should be
                                             redrawn */
        Moved,          /**< Window has been moved to data1, data2
                                         */
        Resized,        /**< Window has been resized to data1xdata2 */
        SizeChanged,   /**< The window size has changed, either as
                                             a result of an API call or through the
                                             system or user changing the window size. */
        Minimized,      /**< Window has been minimized */
        Maximized,      /**< Window has been maximized */
        Restored,       /**< Window has been restored to normal size
                                             and position */
        Enter,          /**< Window has gained mouse focus */
        Leave,          /**< Window has lost mouse focus */
        FocusGained,   /**< Window has gained keyboard focus */
        FocusLost,     /**< Window has lost keyboard focus */

        /// <summary>
        /// The window manager requests that the window be closed.
        /// </summary>
        Close,
        TakeFocus,     /**< Window is being offered a focus (should SetWindowInputFocus() on itself or a subwindow, or ignore) */
        HitTest        /**< Window had a hit test that wasn't SDL_HITTEST_NORMAL. */
    }

    /// <summary>
    /// The types of events that can be delivered.
    /// </summary>
    public enum SDL_EventType
    {
        FirstEvent = 0,     /**< Unused (do not remove) */

        /* Application events */

        /// <summary>
        /// User-requested quit.
        /// </summary>
        Quit = 0x100,

        /// <summary>
        /// The application is being terminated by the OS.
        /// Called on iOS in applicationWillTerminate()
        /// Called on Android in onDestroy()
        /// </summary>
        Terminating,

        /// <summary>
        /// The application is low on memory, free memory if possible.
        /// Called on iOS in applicationDidReceiveMemoryWarning()
        /// Called on Android in onLowMemory()
        /// </summary>
        LowMemory,

        WillEnterBackground, /**< The application is about to enter the background
                                          Called on iOS in applicationWillResignActive()
                                          Called on Android in onPause()
                                     */
        DidEnterBackground, /**< The application did enter the background and may not get CPU for some time
                                          Called on iOS in applicationDidEnterBackground()
                                          Called on Android in onPause()
                                     */
        WillEnterForeground, /**< The application is about to enter the foreground
                                          Called on iOS in applicationWillEnterForeground()
                                          Called on Android in onResume()
                                     */
        DidEnterForeground, /**< The application is now interactive
                                          Called on iOS in applicationDidBecomeActive()
                                          Called on Android in onResume()
                                     */

        /* Window events */
        WindowEvent = 0x200, /**< Window state change */
        SysWMEvent,             /**< System specific event */

        /* Keyboard events */
        KeyDown = 0x300, /**< Key pressed */
        KeyUp,                  /**< Key released */
        TextEditing,            /**< Keyboard text editing (composition) */
        TextInput,              /**< Keyboard text input */
        KeyMapChanged,          /**< Keymap changed due to a system event such as an
                                          input language or keyboard layout change.
                                     */

        /* Mouse events */
        MouseMotion = 0x400, /**< Mouse moved */
        MouseButtonDown,        /**< Mouse button pressed */
        MouseButtonUp,          /**< Mouse button released */
        MouseWheel,             /**< Mouse wheel motion */

        /* Joystick events */
        JoyAxisMotion = 0x600, /**< Joystick axis motion */
        JoyBallMotion,          /**< Joystick trackball motion */
        JoyHatMotion,           /**< Joystick hat position change */
        JoyButtonDown,          /**< Joystick button pressed */
        JoyButtonUp,            /**< Joystick button released */
        JoyDeviceAdded,         /**< A new joystick has been inserted into the system */
        JoyDeviceRemoved,       /**< An opened joystick has been removed */

        /* Game controller events */
        ControllerAxisMotion = 0x650, /**< Game controller axis motion */
        ControllerButtonDown,          /**< Game controller button pressed */
        ControllerButtonUp,            /**< Game controller button released */
        ControllerDeviceAdded,         /**< A new Game controller has been inserted into the system */
        ControllerDeviceRemoved,       /**< An opened Game controller has been removed */
        ControllerDeviceRemapped,      /**< The controller mapping was updated */

        /* Touch events */
        FingerDown = 0x700,
        FingerUp,
        FingerMotion,

        /* Gesture events */
        DollarGesture = 0x800,
        DollarRecord,
        MultiGesture,

        /* Clipboard events */
        ClipboardUpdate = 0x900, /**< The clipboard changed */

        /* Drag and drop events */
        DropFile = 0x1000, /**< The system requests a file open */
        DropTest,                 /**< text/plain drag-and-drop event */
        DropBegin,                /**< A new set of drops is beginning (NULL filename) */
        DropComplete,             /**< Current set of drops is now complete (NULL filename) */

        /* Audio hotplug events */
        AudioDeviceAdded = 0x1100, /**< A new audio device is available */
        AudioDeviceRemoved,        /**< An audio device has been removed. */

        /* Render events */
        RenderTargetsReset = 0x2000, /**< The render targets have been reset and their contents need to be updated */
        RenderDeviceReset, /**< The device has been reset and all textures need to be recreated */

        /** Events ::SDL_USEREVENT through ::SDL_LASTEVENT are for your use,
         *  and should be allocated with SDL_RegisterEvents()
         */
        UserEvent = 0x8000,

        /**
         *  This last event is only for bounding internal arrays
         */
        LastEvent = 0xFFFF
    }

    /// <summary>
    /// Mouse motion event structure (event.motion.*)
    /// </summary>
    public struct SDL_MouseMotionEvent
    {
        public SDL_EventType type;
        public uint timestamp;
        /// <summary>
        /// The window with mouse focus, if any.
        /// </summary>
        public uint windowID;
        /// <summary>
        /// The mouse instance id, or SDL_TOUCH_MOUSEID.
        /// </summary>
        public uint which;
        /// <summary>
        /// The current button state.
        /// </summary>
        public ButtonState state;
        /// <summary>
        /// X coordinate, relative to window.
        /// </summary>
        public int x;
        /// <summary>
        /// Y coordinate, relative to window.
        /// </summary>
        public int y;
        /// <summary>
        /// The relative motion in the X direction.
        /// </summary>
        public int xrel;
        /// <summary>
        /// The relative motion in the Y direction.
        /// </summary>
        public int yrel;
    }

    /// <summary>
    /// Mouse button event structure (event.button.*)
    /// </summary>
    public struct SDL_MouseButtonEvent
    {
        /// <summary>
        /// SDL_MOUSEBUTTONDOWN or ::SDL_MOUSEBUTTONUP.
        /// </summary>
        public SDL_EventType type;
        public uint timestamp;
        /// <summary>
        /// The window with mouse focus, if any.
        /// </summary>
        public uint windowID;
        /// <summary>
        /// The mouse instance id, or SDL_TOUCH_MOUSEID.
        /// </summary>
        public uint which;
        /// <summary>
        /// The mouse button index.
        /// </summary>
        public SDL_MouseButton button;
        /// <summary>
        /// Pressed (1) or Released (0).
        /// </summary>
        public byte state;
        /// <summary>
        /// 1 for single-click, 2 for double-click, etc.
        /// </summary>
        public byte clicks;
        public byte padding1;
        /// <summary>
        /// X coordinate, relative to window.
        /// </summary>
        public int x;
        /// <summary>
        /// Y coordinate, relative to window
        /// </summary>
        public int y;
    }

    /// <summary>
    /// Mouse wheel event structure (event.wheel.*).
    /// </summary>
    public struct SDL_MouseWheelEvent
    {
        /// <summary>
        /// SDL_MOUSEWHEEL.
        /// </summary>
        public SDL_EventType type;
        public uint timestamp;
        /// <summary>
        /// The window with mouse focus, if any.
        /// </summary>
        public uint windowID;
        /// <summary>
        /// The mouse instance id, or SDL_TOUCH_MOUSEID.
        /// </summary>
        public uint which;
        /// <summary>
        /// The amount scrolled horizontally, positive to the right and negative to the left.
        /// </summary>
        public int x;
        /// <summary>
        /// The amount scrolled vertically, positive away from the user and negative toward the user.
        /// </summary>
        public int y;
        /// <summary>
        /// Set to one of the SDL_MOUSEWHEEL_* defines. When FLIPPED the values in X and Y will be opposite. Multiply by -1 to change them back.
        /// </summary>
        public uint direction;
    }


    [Flags]
    public enum ButtonState : uint
    {
        Left = 1 << 0,
        Middle = 1 << 1,
        Right = 1 << 2,
        X1 = 1 << 3,
        X2 = 1 << 4,
    }

    /// <summary>
    /// Keyboard button event structure (event.key.*).
    /// </summary>
    public struct SDL_KeyboardEvent
    {
        public SDL_EventType type;        /**< ::SDL_KEYDOWN or ::SDL_KEYUP */
        public uint timestamp;
        public uint windowID;    /**< The window with keyboard focus, if any */
        /// <summary>
        /// Pressed (1) or Released (0).
        /// </summary>
        public byte state;
        public byte repeat;       /**< Non-zero if this is a key repeat */
        public byte padding2;
        public byte padding3;
        public SDL_Keysym keysym;  /**< The key that was pressed or released */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Keysym
    {
        /// <summary>
        /// SDL physical key code.
        /// </summary>
        public SDL_Scancode scancode;
        /// <summary>
        /// SDL virtual key code.
        /// </summary>
        public SDL_Keycode sym;
        /// <summary>
        /// current key modifiers.
        /// </summary>
        public SDL_Keymod mod;
        private uint __unused;
    }

    public enum SDL_MouseButton : byte
    {
        Left = 1,
        Middle = 2,
        Right = 3,
        X1 = 4,
        X2 = 5,
    }

    /// <summary>
    /// Keyboard text input event structure (event.text.*)
    /// </summary>
    public unsafe struct SDL_TextInputEvent
    {
        public const int MaxTextSize = 32;

        /// <summary>
        /// SDL_TEXTINPUT.
        /// </summary>
        public SDL_EventType type;
        public uint timestamp;
        /// <summary>
        /// The window with keyboard focus, if any.
        /// </summary>
        public uint windowID;
        /// <summary>
        /// The input text.
        /// </summary>
        public fixed byte text[MaxTextSize];
    }
}
