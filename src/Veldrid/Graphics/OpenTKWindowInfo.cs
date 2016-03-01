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

        public bool Exists => _openTKWindow.Exists;

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

        public string Title
        {
            get
            {
                return _openTKWindow.Title;
            }

            set
            {
                _openTKWindow.Title = value;
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

        public WindowState WindowState
        {
            get
            {
                switch (_openTKWindow.WindowState)
                {
                    case OpenTK.WindowState.Normal:
                        return WindowState.Normal;
                    case OpenTK.WindowState.Minimized:
                        return WindowState.Minimized;
                    case OpenTK.WindowState.Maximized:
                        return WindowState.Maximized;
                    case OpenTK.WindowState.Fullscreen:
                        return WindowState.FullScreen;
                    default:
                        throw Illegal.Value<WindowState>();
                }
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
