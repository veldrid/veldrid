using OpenTK;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Veldrid.Platform
{
    /// <summmary>A window which performs message processing on a dedicated thread.
    /// This is desirable on Windows to allow the window to be moved and resized without
    /// interrupting message processing.</summmary>
    public class DedicatedThreadWindow : OpenTKWindowBase, Window
    {
        private SimpleInputSnapshot _snapshotBackBuffer = new SimpleInputSnapshot();
        private bool _shouldClose;

        /// <summary>Gets or sets the polling interval, in milliseconds, at which to poll and process window events.
        /// This property only has effect when LimitPollRate is set to True.</summary>
        public double PollIntervalInMs { get; set; } = 1000.0 / 120.0;

        /// <summary>Gets or sets whether to limit message polling and processing.</summary>
        public bool LimitPollRate { get; set; }

        public DedicatedThreadWindow() { }

        public DedicatedThreadWindow(int width, int height, WindowState initialState)
            : base(width, height, initialState) { }

        protected override void ConstructDefaultWindow(int width, int height, WindowState state)
        {
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                WindowParams wp = new WindowParams()
                {
                    Width = width,
                    Height = height,
                    WindowState = state,
                    ResetEvent = mre
                };

                Task.Factory.StartNew(WindowOwnerRoutine, wp, TaskCreationOptions.LongRunning);
                mre.WaitOne();
            }
        }

        public override void Close()
        {
            _shouldClose = true;
        }

        protected override SimpleInputSnapshot GetAvailableSnapshot()
        {
            _snapshotBackBuffer.Clear();
            SimpleInputSnapshot snapshot = Interlocked.Exchange(ref CurrentSnapshot, _snapshotBackBuffer);
            _snapshotBackBuffer = snapshot;
            return snapshot;
        }

        private void WindowOwnerRoutine(object state)
        {
            WindowParams wp = (WindowParams)state;
            base.ConstructDefaultWindow(wp.Width, wp.Height, wp.WindowState);
            wp.ResetEvent.Set();

            double previousPollTimeMs = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (NativeWindow.Exists)
            {
                if (_shouldClose)
                {
                    NativeWindow.Close();
                }

                double currentTick = sw.ElapsedTicks;
                double currentTimeMs = sw.ElapsedTicks * (1000.0 / Stopwatch.Frequency);
                if (LimitPollRate && currentTimeMs - previousPollTimeMs < PollIntervalInMs)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    previousPollTimeMs = currentTimeMs;
                    NativeWindow.ProcessEvents();
                }
            }
        }

        private class WindowParams
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public WindowState WindowState { get; set; }
            public ManualResetEvent ResetEvent { get; set; }
        }
    }
}
