using System;
using System.Runtime.InteropServices;

namespace Veldrid.Sdl2
{
    /// <summary>
    /// A transparent wrapper over a pointer to a native SDL_Joystick.
    /// </summary>
    public struct SDL_Joystick
    {
        /// <summary>
        /// The native SDL_Joystick pointer.
        /// </summary>
        public readonly IntPtr NativePointer;

        public SDL_Joystick(IntPtr pointer)
        {
            NativePointer = pointer;
        }

        public static implicit operator IntPtr(SDL_Joystick controller) => controller.NativePointer;
        public static implicit operator SDL_Joystick(IntPtr pointer) => new SDL_Joystick(pointer);
    }


    public static unsafe partial class Sdl2Native
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_NumJoysticks_t();
        private static SDL_NumJoysticks_t s_sdl_numJoysticks = LoadFunction<SDL_NumJoysticks_t>("SDL_NumJoysticks");
        /// <summary>
        /// Count the number of joysticks attached to the system right now.
        /// </summary>
        public static int SDL_NumJoysticks() => s_sdl_numJoysticks();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int SDL_JoystickInstanceID_t(SDL_Joystick joystick);
        private static SDL_JoystickInstanceID_t s_sdl_joystickInstanceID = Sdl2Native.LoadFunction<SDL_JoystickInstanceID_t>("SDL_JoystickInstanceID");
        /// <summary>
        /// Returns the instance ID of the specified joystick on success or a negative error code on failure; call SDL_GetError() for more information.
        /// </summary>
        public static int SDL_JoystickInstanceID(SDL_Joystick joystick) => s_sdl_joystickInstanceID(joystick);
    }
}
