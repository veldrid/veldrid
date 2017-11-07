namespace Veldrid
{
    /// <summary>
    /// A <see cref="Buffer"/> object which can be used to store index data.
    /// </summary>
    public interface IndexBuffer : Buffer
    {
        /// <summary>
        /// Gets the format of index data stored in this instance.
        /// </summary>
        IndexFormat Format { get; }
    }
}
