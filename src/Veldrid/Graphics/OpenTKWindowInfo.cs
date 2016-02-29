using System;
using System.Collections.Generic;

namespace Veldrid.Graphics
{
    internal class OpenTKWindowInfo : WindowInfo
    {
        private readonly OpenTK.INativeWindow _openTKWindow;

        public OpenTKWindowInfo(OpenTK.INativeWindow info)
        {
            _openTKWindow = info;
        }

        public int Height
        {
            get
            {
                return _openTKWindow.Height;
            }

            set
            {
                _openTKWindow.Height = value;
            }
        }

        public int Width
        {
            get
            {
                return _openTKWindow.Width;
            }

            set
            {
                _openTKWindow.Width = value;
            }
        }
    }
}
