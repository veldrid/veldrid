using System;
using static Veldrid.Android.EGLNative;
using static Veldrid.Android.AndroidRuntime;
using Veldrid.OpenGL;

namespace Veldrid.Android
{
    public static unsafe class AndroidStartup
    {
        /// <summary>
        /// Creates an OpenGLES <see cref="GraphicsDevice"/> for the given Android Surface.
        /// This should only be used after the "surface created" notification has been issued.
        /// </summary>
        /// <param name="options">The options to use when creating the device.</param>
        /// <param name="surfaceHandle">The Android Surface's handle.</param>
        /// <param name="jniHandle">the JNI (Java Native Interface) Environment handle.</param>
        /// <param name="width">The width of the surface.</param>
        /// <param name="height">The height of the surface.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the OpenGL ES API, and rendering to the given Android Surface.
        /// </returns>
        public static GraphicsDevice CreateOpenGLESGraphicsDevice(
            GraphicsDeviceOptions options,
            IntPtr surfaceHandle,
            IntPtr jniHandle,
            uint width,
            uint height)
        {
            IntPtr aNativeWindow = ANativeWindow_fromSurface(jniHandle, surfaceHandle);
            return CreateOpenGLESGraphicsDevice(options, aNativeWindow, width, height);
        }

        /// <summary>
        /// Creates an OpenGLES <see cref="GraphicsDevice"/> for the given ANativeWindow.
        /// This should only be used after the "surface created" notification has been issued for the underlying Surface.
        /// </summary>
        /// <param name="options">The options to use when creating the device.</param>
        /// <param name="aNativeWindow">The ANativeWindow to render into.</param>
        /// <param name="width">The width of the surface.</param>
        /// <param name="height">The height of the surface.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the OpenGL ES API, and rendering to the given ANativeWindow.
        /// </returns>
        public static GraphicsDevice CreateOpenGLESGraphicsDevice(
            GraphicsDeviceOptions options,
            IntPtr aNativeWindow,
            uint width,
            uint height)
        {
            IntPtr display = eglGetDisplay(0);
            if (display == IntPtr.Zero)
            {
                throw new VeldridException($"Failed to get the default Android EGLDisplay: {eglGetError()}");
            }

            int major, minor;
            if (eglInitialize(display, &major, &minor) == 0)
            {
                throw new VeldridException($"Failed to initialize EGL: {eglGetError()}");
            }

            int[] attribs =
            {
                EGL_RED_SIZE, 8,
                EGL_GREEN_SIZE, 8,
                EGL_BLUE_SIZE, 8,
                EGL_ALPHA_SIZE, 8,
                EGL_DEPTH_SIZE,
                options.SwapchainDepthFormat != null
                    ? GetDepthBits(options.SwapchainDepthFormat.Value)
                    : 0,
                EGL_SURFACE_TYPE, EGL_WINDOW_BIT,
                EGL_RENDERABLE_TYPE, EGL_OPENGL_ES3_BIT,
                EGL_NONE,
            };

            IntPtr* configs = stackalloc IntPtr[50];

            fixed (int* attribsPtr = attribs)
            {
                int num_config;
                if (eglChooseConfig(display, attribsPtr, configs, 50, &num_config) == 0)
                {
                    throw new VeldridException($"Failed to select a valid EGLConfig: {eglGetError()}");
                }
            }

            IntPtr bestConfig = configs[0];

            int format;
            if (eglGetConfigAttrib(display, bestConfig, EGL_NATIVE_VISUAL_ID, &format) == 0)
            {
                throw new VeldridException($"Failed to get the EGLConfig's format: {eglGetError()}");
            }

            ANativeWindow_setBuffersGeometry(aNativeWindow, 0, 0, format);

            IntPtr eglWindowSurface = eglCreateWindowSurface(display, bestConfig, aNativeWindow, null);
            if (eglWindowSurface == IntPtr.Zero)
            {
                throw new VeldridException(
                    $"Failed to create an EGL surface from the Android native window: {eglGetError()}");
            }

            int* contextAttribs = stackalloc int[3];
            contextAttribs[0] = EGL_CONTEXT_CLIENT_VERSION;
            contextAttribs[1] = 2;
            contextAttribs[2] = EGL_NONE;
            IntPtr context = eglCreateContext(display, bestConfig, IntPtr.Zero, contextAttribs);
            if (context == IntPtr.Zero)
            {
                throw new VeldridException($"Failed to create an EGLContext: " + eglGetError());
            }

            Action<IntPtr> makeCurrent = ctx =>
            {
                if (eglMakeCurrent(display, eglWindowSurface, eglWindowSurface, ctx) == 0)
                {
                    throw new VeldridException($"Failed to make the EGLContext {ctx} current: {eglGetError()}");
                }
            };

            // The new context should be made current immediately -- Veldrid's initialization expects it.
            makeCurrent(context);

            Action clearContext = () =>
            {
                if (eglMakeCurrent(display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) == 0)
                {
                    throw new VeldridException("Failed to clear the current EGLContext: " + eglGetError());
                }
            };

            Action swapBuffers = () =>
            {
                if (eglSwapBuffers(display, eglWindowSurface) == 0)
                {
                    throw new VeldridException("Failed to swap buffers: " + eglGetError());
                }
            };

            Action<bool> setSync = vsync =>
            {
                if (eglSwapInterval(display, vsync ? 1 : 0) == 0)
                {
                    throw new VeldridException($"Failed to set the swap interval: " + eglGetError());
                }
            };

            // Set the desired initial state.
            setSync(options.SyncToVerticalBlank);

            Action<IntPtr> destroyContext = ctx =>
            {
                if (eglDestroyContext(display, ctx) == 0)
                {
                    throw new VeldridException($"Failed to destroy EGLContext {ctx}: {eglGetError()}");
                }
            };

            OpenGLPlatformInfo info = new OpenGLPlatformInfo(
                context,
                eglGetProcAddress,
                makeCurrent,
                eglGetCurrentContext,
                clearContext,
                destroyContext,
                swapBuffers,
                setSync);

            return GraphicsDevice.CreateOpenGL(options, info, width, height);
        }

        private static int GetDepthBits(PixelFormat value)
        {
            switch (value)
            {
                case PixelFormat.R16_UNorm:
                    return 16;
                case PixelFormat.R32_Float:
                    return 32;
                default:
                    throw new VeldridException($"Unsupported depth format: {value}");
            }
        }
    }
}
