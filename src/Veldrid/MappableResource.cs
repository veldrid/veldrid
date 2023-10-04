namespace Veldrid
{
    /// <summary>
    /// A marker interface designating a device resource which can be mapped into CPU-visible memory with
    /// <see cref="GraphicsDevice.Map(MappableResource, uint, uint, MapMode, uint)"/>
    /// </summary>
    public interface MappableResource
    {
        /// <summary>
        /// Gets the total size of a given subresource.
        /// </summary>
        /// <param name="subresource">The subresource.</param>
        /// <returns>The total size of the subresource in bytes.</returns>
        uint GetSizeInBytes(uint subresource);
    }
}
