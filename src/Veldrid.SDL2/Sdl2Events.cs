using System.Collections.Generic;

using static Veldrid.Sdl2.Sdl2Native;

namespace Veldrid.Sdl2
{
    public static class Sdl2Events
    {
        private static readonly object s_lock = new();
        private static readonly List<SDLEventHandler> s_processors = new();

        public static void Subscribe(SDLEventHandler processor)
        {
            lock (s_lock)
            {
                s_processors.Add(processor);
            }
        }

        public static void Unsubscribe(SDLEventHandler processor)
        {
            lock (s_lock)
            {
                s_processors.Remove(processor);
            }
        }

        /// <summary>
        /// Pumps the SDL2 event loop, and calls all registered event processors for each event.
        /// </summary>
        public static unsafe void ProcessEvents()
        {
            SDL_Event ev;
            while (SDL_PollEvent(&ev) == 1)
            {
                lock (s_lock)
                {
                    foreach (SDLEventHandler processor in s_processors)
                    {
                        processor(ref ev);
                    }
                }
            }
        }
    }
}
