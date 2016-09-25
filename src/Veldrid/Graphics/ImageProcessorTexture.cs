using ImageProcessorCore;
using System.IO;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A texture loaded by ImageProcessorCore.
    /// </summary>
    public class ImageProcessorTexture : TextureData
    {
        /// <summary>
        /// The ImageProcessor image.
        /// </summary>
        public Image Image { get; }

        /// <summary>
        /// The raw pixel data, stored in RGBA format, where each element is a 32-bit float (128-bits-per-pixel).
        /// </summary>
        public Color[] Pixels => Image.Pixels;

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width => Image.Width;
        /// <summary>
        /// The height of the iamge.
        /// </summary>
        public int Height => Image.Height;

        /// <summary>
        /// ImageProcessorTexture images are stored in R32_G32_B32_A32_Float format.
        /// </summary>
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8;

        /// <summary>
        /// The size of each pixel; 16 bytes.
        /// </summary>
        public int PixelSizeInBytes => sizeof(byte) * 4;

        /// <summary>
        /// Loads and constructs a new ImageProcessorTexture from the file at the given path.
        /// </summary>
        /// <param name="filePath">The path to the file on disk.</param>
        public ImageProcessorTexture(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                Image = new Image(fs);
            }
        }

        /// <summary>
        /// Constructs an ImageProcessorTexture from the existing ImageProcessor image.
        /// </summary>
        /// <param name="image">The existing image.</param>
        public ImageProcessorTexture(Image image)
        {
            Image = image;
        }
        
        /// <summary>
        /// Saves the image to disk.
        /// </summary>
        /// <param name="path">The target path on disk.</param>
        public void SaveToFile(string path)
        {
            using (FileStream fs = File.OpenWrite(path))
            {
                Image.Save(fs);
            }
        }

        /// <summary>
        /// Constructs a DeviceTexture from this texture.
        /// </summary>
        /// <param name="producer"></param>
        /// <returns>A new <see cref="DeviceTexture2D"/> containing this image's pixel data.</returns>
        public DeviceTexture2D CreateDeviceTexture(DeviceTextureCreator producer)
        {
            return producer.CreateTexture(Pixels, Width, Height, PixelSizeInBytes, Format);
        }

        /// <summary>
        /// Accepts pixel data from the given provider. This will overrite the pixel data in this texture.
        /// </summary>
        /// <param name="pixelDataProvider">The data provider to accept pixel information from.</param>
        public void AcceptPixelData(PixelDataProvider pixelDataProvider)
        {
            pixelDataProvider.SetPixelData(Pixels, Width, Height, PixelSizeInBytes);
        }
    }
}
