using Snake;

namespace Veldrid.SampleGallery
{
    internal static class Program
    {
        private static void Main()
        {
            WebGalleryDriver driver = new WebGalleryDriver();
            Gallery gallery = new Gallery(driver);
            gallery.LoadExample(new SnakeExample());
            driver.Run();
        }
    }
}
