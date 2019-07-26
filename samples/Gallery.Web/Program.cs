using Snake;

namespace Veldrid.SampleGallery
{
    internal static class Program
    {
        private static void Main()
        {
            WebGalleryDriver driver = new WebGalleryDriver();
            Gallery gallery = new Gallery(driver);
            gallery.RegisterExample("Simple Mesh Render", () => new SimpleMeshRender());
            gallery.RegisterExample("Snake", () => new SnakeExample());
            gallery.LoadExample("Simple Mesh Render");
            driver.Run();
        }
    }
}
