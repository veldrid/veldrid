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

                FrameTimeAverager fta = new FrameTimeAverager(500, 666);

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
            _rc.ClearBuffer();
            _tcr.Render(_rc);
            _ccr.Render(_rc);
            _rc.SwapBuffers();
        }

        private class FrameTimeAverager
        {
            private readonly int _numFramesToAverage;
            private readonly double _timeLimit = 666;

            private double _accumulatedTime = 0;
            private int _frameCount = 0;

            public double CurrentAverageFrameTime { get; private set; }
            public double CurrentAverageFramesPerSecond { get { return 1000 / CurrentAverageFrameTime; } }

            public FrameTimeAverager(int maxFrames, double maxTime)
            {
                _numFramesToAverage = maxFrames;
                _timeLimit = maxTime;
            }

            public void AddTime(double frameTime)
            {
                _accumulatedTime += frameTime;
                _frameCount++;
                if (_frameCount == _numFramesToAverage || _accumulatedTime >= _timeLimit)
                {
                    Average();
                }
            }

            private void Average()
            {
                double total = _accumulatedTime;
                CurrentAverageFrameTime = total / _frameCount;

                _accumulatedTime = 0;
                _frameCount = 0;
            }
        }
    }
}