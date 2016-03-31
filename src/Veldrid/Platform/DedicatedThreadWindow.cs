using OpenTK;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Veldrid.Platform
{
    public class DedicatedThreadWindow : OpenTKWindowBase, Window
    {
        private SimpleInputSnapshot _snapshotBackBuffer = new SimpleInputSnapshot();
        private bool _shouldClose;

        public double PollIntervalInMs { get; set; } = 1000.0 / 120.0;
        public bool LimitPollRate { get; set; }

        protected override void ConstructDefaultWindow()
        {
            using (ManualResetEvent mre = new ManualResetEvent(false))
            {
                Task.Factory.StartNew(WindowOwnerRoutine, mre, TaskCreationOptions.LongRunning);
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
            ManualResetEvent mre = (ManualResetEvent)state;
            base.ConstructDefaultWindow();
            mre.Set();

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
    }
}
