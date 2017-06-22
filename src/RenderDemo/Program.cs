using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Veldrid.Graphics;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;
using Veldrid.Graphics.OpenGLES;
using Veldrid.Platform;
using Veldrid.Sdl2;

namespace Veldrid.RenderDemo
{
    public static class Program
    {
        public static void Main()
        {
            bool onWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var window = new Sdl2Window("Veldrid Render Demo", 100, 100, 960, 540, SDL_WindowFlags.Resizable | SDL_WindowFlags.OpenGL, RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
            RenderContext rc;
            bool preferOpenGL = Preferences.Instance.PreferOpenGL;
            if (!preferOpenGL && onWindows)
            {
                rc = CreateDefaultD3dRenderContext(window);
            }
            else
            {
                bool useGLES = false;
                if (useGLES)
                {
                    rc = CreateDefaultOpenGLESRenderContext(window);
                }
                else
                {
                    rc = CreateDefaultOpenGLRenderContext(window);
                }
            }

            var options = new List<RenderDemo.RendererOption>();
            var openGLOption = new RenderDemo.RendererOption("OpenGL", () => CreateDefaultOpenGLRenderContext(window));
            var openGLESOption = new RenderDemo.RendererOption("OpenGL ES", () => CreateDefaultOpenGLESRenderContext(window));
            var d3dOption = new RenderDemo.RendererOption("Direct3D", () => CreateDefaultD3dRenderContext(window));

            if (onWindows)
            {
                if (rc is OpenGLRenderContext)
                {
                    options.Add(openGLOption);
                    options.Add(d3dOption);
                    options.Add(openGLESOption);
                }
                else if (rc is OpenGLESRenderContext)
                {
                    options.Add(openGLESOption);
                    options.Add(openGLOption);
                    options.Add(d3dOption);
                }
                else
                {
                    Debug.Assert(rc is D3DRenderContext);
                    options.Add(d3dOption);
                    options.Add(openGLOption);
                }
            }
            else
            {
                options.Add(openGLOption);
                options.Add(openGLESOption);
            }

            RenderDemo.RunDemo(rc, window, options.ToArray());
        }

        private static OpenGLESRenderContext CreateDefaultOpenGLESRenderContext(Sdl2Window window)
        {
            bool debugContext = false;
#if DEBUG
            debugContext = Preferences.Instance.AllowOpenGLDebugContexts;
#endif
            if (debugContext)
            {
                Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextFlags, (int)SDL_GLContextFlag.Debug);
            }

            IntPtr sdlHandle = window.SdlWindowHandle;
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextProfileMask, (int)SDL_GLProfile.ES);
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMajorVersion, 3);
            Sdl2Native.SDL_GL_SetAttribute(SDL_GLAttribute.ContextMinorVersion, 0);
            IntPtr contextHandle = Sdl2Native.SDL_GL_CreateContext(sdlHandle);
            Sdl2Native.SDL_GL_MakeCurrent(sdlHandle, contextHandle);

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
            var rc = new OpenGLESRenderContext(window, ci);
            if (debugContext)
            {
                rc.EnableDebugCallback(OpenTK.Graphics.ES30.DebugSeverity.DebugSeverityNotification);
            }
            return rc;
        }

        private static OpenGLRenderContext CreateDefaultOpenGLRenderContext(Sdl2Window window)
        {
            bool debugContext = false;
#if DEBUG
            debugContext = Preferences.Instance.AllowOpenGLDebugContexts;
#endif
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
                rc.EnableDebugCallback(OpenTK.Graphics.OpenGL.DebugSeverity.DebugSeverityNotification);
            }
            return rc;
        }

        private static D3DRenderContext CreateDefaultD3dRenderContext(Window window)
        {
            SharpDX.Direct3D11.DeviceCreationFlags flags = SharpDX.Direct3D11.DeviceCreationFlags.None;
#if DEBUG
            if (Preferences.Instance.AllowDirect3DDebugDevice)
            {
                flags |= SharpDX.Direct3D11.DeviceCreationFlags.Debug;
            }
#endif
            return new D3DRenderContext(window, flags);
        }
    }
}
