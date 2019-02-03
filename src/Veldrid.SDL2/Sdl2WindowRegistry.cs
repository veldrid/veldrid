using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static Veldrid.Sdl2.Sdl2Native;

namespace Veldrid.Sdl2
{
    internal static class Sdl2WindowRegistry
    {
        public static readonly object Lock = new object();
        private static readonly Dictionary<uint, Sdl2Window> _eventsByWindowID
            = new Dictionary<uint, Sdl2Window>();
        private static bool _firstInit;

        public static void RegisterWindow(Sdl2Window window)
        {
            lock (Lock)
            {
                _eventsByWindowID.Add(window.WindowID, window);
                if (!_firstInit)
                {
                    _firstInit = true;
                    Sdl2Events.Subscribe(ProcessWindowEvent);
                }
            }
        }

        public static void RemoveWindow(Sdl2Window window)
        {
            lock (Lock)
            {
                _eventsByWindowID.Remove(window.WindowID);
            }
        }

        private static void ProcessWindowEvent(ref SDL_Event ev)
        {
            bool handled = false;
            switch (ev.type)
            {
                case SDL_EventType.Quit:
                case SDL_EventType.Terminating:
                case SDL_EventType.WindowEvent:
                case SDL_EventType.KeyDown:
                case SDL_EventType.KeyUp:
                case SDL_EventType.TextEditing:
                case SDL_EventType.TextInput:
                case SDL_EventType.KeyMapChanged:
                case SDL_EventType.MouseMotion:
                case SDL_EventType.MouseButtonDown:
                case SDL_EventType.MouseButtonUp:
                case SDL_EventType.MouseWheel:
                case SDL_EventType.DropBegin:
                case SDL_EventType.DropComplete:
                case SDL_EventType.DropFile:
                case SDL_EventType.DropText:
                    handled = true;
                    break;
                default:
                    handled = false;
                    break;
            }

            if (handled && _eventsByWindowID.TryGetValue(ev.windowID, out Sdl2Window window))
            {
                window.AddEvent(ev);
            }
        }
    }
}
