namespace Veldrid.Platform
{
    public interface OpenTKWindow : Window
    {
        OpenTK.Platform.IWindowInfo OpenTKWindowInfo { get; }
    }
}