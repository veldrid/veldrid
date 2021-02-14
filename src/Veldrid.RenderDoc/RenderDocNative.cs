using NativeLibraryLoader;
using System.Runtime.InteropServices;

namespace Veldrid
{
    /// <summary>
    /// RenderDoc API entry point
    ///
    /// This entry point can be obtained via GetProcAddress/dlsym if RenderDoc is available.
    ///
    /// The name is the same as the typedef - "RENDERDOC_GetAPI"
    ///
    /// This function is not thread safe, and should not be called on multiple threads at once.
    /// Ideally, call this once as early as possible in your application's startup, before doing
    /// any API work, since some configuration functionality etc has to be done also before
    /// initialising any APIs.
    /// </summary>
    /// <param name="version">A single value from the RENDERDOC_Version above.</param>
    /// <param name="outAPIPointers">Will be filled out with a pointer to the corresponding struct of function pointers</param>
    /// <returns>
    ///   1 - if the outAPIPointers has been filled with a pointer to the API struct requested
    ///   0 - if the requested version is not supported or the arguments are invalid.
    /// </returns>
    internal unsafe delegate int pRENDERDOC_GetAPI(RENDERDOC_Version version, void** outAPIPointers);

    internal enum RENDERDOC_Version
    {
        API_Version_1_0_0 = 10000,    // RENDERDOC_API_1_0_0 = 1 00 00
        API_Version_1_0_1 = 10001,    // RENDERDOC_API_1_0_1 = 1 00 01
        API_Version_1_0_2 = 10002,    // RENDERDOC_API_1_0_2 = 1 00 02
        API_Version_1_1_0 = 10100,    // RENDERDOC_API_1_1_0 = 1 01 00
        API_Version_1_1_1 = 10101,    // RENDERDOC_API_1_1_1 = 1 01 01
        API_Version_1_1_2 = 10102,    // RENDERDOC_API_1_1_2 = 1 01 02
        API_Version_1_2_0 = 10200,    // RENDERDOC_API_1_2_0 = 1 02 00
        API_Version_1_3_0 = 10300,    // RENDERDOC_API_1_3_0 = 1 03 00
        API_Version_1_4_0 = 10400,    // RENDERDOC_API_1_4_0 = 1 04 00
        API_Version_1_4_1 = 10401,    // RENDERDOC_API_1_4_1 = 1 04 01
    }

    /// <summary>
    /// RenderDoc can return a higher version than requested if it's backwards compatible,
    /// this function returns the actual version returned. If a parameter is NULL, it will be
    /// ignored and the others will be filled out.
    /// /// </summary>
    internal unsafe delegate void pRENDERDOC_GetAPIVersion(int* major, int* minor, int* patch);

    internal enum RENDERDOC_CaptureOption
    {
        // Allow the application to enable vsync
        //
        // Default - enabled
        //
        // 1 - The application can enable or disable vsync at will
        // 0 - vsync is force disabled
        AllowVSync = 0,

        // Allow the application to enable fullscreen
        //
        // Default - enabled
        //
        // 1 - The application can enable or disable fullscreen at will
        // 0 - fullscreen is force disabled
        AllowFullscreen = 1,

        // Record API debugging events and messages
        //
        // Default - disabled
        //
        // 1 - Enable built-in API debugging features and records the results into
        //     the capture, which is matched up with events on replay
        // 0 - no API debugging is forcibly enabled
        APIValidation = 2,
        DebugDeviceMode = 2,    // deprecated name of this enum

        // Capture CPU callstacks for API events
        //
        // Default - disabled
        //
        // 1 - Enables capturing of callstacks
        // 0 - no callstacks are captured
        CaptureCallstacks = 3,

        // When capturing CPU callstacks, only capture them from drawcalls.
        // This option does nothing without the above option being enabled
        //
        // Default - disabled
        //
        // 1 - Only captures callstacks for drawcall type API events.
        //     Ignored if CaptureCallstacks is disabled
        // 0 - Callstacks, if enabled, are captured for every event.
        CaptureCallstacksOnlyDraws = 4,

        // Specify a delay in seconds to wait for a debugger to attach, after
        // creating or injecting into a process, before continuing to allow it to run.
        //
        // 0 indicates no delay, and the process will run immediately after injection
        //
        // Default - 0 seconds
        //
        DelayForDebugger = 5,

        // Verify buffer access. This includes checking the memory returned by a Map() call to
        // detect any out-of-bounds modification, as well as initialising buffers with undefined contents
        // to a marker value to catch use of uninitialised memory.
        //
        // NOTE: This option is only valid for OpenGL and D3D11. Explicit APIs such as D3D12 and Vulkan do
        // not do the same kind of interception & checking and undefined contents are really undefined.
        //
        // Default - disabled
        //
        // 1 - Verify buffer access
        // 0 - No verification is performed, and overwriting bounds may cause crashes or corruption in
        //     RenderDoc.
        VerifyBufferAccess = 6,

        // The old name for VerifyBufferAccess was VerifyMapWrites.
        // This option now controls the filling of uninitialised buffers with 0xdddddddd which was
        // previously always enabled
        VerifyMapWrites = VerifyBufferAccess,

        // Hooks any system API calls that create child processes, and injects
        // RenderDoc into them recursively with the same options.
        //
        // Default - disabled
        //
        // 1 - Hooks into spawned child processes
        // 0 - Child processes are not hooked by RenderDoc
        HookIntoChildren = 7,

        // By default RenderDoc only includes resources in the final capture necessary
        // for that frame, this allows you to override that behaviour.
        //
        // Default - disabled
        //
        // 1 - all live resources at the time of capture are included in the capture
        //     and available for inspection
        // 0 - only the resources referenced by the captured frame are included
        RefAllResources = 8,

        // **NOTE**: As of RenderDoc v1.1 this option has been deprecated. Setting or
        // getting it will be ignored, to allow compatibility with older versions.
        // In v1.1 the option acts as if it's always enabled.
        //
        // By default RenderDoc skips saving initial states for resources where the
        // previous contents don't appear to be used, assuming that writes before
        // reads indicate previous contents aren't used.
        //
        // Default - disabled
        //
        // 1 - initial contents at the start of each captured frame are saved, even if
        //     they are later overwritten or cleared before being used.
        // 0 - unless a read is detected, initial contents will not be saved and will
        //     appear as black or empty data.
        SaveAllInitials = 9,

        // In APIs that allow for the recording of command lists to be replayed later,
        // RenderDoc may choose to not capture command lists before a frame capture is
        // triggered, to reduce overheads. This means any command lists recorded once
        // and replayed many times will not be available and may cause a failure to
        // capture.
        //
        // NOTE: This is only true for APIs where multithreading is difficult or
        // discouraged. Newer APIs like Vulkan and D3D12 will ignore this option
        // and always capture all command lists since the API is heavily oriented
        // around it and the overheads have been reduced by API design.
        //
        // 1 - All command lists are captured from the start of the application
        // 0 - Command lists are only captured if their recording begins during
        //     the period when a frame capture is in progress.
        CaptureAllCmdLists = 10,

        // Mute API debugging output when the API validation mode option is enabled
        //
        // Default - enabled
        //
        // 1 - Mute any API debug messages from being displayed or passed through
        // 0 - API debugging is displayed as normal
        DebugOutputMute = 11,

        // Option to allow vendor extensions to be used even when they may be
        // incompatible with RenderDoc and cause corrupted replays or crashes.
        //
        // Default - inactive
        //
        // No values are documented, this option should only be used when absolutely
        // necessary as directed by a RenderDoc developer.
        AllowUnsupportedVendorExtensions = 12,

    }

    // Sets an option that controls how RenderDoc behaves on capture.
    //
    // Returns 1 if the option and value are valid
    // Returns 0 if either is invalid and the option is unchanged
    internal unsafe delegate int pRENDERDOC_SetCaptureOptionU32(RENDERDOC_CaptureOption opt, uint val);
    internal unsafe delegate int pRENDERDOC_SetCaptureOptionF32(RENDERDOC_CaptureOption opt, float val);

    // Gets the current value of an option as a uint
    //
    // If the option is invalid, 0xffffffff is returned
    internal unsafe delegate uint pRENDERDOC_GetCaptureOptionU32(RENDERDOC_CaptureOption opt);

    // Gets the current value of an option as a float
    //
    // If the option is invalid, -FLT_MAX is returned
    internal unsafe delegate float pRENDERDOC_GetCaptureOptionF32(RENDERDOC_CaptureOption opt);

    internal enum RENDERDOC_InputButton
    {
        // '0' - '9' matches ASCII values
        eRENDERDOC_Key_0 = 0x30,
        eRENDERDOC_Key_1 = 0x31,
        eRENDERDOC_Key_2 = 0x32,
        eRENDERDOC_Key_3 = 0x33,
        eRENDERDOC_Key_4 = 0x34,
        eRENDERDOC_Key_5 = 0x35,
        eRENDERDOC_Key_6 = 0x36,
        eRENDERDOC_Key_7 = 0x37,
        eRENDERDOC_Key_8 = 0x38,
        eRENDERDOC_Key_9 = 0x39,

        // 'A' - 'Z' matches ASCII values
        eRENDERDOC_Key_A = 0x41,
        eRENDERDOC_Key_B = 0x42,
        eRENDERDOC_Key_C = 0x43,
        eRENDERDOC_Key_D = 0x44,
        eRENDERDOC_Key_E = 0x45,
        eRENDERDOC_Key_F = 0x46,
        eRENDERDOC_Key_G = 0x47,
        eRENDERDOC_Key_H = 0x48,
        eRENDERDOC_Key_I = 0x49,
        eRENDERDOC_Key_J = 0x4A,
        eRENDERDOC_Key_K = 0x4B,
        eRENDERDOC_Key_L = 0x4C,
        eRENDERDOC_Key_M = 0x4D,
        eRENDERDOC_Key_N = 0x4E,
        eRENDERDOC_Key_O = 0x4F,
        eRENDERDOC_Key_P = 0x50,
        eRENDERDOC_Key_Q = 0x51,
        eRENDERDOC_Key_R = 0x52,
        eRENDERDOC_Key_S = 0x53,
        eRENDERDOC_Key_T = 0x54,
        eRENDERDOC_Key_U = 0x55,
        eRENDERDOC_Key_V = 0x56,
        eRENDERDOC_Key_W = 0x57,
        eRENDERDOC_Key_X = 0x58,
        eRENDERDOC_Key_Y = 0x59,
        eRENDERDOC_Key_Z = 0x5A,

        // leave the rest of the ASCII range free
        // in case we want to use it later
        eRENDERDOC_Key_NonPrintable = 0x100,

        eRENDERDOC_Key_Divide,
        eRENDERDOC_Key_Multiply,
        eRENDERDOC_Key_Subtract,
        eRENDERDOC_Key_Plus,

        eRENDERDOC_Key_F1,
        eRENDERDOC_Key_F2,
        eRENDERDOC_Key_F3,
        eRENDERDOC_Key_F4,
        eRENDERDOC_Key_F5,
        eRENDERDOC_Key_F6,
        eRENDERDOC_Key_F7,
        eRENDERDOC_Key_F8,
        eRENDERDOC_Key_F9,
        eRENDERDOC_Key_F10,
        eRENDERDOC_Key_F11,
        eRENDERDOC_Key_F12,

        eRENDERDOC_Key_Home,
        eRENDERDOC_Key_End,
        eRENDERDOC_Key_Insert,
        eRENDERDOC_Key_Delete,
        eRENDERDOC_Key_PageUp,
        eRENDERDOC_Key_PageDn,

        eRENDERDOC_Key_Backspace,
        eRENDERDOC_Key_Tab,
        eRENDERDOC_Key_PrtScrn,
        eRENDERDOC_Key_Pause,

        eRENDERDOC_Key_Max,
    }

    // Sets which key or keys can be used to toggle focus between multiple windows
    //
    // If keys is NULL or num is 0, toggle keys will be disabled
    internal unsafe delegate void pRENDERDOC_SetFocusToggleKeys(RENDERDOC_InputButton* keys, int num);

    // Sets which key or keys can be used to capture the next frame
    //
    // If keys is NULL or num is 0, captures keys will be disabled
    internal unsafe delegate void pRENDERDOC_SetCaptureKeys(RENDERDOC_InputButton* keys, int num);

    internal enum RENDERDOC_OverlayBits : uint
    {
        // This single bit controls whether the overlay is enabled or disabled globally
        Enabled = 0x1,

        // Show the average framerate over several seconds as well as min/max
        FrameRate = 0x2,

        // Show the current frame number
        FrameNumber = 0x4,

        // Show a list of recent captures, and how many captures have been made
        CaptureList = 0x8,

        // Default values for the overlay mask
        Default = (Enabled | FrameRate | FrameNumber | CaptureList),

        // Enable all bits
        All = ~0U,

        // Disable all bits
        None = 0,
    }

    // returns the overlay bits that have been set
    internal unsafe delegate uint pRENDERDOC_GetOverlayBits();
    // sets the overlay bits with an and & or mask
    internal unsafe delegate void pRENDERDOC_MaskOverlayBits(uint And, uint Or);

    // this function will attempt to shut down RenderDoc.
    //
    // Note: that this will only work correctly if done immediately after
    // the dll is loaded, before any API work happens. RenderDoc will remove its
    // injected hooks and shut down. Behaviour is undefined if this is called
    // after any API functions have been called.
    internal unsafe delegate void pRENDERDOC_Shutdown();

    // This function will unload RenderDoc's crash handler.
    //
    // If you use your own crash handler and don't want RenderDoc's handler to
    // intercede, you can call this function to unload it and any unhandled
    // exceptions will pass to the next handler.
    internal unsafe delegate void pRENDERDOC_UnloadCrashHandler();

    // Sets the capture file path template
    //
    // pathtemplate is a UTF-8 string that gives a template for how captures will be named
    // and where they will be saved.
    //
    // Any extension is stripped off the path, and captures are saved in the directory
    // specified, and named with the filename and the frame number appended. If the
    // directory does not exist it will be created, including any parent directories.
    //
    // If pathtemplate is NULL, the template will remain unchanged
    //
    // Example:
    //
    // SetCaptureFilePathTemplate("my_captures/example");
    //
    // Capture #1 -> my_captures/example_frame123.rdc
    // Capture #2 -> my_captures/example_frame456.rdc
    internal unsafe delegate void pRENDERDOC_SetCaptureFilePathTemplate(byte* pathtemplate);

    // returns the current capture path template, see SetCaptureFileTemplate above, as a UTF-8 string
    internal unsafe delegate byte* pRENDERDOC_GetCaptureFilePathTemplate();

    // returns the number of captures that have been made
    internal unsafe delegate uint pRENDERDOC_GetNumCaptures();

    // This function returns the details of a capture, by index. New captures are added
    // to the end of the list.
    //
    // filename will be filled with the absolute path to the capture file, as a UTF-8 string
    // pathlength will be written with the length in bytes of the filename string
    // timestamp will be written with the time of the capture, in seconds since the Unix epoch
    //
    // Any of the parameters can be NULL and they'll be skipped.
    //
    // The function will return 1 if the capture index is valid, or 0 if the index is invalid
    // If the index is invalid, the values will be unchanged
    //
    // Note: when captures are deleted in the UI they will remain in this list, so the
    // capture path may not exist anymore.
    internal unsafe delegate uint pRENDERDOC_GetCapture(uint idx, char* filename, uint* pathlength, ulong* timestamp);

    // Sets the comments associated with a capture file. These comments are displayed in the
    // UI program when opening.
    //
    // filePath should be a path to the capture file to add comments to. If set to NULL or ""
    // the most recent capture file created made will be used instead.
    // comments should be a NULL-terminated UTF-8 string to add as comments.
    //
    // Any existing comments will be overwritten.
    internal unsafe delegate void pRENDERDOC_SetCaptureFileComments(byte* filePath, byte* comments);

    // returns 1 if the RenderDoc UI is connected to this application, 0 otherwise
    internal unsafe delegate uint pRENDERDOC_IsTargetControlConnected();

    // This function will launch the Replay UI associated with the RenderDoc library injected
    // into the running application.
    //
    // if connectTargetControl is 1, the Replay UI will be launched with a command line parameter
    // to connect to this application
    // cmdline is the rest of the command line, as a UTF-8 string. E.g. a captures to open
    // if cmdline is NULL, the command line will be empty.
    //
    // returns the PID of the replay UI if successful, 0 if not successful.
    internal unsafe delegate uint pRENDERDOC_LaunchReplayUI(uint connectTargetControl, byte* cmdline);

    //////////////////////////////////////////////////////////////////////////
    // Capturing functions
    //

    // This sets the RenderDoc in-app overlay in the API/window pair as 'active' and it will
    // respond to keypresses. Neither parameter can be NULL
    internal unsafe delegate void pRENDERDOC_SetActiveWindow(void* device, void* wndHandle);

    // capture the next frame on whichever window and API is currently considered active
    internal unsafe delegate void pRENDERDOC_TriggerCapture();

    // capture the next N frames on whichever window and API is currently considered active
    internal unsafe delegate void pRENDERDOC_TriggerMultiFrameCapture(uint numFrames);

    // When choosing either a device pointer or a window handle to capture, you can pass NULL.
    // Passing NULL specifies a 'wildcard' match against anything. This allows you to specify
    // any API rendering to a specific window, or a specific API instance rendering to any window,
    // or in the simplest case of one window and one API, you can just pass NULL for both.
    //
    // In either case, if there are two or more possible matching (device,window) pairs it
    // is undefined which one will be captured.
    //
    // Note: for headless rendering you can pass NULL for the window handle and either specify
    // a device pointer or leave it NULL as above.

    // Immediately starts capturing API calls on the specified device pointer and window handle.
    //
    // If there is no matching thing to capture (e.g. no supported API has been initialised),
    // this will do nothing.
    //
    // The results are undefined (including crashes) if two captures are started overlapping,
    // even on separate devices and/oror windows.
    internal unsafe delegate void pRENDERDOC_StartFrameCapture(void* device, void* wndHandle);

    // Returns whether or not a frame capture is currently ongoing anywhere.
    //
    // This will return 1 if a capture is ongoing, and 0 if there is no capture running
    internal unsafe delegate uint pRENDERDOC_IsFrameCapturing();

    // Ends capturing immediately.
    //
    // This will return 1 if the capture succeeded, and 0 if there was an error capturing.
    internal unsafe delegate uint pRENDERDOC_EndFrameCapture(void* device, void* wndHandle);

    // Ends capturing immediately and discard any data stored without saving to disk.
    //
    // This will return 1 if the capture was discarded, and 0 if there was an error or no capture
    // was in progress
    internal unsafe delegate uint pRENDERDOC_DiscardFrameCapture(void* device, void* wndHandle);

    //////////////////////////////////////////////////////////////////////////////////////////////////
    // RenderDoc API versions
    //

    [StructLayout(LayoutKind.Sequential)]
    internal struct RENDERDOC_API_1_4_0
    {
        public pRENDERDOC_GetAPIVersion GetAPIVersion;

        public pRENDERDOC_SetCaptureOptionU32 SetCaptureOptionU32;
        public pRENDERDOC_SetCaptureOptionF32 SetCaptureOptionF32;

        public pRENDERDOC_GetCaptureOptionU32 GetCaptureOptionU32;
        public pRENDERDOC_GetCaptureOptionF32 GetCaptureOptionF32;

        public pRENDERDOC_SetFocusToggleKeys SetFocusToggleKeys;
        public pRENDERDOC_SetCaptureKeys SetCaptureKeys;

        public pRENDERDOC_GetOverlayBits GetOverlayBits;
        public pRENDERDOC_MaskOverlayBits MaskOverlayBits;

        public pRENDERDOC_Shutdown Shutdown;
        public pRENDERDOC_UnloadCrashHandler UnloadCrashHandler;

        public pRENDERDOC_SetCaptureFilePathTemplate SetCaptureFilePathTemplate;
        public pRENDERDOC_GetCaptureFilePathTemplate GetCaptureFilePathTemplate;

        public pRENDERDOC_GetNumCaptures GetNumCaptures;
        public pRENDERDOC_GetCapture GetCapture;

        public pRENDERDOC_TriggerCapture TriggerCapture;

        public pRENDERDOC_IsTargetControlConnected IsTargetControlConnected;
        public pRENDERDOC_LaunchReplayUI LaunchReplayUI;

        public pRENDERDOC_SetActiveWindow SetActiveWindow;

        public pRENDERDOC_StartFrameCapture StartFrameCapture;
        public pRENDERDOC_IsFrameCapturing IsFrameCapturing;
        public pRENDERDOC_EndFrameCapture EndFrameCapture;

        // new function in 1.1.0
        public pRENDERDOC_TriggerMultiFrameCapture TriggerMultiFrameCapture;

        // new function in 1.2.0
        public pRENDERDOC_SetCaptureFileComments SetCaptureFileComments;
        // new function in 1.4.0
        public pRENDERDOC_DiscardFrameCapture DiscardFrameCapture;
    }
}
