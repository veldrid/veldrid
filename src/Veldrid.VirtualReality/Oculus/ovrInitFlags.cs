using System;

namespace Veldrid.VirtualReality.Oculus
{
    [Flags]
    internal enum ovrInitFlags
    {
        /// <summary>
        /// When a debug library is requested, a slower debugging version of the library will
        /// run which can be used to help solve problems in the library and debug application code.
        /// </summary>
        Debug = 0x00000001,

        /// <summary>
        /// When a version is requested, the LibOVR runtime respects the RequestedMinorVersion
        /// field and verifies that the RequestedMinorVersion is supported. Normally when you
        /// specify this flag you simply use OVR_MINOR_VERSION for ovrInitParams::RequestedMinorVersion,
        /// though you could use a lower version than OVR_MINOR_VERSION to specify previous
        /// version behavior.
        /// </summary>
        RequestVersion = 0x00000004,

        /// <summary>
        /// This client will not be visible in the HMD.
        /// Typically set by diagnostic or debugging utilities.
        /// </summary>
        Invisible = 0x00000010,

        /// <summary>
        /// This client will alternate between VR and 2D rendering.
        /// Typically set by game engine editors and VR-enabled web browsers.
        /// </summary>
        MixedRendering = 0x00000020,

        /// <summary>
        /// This client is aware of ovrSessionStatus focus states (e.g. ovrSessionStatus::HasInputFocus),
        /// and responds to them appropriately (e.g. pauses and stops drawing hands when lacking focus).
        /// </summary>
        FocusAware = 0x00000040,
    }
}
