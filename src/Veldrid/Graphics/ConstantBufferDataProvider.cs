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

        event Action DataChanged;
    }

    public interface ConstantBufferDataProvider<T> : ConstantBufferDataProvider
    {
        T Data { get; }
    }

}