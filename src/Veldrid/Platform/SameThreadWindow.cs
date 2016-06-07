﻿using OpenTK;

namespace Veldrid.Platform
{
    ///
    /// <summary>A window that performs OS message processing on the same thread which calls GetAvailableSnapshot.</summary>
    ///
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
