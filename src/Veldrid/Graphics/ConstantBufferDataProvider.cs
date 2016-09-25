using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// Represents a type which provides data to a ConstantBuffer.
    /// </summary>
    public interface ConstantBufferDataProvider
    {
        /// <summary>
        /// The size of data provided.
        /// </summary>
        int DataSizeInBytes { get; }

        /// <summary>
        /// Provides data to the given ConstantBuffer.
        /// </summary>
        /// <param name="buffer"></param>
        void SetData(ConstantBuffer buffer);

        /// <summary>
        /// Provides a notification when this provider's data changes.
        /// </summary>
        event Action DataChanged;
    }

    public interface ConstantBufferDataProvider<T> : ConstantBufferDataProvider
    {
        /// <summary>
        /// Gets the data provided by this <see cref="ConstantBufferDataProvider{T}"/> directly.
        /// </summary>
        T Data { get; }
    }
}