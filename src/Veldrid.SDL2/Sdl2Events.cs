using System.Collections.Generic;

using static Veldrid.Sdl2.Sdl2Native;

namespace Veldrid.Sdl2
{
    public static class Sdl2Events
    {
        private static readonly object s_lock = new object();
        private static readonly List<Sdl2EventProcessor> s_processors = new List<Sdl2EventProcessor>();
        public static void Subscribe(Sdl2EventProcessor processor)
        {
            lock (s_lock)
            {
                s_processors.Add(processor);
            }
        }

        public static void Unsubscribe(Sdl2EventProcessor processor)
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
            lock (s_lock)
            {
                SDL_Event ev;
                while (SDL_PollEvent(&ev) == 1)
                {
                    foreach (Sdl2EventProcessor processor in s_processors)
                    {
                        processor(ref ev);
                    }
                }
            }
        }
    }

    public delegate void Sdl2EventProcessor(ref SDL_Event ev);
}