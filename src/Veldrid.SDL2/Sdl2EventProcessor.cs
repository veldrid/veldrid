using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static Veldrid.Sdl2.Sdl2Native;

namespace Veldrid.Sdl2
{
    internal static class Sdl2EventProcessor
    {
        public static readonly object Lock = new object();
        private static readonly Dictionary<uint, Sdl2Window> _eventsByWindowID
            = new Dictionary<uint, Sdl2Window>();

        public static unsafe void PumpEvents()
        {
            Debug.Assert(Monitor.IsEntered(Lock));
            SDL_Event ev;
            while (SDL_PollEvent(&ev) == 1)
            {
                uint windowID = ev.windowID;

                if (_eventsByWindowID.TryGetValue(ev.windowID, out Sdl2Window window))
                {
                    window.AddEvent(ev);
                }
            }
        }

        public static void RegisterWindow(Sdl2Window window)
        {
            lock (Lock)
            {
                _eventsByWindowID.Add(window.WindowID, window);
            }
        }

        public static void RemoveWindow(Sdl2Window window)
        {
            lock (Lock)
            {
                _eventsByWindowID.Remove(window.WindowID);
            }
        }
    }
}
