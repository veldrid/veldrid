using OpenTK;

namespace Veldrid.Platform
{
    public class SameThreadWindow : OpenTKWindowBase
    {
        public override void Close()
        {
            NativeWindow.Close();
        }

        public override InputSnapshot GetInputSnapshot()
        {
            CurrentSnapshot.Clear();
            NativeWindow.ProcessEvents();
            return CurrentSnapshot;
        }
    }
}
