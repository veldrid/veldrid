using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Veldrid.Graphics;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;

namespace Veldrid.RenderDemo
{
    public static class Program
    {
        private static TexturedCubeRenderer _tcr;
        private static ColoredCubeRenderer _ccr;
        private static RenderContext _rc;

        public static void Main()
        {
            try
            {
                var window = new OpenTK.NativeWindow();
                window.Visible = true;
                window.X = 100;
                window.Y = 100;

                _rc = new D3DRenderContext(window);
                _tcr = new TexturedCubeRenderer(_rc);
                _ccr = new ColoredCubeRenderer(_rc);

                _ccr.Position += System.Numerics.Vector3.UnitX * 3f;

                FrameTimeAverager fta = new FrameTimeAverager(40);

                DateTime previousFrameTime = DateTime.UtcNow;
                while (window.Exists)
                {
                    DateTime currentFrameTime = DateTime.UtcNow;
                    double deltaMilliseconds = (currentFrameTime - previousFrameTime).TotalMilliseconds;
                    previousFrameTime = currentFrameTime;
                    fta.AddTime(deltaMilliseconds);

                    window.Title = fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + fta.CurrentAverageFrameTime.ToString("#00.00 ms");
                    Draw();
                    window.ProcessEvents();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                if (_rc is OpenGLRenderContext)
                {
                    Console.WriteLine("GL Error: " + GL.GetError());
                }
            }
        }

        private static void Draw()
        {
            _rc.BeginFrame();
            _tcr.Render(_rc);
            _ccr.Render(_rc);
            _rc.SwapBuffers();
        }

        private class FrameTimeAverager
        {
            private readonly Stack<double> _frameTimes;
            private readonly int _numFramesToAverage;

            public double CurrentAverageFrameTime { get; private set; }
            public double CurrentAverageFramesPerSecond { get { return 1000 / CurrentAverageFrameTime; } }

            public FrameTimeAverager(int numFramesToAverage)
            {
                _numFramesToAverage = numFramesToAverage;
                _frameTimes = new Stack<double>(numFramesToAverage);
            }

            public void AddTime(double frameTime)
            {
                _frameTimes.Push(frameTime);
                if (_frameTimes.Count == _numFramesToAverage)
                {
                    AverageStack();
                }
            }

            private void AverageStack()
            {
                double total = 0.0;
                for (int i = 0; i < _numFramesToAverage; i++)
                {
                    total += _frameTimes.Pop();
                }

                CurrentAverageFrameTime = total / _numFramesToAverage;
            }
        }
    }
}