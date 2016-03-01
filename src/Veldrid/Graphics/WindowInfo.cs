namespace Veldrid.Graphics
{
    public interface WindowInfo
    {
        int Width { get; set; }
        int Height { get; set; }
        string Title { get; set; }
        WindowState WindowState { get; set; }
        bool Exists { get; }
    }
}
