namespace Veldrid.Graphics
{
    /// <summary>
    /// Represents a texture object owned by the graphics device.
    /// </summary>
    public interface DeviceTexture
    {
        /// <summary>
        /// Copies the DeviceTexture's pixel data into a CPU-side Texture.
        /// </summary>
        /// <param name="texture">The Texture to copy the pixel data into.</param>
        void CopyTo(Texture texture);
    }
}
