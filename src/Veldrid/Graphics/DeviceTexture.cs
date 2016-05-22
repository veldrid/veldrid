namespace Veldrid.Graphics
{
    /// <summary>
    /// Represents a texture object owned by the graphics device.
    /// </summary>
    public interface DeviceTexture
    {
        /// <summary>
        /// The width of the texture. For cubemap textures, returns the width of each individual face.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of the texture. For cubemap textures, returns the height of each individual face.
        /// </summary>
        int Height { get; }
    }

    public interface DeviceTexture2D : DeviceTexture
    {
        /// <summary>
        /// Copies the DeviceTexture's pixel data into a CPU-side Texture.
        /// </summary>
        /// <param name="textureData">The TextureData to copy the pixel data into.</param>
        void CopyTo(TextureData textureData);
    }
}
