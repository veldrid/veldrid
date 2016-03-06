using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;
using System.Linq;
using Veldrid.Graphics;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics.OpenGL;
using Veldrid.Platform;

namespace Veldrid.RenderDemo
{
    public static class Program
    {
        private static TexturedCubeRenderer _tcr;
        private static ColoredCubeRenderer[] _ccrs;
        private static RenderContext _rc;
        private static FrameTimeAverager _fta;
        private static string _apiName;
        private static double _desiredFrameLengthMilliseconds = 1000.0 / 60.0;
        private static OpenTKWindow _window;

        public static void Main()
        {
            try
            {
                _window = new DedicatedThreadWindow();
                _rc = new OpenGLRenderContext(_window);
                _tcr = new TexturedCubeRenderer(_rc);
                _tcr.Position = new System.Numerics.Vector3(-5f, 0, -3);

                _alternateFramebuffer = _rc.ResourceFactory.CreateFramebuffer(_window.Width, _window.Height);
                _altBufferImage = new ImageProcessorTexture(new ImageProcessor.Image(_window.Width, _window.Height));

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

                _apiName = (_rc is OpenGLRenderContext) ? "OpenGL" : "Direct3D";

                _fta = new FrameTimeAverager(666);

                DateTime previousFrameTime = DateTime.UtcNow;
                while (_rc.Window.Exists)
                {
                    double deltaMilliseconds = 0;
                    DateTime currentFrameTime = default(DateTime);
                    while (deltaMilliseconds < _desiredFrameLengthMilliseconds)
                    {
                        currentFrameTime = DateTime.UtcNow;
                        deltaMilliseconds = (currentFrameTime - previousFrameTime).TotalMilliseconds;
                    }

                    previousFrameTime = currentFrameTime;

                    _elapsed += deltaMilliseconds;

                    var snapshot = _window.GetInputSnapshot();
                    Update(snapshot, deltaMilliseconds);
                    Draw(snapshot);
                }

            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                Console.WriteLine("Error: " + e);
                if (_rc is OpenGLRenderContext)
                {
                    Console.WriteLine("GL Error: " + GL.GetError());
                }
            }
        }

        private const double TickDuration = 1000;
        private static double _elapsed = 0;
        private static Framebuffer _alternateFramebuffer;

        private static void Update(InputSnapshot snapshot, double deltaMilliseconds)
        {
            _fta.AddTime(deltaMilliseconds);

            _rc.Window.Title = $"[{_apiName}] " + _fta.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") + _fta.CurrentAverageFrameTime.ToString("#00.00 ms");

            foreach (var ke in snapshot.KeyEvents)
            {
                if (ke.Key == OpenTK.Input.Key.Escape
                    || (ke.Key == OpenTK.Input.Key.F4 && (ke.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt))
                {
                    _window.Close();
                }
                if (ke.Key == OpenTK.Input.Key.F9 && ke.Down)
                {
                    _takeScreenshot = true;
                }

                Console.WriteLine(ke.Key + " is " + (ke.Down ? "down." : "up."));
            }

            foreach (var me in snapshot.MouseEvents)
            {
                Console.WriteLine($"MouseButton {me.MouseButton} is {(me.Down ? "down." : "up.")}");
            }
        }

        private static ImageProcessorTexture _altBufferImage;
        private static bool _takeScreenshot;

        private static void Draw(InputSnapshot input)
        {
            _rc.ClearBuffer();

            if (_takeScreenshot)
            {
                _rc.SetFramebuffer(_alternateFramebuffer);
                _rc.ClearBuffer();
            }

            _tcr.Render(_rc);
            foreach (var ccr in _ccrs)
            {
                ccr.Render(_rc);
            }

            if (_takeScreenshot)
            {
                _takeScreenshot = false;
                _rc.SetDefaultFramebuffer();
                _alternateFramebuffer.ColorTexture.CopyTo(_altBufferImage);
                _altBufferImage.SaveToFile(Environment.TickCount + ".png");
            }

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