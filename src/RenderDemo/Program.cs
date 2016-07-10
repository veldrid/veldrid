using System.Collections.Generic;
using System.Runtime.InteropServices;
using Veldrid.Graphics;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;
using Veldrid.Platform;

namespace Veldrid.RenderDemo
{
    public static class Program
    {
        public static void Main()
        {
            bool onWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            OpenTKWindow window;
            RenderContext rc;
            if (onWindows)
            {
                window = new DedicatedThreadWindow();
            }
            else
            {
                window = new SameThreadWindow();
            }

            bool preferOpenGL = Preferences.Instance.PreferOpenGL;
            if (!preferOpenGL && onWindows)
            {
                rc = CreateDefaultD3dRenderContext(window);
            }
            else
            {
                rc = CreateDefaultOpenGLRenderContext(window);
            }

            var options = new List<RenderDemo.RendererOption>();
            var openGLOption = new RenderDemo.RendererOption("OpenGL", () => CreateDefaultOpenGLRenderContext(window));
            var d3dOption = new RenderDemo.RendererOption("Direct3D", () => CreateDefaultD3dRenderContext(window));

            if (onWindows)
            {
                if (rc is OpenGLRenderContext)
                {
                    options.Add(openGLOption);
                    options.Add(d3dOption);
                }
                else
                {
                    options.Add(d3dOption);
                    options.Add(openGLOption);
                }
            }
            else
            {
                options.Add(openGLOption);
            }

            RenderDemo.RunDemo(rc, options.ToArray());
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
