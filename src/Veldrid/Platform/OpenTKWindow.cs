namespace Veldrid.Platform
{
    public interface OpenTKWindow : Window
    {
        OpenTK.NativeWindow NativeWindow { get; }
        OpenTK.Platform.IWindowInfo OpenTKWindowInfo { get; }
    }
}