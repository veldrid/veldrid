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
        private static ColoredCubeRenderer[] _ccrs;
        private static RenderContext _rc;

        public static void Main()
        {
            try
            {
                _rc = new D3DRenderContext();
                _tcr = new TexturedCubeRenderer(_rc);


                _ccrs = new ColoredCubeRenderer[6 * 6 * 6];
                for (int x = 0; x < 6; x++)
                {
                    for (int y = 0; y < 6; y++)
                    {
                        for (int z = 0; z < 6; z++)
                        {
                            var ccr = new ColoredCubeRenderer(_rc);
                            ccr.Position = new System.Numerics.Vector3((x * 1.35f) - 3, (y * 1.35f) - 6, (z * 1.35f) - 10);
                            _ccrs[x + y * 6 + z * 36] = ccr;
                        }
                    }
                }

                string apiName = (_rc is OpenGLRenderContext) ? "OpenGL" : "Direct3D";

                FrameTimeAverager fta = new FrameTimeAverager(666);

                DateTime previousFrameTime = DateTime.UtcNow;
                while (_rc.WindowInfo.Exists)
                {
                    DateTime currentFrameTime = DateTime.UtcNow;
                    double deltaMilliseconds = (currentFrameTime - previousFrameTime).TotalMilliseconds;
                    previousFrameTime = currentFrameTime;
                    fta.AddTime(deltaMilliseconds);

                    _rc.WindowInfo.Title = $"[{apiName}] " + fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + fta.CurrentAverageFrameTime.ToString("#00.00 ms");
                    Draw();
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
            foreach (var ccr in _ccrs) { ccr.Render(_rc); }
            _rc.SwapBuffers();
        }

        private class FrameTimeAverager
        {
            private readonly double _timeLimit = 666;

            private double _accumulatedTime = 0;
            private int _frameCount = 0;

            public double CurrentAverageFrameTime { get; private set; }
            public double CurrentAverageFramesPerSecond { get { return 1000 / CurrentAverageFrameTime; } }

            public FrameTimeAverager(double maxTimeMilliseconds)
            {
                _timeLimit = maxTimeMilliseconds;
            }

            public void AddTime(double frameTime)
            {
                _accumulatedTime += frameTime;
                _frameCount++;
                if (_accumulatedTime >= _timeLimit)
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