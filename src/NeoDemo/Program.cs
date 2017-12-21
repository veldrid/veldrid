namespace Veldrid.NeoDemo
{
    class Program
    {
        unsafe static void Main(string[] args)
        {
            Sdl2.SDL_version version;
            Sdl2.Sdl2Native.SDL_GetVersion(&version);
            new NeoDemo().Run();
        }
    }
}
