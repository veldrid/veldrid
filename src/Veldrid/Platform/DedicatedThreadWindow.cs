using OpenTK;
using System.Threading;
using System.Threading.Tasks;

namespace Veldrid.Platform
{
    public class DedicatedThreadWindow : OpenTKWindowBase, Window
    {
        private SimpleInputSnapshot _snapshotBackBuffer = new SimpleInputSnapshot();
        private bool _shouldClose;

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

        public override InputSnapshot GetInputSnapshot()
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

            while (NativeWindow.Exists)
            {
                if (_shouldClose)
                {
                    NativeWindow.Close();
                }

                NativeWindow.ProcessEvents();
            }
        }
    }
}
