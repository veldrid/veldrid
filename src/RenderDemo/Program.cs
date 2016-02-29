using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Numerics;
using Veldrid.Graphics;
using Veldrid.Graphics.OpenGL;

namespace Veldrid.RenderDemo
{
    public static class Program
    {
        private static TexturedCubeRenderer _tcr;
        private static OpenGLRenderContext _rc;

        public static void Main()
        {
            try
            {
                var window = new OpenTK.NativeWindow();
                window.Visible = true;
                window.X = 100;
                window.Y = 100;

                _rc = new OpenGLRenderContext(window);
                _tcr = new TexturedCubeRenderer(_rc);

                while (window.Exists)
                {
                    window.Title = (window.Focused ? "[F] " : "[N] ") + Environment.TickCount.ToString();
                    Draw();
                    window.ProcessEvents();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine("GL Error: " + GL.GetError());
            }
        }

        private static void Draw()
        {
            _rc.BeginFrame();
            _tcr.Render(_rc);
            _rc.SwapBuffers();
        }
    }
}