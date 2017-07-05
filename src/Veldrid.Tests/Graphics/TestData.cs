using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;
using Veldrid.Platform;
using Veldrid.Sdl2;

namespace Veldrid.Graphics
{
    public static class TestData
    {
        public static IEnumerable<RenderContext> RenderContexts()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                yield return new D3DRenderContext(CreateTestWindow());
                yield return CreateDefaultOpenGLRenderContext(CreateTestWindow());
            }
            else
            {
                yield return CreateDefaultOpenGLRenderContext(CreateTestWindow());
            }
        }

        public static IEnumerable<object[]> RenderContextsTestData()
        {
            foreach (var item in RenderContexts())
            {
                yield return Array(item);
            }
        }


        public static Sdl2Window CreateTestWindow()
        {
            return new Sdl2Window("Test_Window", 0, 0, 1, 1, SDL_WindowFlags.OpenGL | SDL_WindowFlags.Hidden, false);
        }

        public static OpenGLRenderContext CreateDefaultOpenGLRenderContext(Sdl2Window window)
        {
            bool debugContext = false;
            IntPtr sdlHandle = window.SdlWindowHandle;
            if (debugContext)
            {
                Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextFlags, (int)SDL_GLContextFlag.Debug);
            }

            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextProfileMask, (int)SDL_GLProfile.Core);
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMajorVersion, 4);
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMinorVersion, 0);

            IntPtr contextHandle = Sdl2Native.SDL_GL_CreateContext(sdlHandle);
            if (contextHandle == IntPtr.Zero)
            {
                unsafe
                {
                    byte* error = Sdl2Native.SDL_GetError();
                    string errorString = Utilities.GetString(error);
                    throw new InvalidOperationException("Unable to create GL Context: " + errorString);
                }
            }

            Sdl2Native.SDL_GL_MakeCurrent(sdlHandle, contextHandle);
            OpenGLPlatformContextInfo ci = new OpenGLPlatformContextInfo(
                contextHandle,
                Sdl2Native.SDL_GL_GetProcAddress,
                Sdl2Native.SDL_GL_GetCurrentContext,
                () => Sdl2Native.SDL_GL_SwapWindow(sdlHandle));
            var rc = new OpenGLRenderContext(window, ci);
            if (debugContext)
            {
                // Slows things down significantly -- Only use when debugging something specific.
                rc.EnableDebugCallback(OpenTK.Graphics.OpenGL.DebugSeverity.DebugSeverityNotification);
            }
            return rc;
        }

        internal static IEnumerable<object> DataValueArrays()
        {
            int[] starts = { 0, 1, 10 };
            int[] lengths = { 1, 10, 10000 };
            foreach (int start in starts)
            {
                foreach (int length in lengths)
                {
                    yield return Enumerable.Range(start, start + length).ToArray();
                    yield return Enumerable.Range(start, start + length).Select(i => (uint)i).ToArray();
                    yield return Enumerable.Range(start, start + length).Select(i => (float)i).ToArray();
                    yield return Enumerable.Range(start, start + length).Select(i => (ushort)i).ToArray();
                }
            }
        }

        internal static IEnumerable<int> IntRange(int first, int count, int interval)
        {
            int ret = first;
            yield return ret;
            for (int i = 0; i < count; i++)
            {
                ret += interval;
                yield return ret;
            }
        }

        public static object[] Array(params object[] items)
        {
            return items;
        }

        public static T[] Array<T>(params T[] items)
        {
            return items;
        }
    }
}
