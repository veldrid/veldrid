namespace Veldrid.Graphics
{
    /// <summary>
    /// A type of DeviceTexture representing a texture cube, with six
    /// individual 2D texture faces comprising a 3D cube. Used for
    /// environment mapping, skyboxes, etc.
    /// </summary>
    public interface CubemapTexture : DeviceTexture
    {
    }
}