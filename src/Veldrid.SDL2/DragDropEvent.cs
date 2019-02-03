namespace Veldrid.Sdl2
{
    public struct DragDropEvent
    {
        public string File { get; }

        public DragDropEvent(string file)
        {
            File = file;
        }
    }
}