using System;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    /// <summary>
    /// A transparent wrapper over a pointer to a native SDL_GameController.
    /// </summary>
    public struct SDL_GameController
    {
        /// <summary>
        /// The native SDL_GameController pointer.
        /// </summary>
        public readonly IntPtr NativePointer;

        public SDL_GameController(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(SDL_GameController controller) => controller.NativePointer;
        public static implicit operator SDL_GameController(IntPtr pointer) => new SDL_GameController(pointer);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_ControllerAxisEvent
    {
        /// <summary>
        /// SDL_CONTROLLERAXISMOTION.
        /// </summary>
        public uint type;
        /// <summary>
        /// In milliseconds, populated using SDL_GetTicks().
        /// </summary>
        public uint timestamp;
        /// <summary>
        /// The joystick instance id.
        /// </summary>
        public int which;
        /// <summary>
        /// The controller axis.
        /// </summary>
        public SDL_GameControllerAxis axis;
        private byte padding1;
        private byte padding2;
        private byte padding3;
        /// <summary>
        /// The axis value (range: -32768 to 32767)
        /// </summary>
        public short value;
        private ushort padding4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_ControllerButtonEvent
    {
        /// <summary>
        /// SDL_CONTROLLERBUTTONDOWN or SDL_CONTROLLERBUTTONUP.
        /// </summary>
        public uint type;
        /// <summary>
        /// In milliseconds, populated using SDL_GetTicks().
        /// </summary>
        public uint timestamp;
        /// <summary>
        /// The joystick instance id.
        /// </summary>
        public int which;
        /// <summary>
        /// The controller button
        /// </summary>
        public SDL_GameControllerButton button;
        /// <summary>
        /// SDL_PRESSED or SDL_RELEASED
        /// </summary>
        public byte state;
        private byte padding1;
        private byte padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_ControllerDeviceEvent
    {
        /// <summary>
        /// SDL_CONTROLLERDEVICEADDED, SDL_CONTROLLERDEVICEREMOVED, or SDL_CONTROLLERDEVICEREMAPPED.
        /// </summary>
        public uint type;
        /// <summary>
        /// In milliseconds, populated using SDL_GetTicks().
        /// </summary>
        public uint timestamp;
        /// <summary>
        /// The joystick device index for the ADDED event, instance id for the REMOVED or REMAPPED event.
        /// </summary>
        public int which;
    }

    /// <summary>
    /// The list of axes available from a controller.
    /// Thumbstick axis values range from SDL_Joystick_AXIS_MIN to SDL_Joystick_AXIS_MAX,
    /// and are centered within ~8000 of zero, though advanced UI will allow users to set
    /// or autodetect the dead zone, which varies between controllers.
    /// Trigger axis values range from 0 to SDL_Joystick_AXIS_MAX.
    /// </summary>
    public enum SDL_GameControllerAxis : byte
    {
        Invalid = unchecked((byte)-1),
        LeftX = 0,
        LeftY,
        RightX,
        RightY,
        TriggerLeft,
        TriggerRight,
        Max,
    }

    /// <summary>
    /// The list of buttons available from a controller.
    /// </summary>
    public enum SDL_GameControllerButton : byte
    {
        Invalid = unchecked((byte)-1),
        A = 0,
        B,
        X,
        Y,
        Back,
        Guide,
        Start,
        LeftStick,
        RightStick,
        LeftShoulder,
        RightShoulder,
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,
        Max
    }

    public static unsafe partial class Sdl2Native
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SDL_GameController SDL_GameControllerOpen_t(int joystick_index);
        private static SDL_GameControllerOpen_t s_sdl_gameControllerOpen = LoadFunction<SDL_GameControllerOpen_t>("SDL_GameControllerOpen");
        /// <summary>
        /// Open a game controller for use.
        /// The index passed as an argument refers to the N'th game controller on the system. This index is not the value which
        /// will identify this controller in future controller events. The joystick's instance id will be used there instead.
        /// </summary>
        /// <returns>A controller identifier, or NULL if an error occurred.</returns>
        public static SDL_GameController SDL_GameControllerOpen(int joystick_index) => s_sdl_gameControllerOpen(joystick_index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_GameControllerClose_t(SDL_GameController gamecontroller);
        private static SDL_GameControllerClose_t s_sdl_gameControllerClose = LoadFunction<SDL_GameControllerClose_t>("SDL_GameControllerClose");
        /// <summary>
        /// Close a controller previously opened with SDL_GameControllerOpen().
        /// </summary>
        public static void SDL_GameControllerClose(SDL_GameController gamecontroller) => s_sdl_gameControllerClose(gamecontroller);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_IsGameController_t(int joystick_index);
        private static SDL_IsGameController_t s_sdl_isGameController = LoadFunction<SDL_IsGameController_t>("SDL_IsGameController");
        /// <summary>
        /// Is the joystick on this index supported by the game controller interface?
        /// </summary>
        public static bool SDL_IsGameController(int joystick_index) => s_sdl_isGameController(joystick_index) != 0;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* SDL_GameControllerNameForIndex_t(int joystick_index);
        private static SDL_GameControllerNameForIndex_t s_sdl_gameControllerNameForIndex = LoadFunction<SDL_GameControllerNameForIndex_t>("SDL_GameControllerNameForIndex");
        /// <summary>
        /// Get the implementation dependent name of a game controller.
        /// This can be called before any controllers are opened.
        /// If no name can be found, this function returns null.
        /// </summary>
        public static byte* SDL_GameControllerNameForIndex(int joystick_index) => s_sdl_gameControllerNameForIndex(joystick_index);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SDL_GameController SDL_GameControllerFromInstanceID_t(int joyid);
        private static SDL_GameControllerFromInstanceID_t s_sdl_gameControllerFromInstanceID = LoadFunction<SDL_GameControllerFromInstanceID_t>("SDL_GameControllerFromInstanceID");
        /// <summary>
        /// Return the SDL_GameController associated with an instance id.
        /// </summary>
        public static SDL_GameController SDL_GameControllerFromInstanceID(int joyid) => s_sdl_gameControllerFromInstanceID(joyid);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte* SDL_GameControllerName_t(SDL_GameController gamecontroller);
        private static SDL_GameControllerName_t s_sdl_gameControllerName = LoadFunction<SDL_GameControllerName_t>("SDL_GameControllerName");
        /// <summary>
        /// Return the name for this currently opened controller.
        /// </summary>
        public static byte* SDL_GameControllerName(SDL_GameController gamecontroller) => s_sdl_gameControllerName(gamecontroller);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate ushort SDL_GameControllerGetVendor_t(SDL_GameController gamecontroller);
        private static SDL_GameControllerGetVendor_t s_sdl_gameControllerGetVendor = LoadFunction<SDL_GameControllerGetVendor_t>("SDL_GameControllerGetVendor");
        /// <summary>
        /// Get the USB vendor ID of an opened controller, if available.
        /// If the vendor ID isn't available this function returns 0.
        /// </summary>
        public static ushort SDL_GameControllerGetVendor(SDL_GameController gamecontroller) => s_sdl_gameControllerGetVendor(gamecontroller);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate ushort SDL_GameControllerGetProduct_t(SDL_GameController gamecontroller);
        private static SDL_GameControllerGetProduct_t s_sdl_gameControllerGetProduct = LoadFunction<SDL_GameControllerGetProduct_t>("SDL_GameControllerGetProduct");
        /// <summary>
        /// Get the USB product ID of an opened controller, if available.
        /// If the product ID isn't available this function returns 0.
        /// </summary>
        public static ushort SDL_GameControllerGetProduct(SDL_GameController gamecontroller) => s_sdl_gameControllerGetProduct(gamecontroller);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate ushort SDL_GameControllerGetProductVersion_t(SDL_GameController gamecontroller);
        private static SDL_GameControllerGetProductVersion_t s_sdl_gameControllerGetProductVersion = LoadFunction<SDL_GameControllerGetProductVersion_t>("SDL_GameControllerGetProductVersion");
        /// <summary>
        /// Get the product version of an opened controller, if available.
        /// If the product version isn't available this function returns 0.
        /// </summary>
        public static ushort SDL_GameControllerGetProductVersion(SDL_GameController gamecontroller) => s_sdl_gameControllerGetProductVersion(gamecontroller);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GameControllerGetAttached_t(SDL_GameController gamecontroller);
        private static SDL_GameControllerGetAttached_t s_sdl_gameControllerGetAttached = LoadFunction<SDL_GameControllerGetAttached_t>("SDL_GameControllerGetAttached");
        /// <summary>
        /// Returns 1 if the controller has been opened and currently connected, or 0 if it has not.
        /// </summary>
        public static int SDL_GameControllerGetAttached(SDL_GameController gamecontroller) => s_sdl_gameControllerGetAttached(gamecontroller);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate SDL_Joystick SDL_GameControllerGetJoystick_t(SDL_GameController gamecontroller);
        private static SDL_GameControllerGetJoystick_t s_sdl_gameControllerGetJoystick = LoadFunction<SDL_GameControllerGetJoystick_t>("SDL_GameControllerGetJoystick");
        /// <summary>
        /// Get the underlying joystick object used by a controller.
        /// </summary>
        public static SDL_Joystick SDL_GameControllerGetJoystick(SDL_GameController gamecontroller) => s_sdl_gameControllerGetJoystick(gamecontroller);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_GameControllerEventState_t(int state);
        private static SDL_GameControllerEventState_t s_sdl_gameControllerEventState = LoadFunction<SDL_GameControllerEventState_t>("SDL_GameControllerEventState");
        /// <summary>
        /// Enable/disable controller event polling.
        /// If controller events are disabled, you must call SDL_GameControllerUpdate()
        /// yourself and check the state of the controller when you want controller
        /// information.
        /// The state can be one of SDL_QUERY, SDL_ENABLE or SDL_IGNORE.
        /// </summary>
        public static int SDL_GameControllerEventState(int state) => s_sdl_gameControllerEventState(state);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SDL_GameControllerUpdate_t();
        private static SDL_GameControllerUpdate_t s_sdl_gameControllerUpdate = LoadFunction<SDL_GameControllerUpdate_t>("SDL_GameControllerUpdate");
        /// <summary>
        /// Update the current state of the open game controllers.
        /// This is called automatically by the event loop if any game controller
        /// events are enabled.
        /// </summary>
        public static void SDL_GameControllerUpdate() => s_sdl_gameControllerUpdate();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate short SDL_GameControllerGetAxis_t(SDL_GameController gamecontroller, SDL_GameControllerAxis axis);
        private static SDL_GameControllerGetAxis_t s_sdl_gameControllerGetAxis = LoadFunction<SDL_GameControllerGetAxis_t>("SDL_GameControllerGetAxis");
        /// <summary>
        /// Get the current state of an axis control on a game controller.
        /// The state is a value ranging from -32768 to 32767 (except for the triggers,
        /// which range from 0 to 32767).
        /// The axis indices start at index 0.
        /// </summary>
        public static short SDL_GameControllerGetAxis(SDL_GameController gamecontroller, SDL_GameControllerAxis axis) => s_sdl_gameControllerGetAxis(gamecontroller, axis);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate byte SDL_GameControllerGetButton_t(SDL_GameController gamecontroller, SDL_GameControllerButton button);
        private static SDL_GameControllerGetButton_t s_sdl_gameControllerGetButton = LoadFunction<SDL_GameControllerGetButton_t>("SDL_GameControllerGetButton");
        /// <summary>
        /// Get the current state of a button on a game controller.
        /// The button indices start at index 0.
        /// </summary>
        public static byte SDL_GameControllerGetButton(SDL_GameController gamecontroller, SDL_GameControllerButton button) => s_sdl_gameControllerGetButton(gamecontroller, button);
    }
}
