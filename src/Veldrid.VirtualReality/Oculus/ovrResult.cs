namespace Veldrid.VirtualReality.Oculus
{
    internal enum ovrResult
    {
        Success = 0,

        Success_NotVisible = 1000,

        Success_BoundaryInvalid = 1001,

        Success_DeviceUnavailable = 1002,

        /******************/
        /* General errors */
        /******************/

        Error_MemoryAllocationFailure = -1000,

        Error_InvalidSession = -1002,

        Error_Timeout = -1003,

        Error_NotInitialized = -1004,

        Error_InvalidParameter = -1005,

        Error_ServiceError = -1006,

        Error_NoHmd = -1007,

        Error_Unsupported = -1009,

        Error_DeviceUnavailable = -1010,

        Error_InvalidHeadsetOrientation = -1011,

        Error_ClientSkippedDestroy = -1012,

        Error_ClientSkippedShutdown = -1013,

        Error_ServiceDeadlockDetected = -1014,

        Error_InvalidOperation = -1015,

        Error_InsufficientArraySize = -1016,

        Error_NoExternalCameraInfo = -1017,

        /*************************************************/
        /* Audio error range, reserved for Audio errors. */
        /*************************************************/

        Error_AudioDeviceNotFound = -2001,

        Error_AudioComError = -2002,

        /**************************/
        /* Initialization errors. */
        /**************************/

        Error_Initialize = -3000,

        Error_LibLoad = -3001,

        Error_LibVersion = -3002,

        Error_ServiceConnection = -3003,

        Error_ServiceVersion = -3004,

        Error_IncompatibleOS = -3005,

        Error_DisplayInit = -3006,

        Error_ServerStart = -3007,

        Error_Reinitialization = -3008,

        Error_MismatchedAdapters = -3009,

        Error_LeakingResources = -3010,

        Error_ClientVersion = -3011,

        Error_OutOfDateOS = -3012,

        Error_OutOfDateGfxDriver = -3013,

        Error_IncompatibleGPU = -3014,

        Error_NoValidVRDisplaySystem = -3015,

        Error_Obsolete = -3016,

        Error_DisabledOrDefaultAdapter = -3017,

        Error_HybridGraphicsNotSupported = -3018,

        Error_DisplayManagerInit = -3019,

        Error_TrackerDriverInit = -3020,

        Error_LibSignCheck = -3021,

        Error_LibPath = -3022,

        Error_LibSymbols = -3023,

        Error_RemoteSession = -3024,

        Error_InitializeVulkan = -3025,

        /********************/
        /* Rendering errors */
        /********************/

        Error_DisplayLost = -6000,

        Error_TextureSwapChainFull = -6001,

        Error_TextureSwapChainInvalid = -6002,

        Error_GraphicsDeviceReset = -6003,

        Error_DisplayRemoved = -6004,

        Error_ContentProtectionNotAvailable = -6005,

        Error_ApplicationInvisible = -6006,

        Error_Disallowed = -6007,

        Error_DisplayPluggedIncorrectly = -6008,

        /****************/
        /* Fatal errors */
        /****************/

        Error_RuntimeException = -7000,

        /**********************/
        /* Calibration errors */
        /**********************/

        Error_NoCalibration = -9000,

        Error_OldVersion = -9001,

        Error_MisformattedBlock = -9002,
    }
}
