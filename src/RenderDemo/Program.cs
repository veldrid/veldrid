using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Veldrid.Graphics;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;
using Veldrid.Graphics.OpenGLES;
using Veldrid.Platform;

namespace Veldrid.RenderDemo
{
    public static class Program
    {
        public static void Main()
        {
            bool onWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            OpenTKWindow window = new SameThreadWindow();
            RenderContext rc;
            bool sdl2OnWindows = false;
#if SDL2_ON_WINDOWS
            sdl2OnWindows = true;
#endif
            bool preferOpenGL = Preferences.Instance.PreferOpenGL || sdl2OnWindows;
            if (!sdl2OnWindows && !preferOpenGL && onWindows)
            {
                rc = CreateDefaultD3dRenderContext(window);
            }
            else
            {
                bool useGLES = sdl2OnWindows;
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
                    if (!sdl2OnWindows)
                    {
                        options.Add(d3dOption);
                    }
                    options.Add(openGLESOption);
                }
                else if (rc is OpenGLESRenderContext)
                {
                    options.Add(openGLESOption);
                    options.Add(openGLOption);
                    if (!sdl2OnWindows)
                    {
                        options.Add(d3dOption);
                    }
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

            RenderDemo.RunDemo(rc, options.ToArray());
        }

        private static OpenGLESRenderContext CreateDefaultOpenGLESRenderContext(OpenTKWindow window)
        {
            return new OpenGLESRenderContext(window);
        }

        private static OpenGLRenderContext CreateDefaultOpenGLRenderContext(OpenTKWindow window)
        {
            bool debugContext = false;
#if DEBUG
            debugContext = Preferences.Instance.AllowOpenGLDebugContexts;
#endif
            return new OpenGLRenderContext(window, debugContext);
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
