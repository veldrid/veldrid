using OpenTK;

namespace Veldrid.Platform
{
    public class SameThreadWindow : OpenTKWindowBase
    {
        public override void Close()
        {
            NativeWindow.Close();
        }

        protected override SimpleInputSnapshot GetAvailableSnapshot()
        {
            CurrentSnapshot.Clear();
            NativeWindow.ProcessEvents();
            return CurrentSnapshot;
        }
    }
}
