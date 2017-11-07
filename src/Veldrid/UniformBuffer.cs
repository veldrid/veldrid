namespace Veldrid
{
    /// <summary>
    /// A bindable device resources storing arbitrary graphics data. When bound to a <see cref="ResourceSet"/>, gives a shader
    /// access to the data stored in the Buffer.
    /// </summary>
    public interface UniformBuffer : Buffer, BindableResource
    {
    }
}
