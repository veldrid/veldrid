namespace Veldrid.Graphics
{
    public interface ConstantBuffer
    {
        void SetData<T>(T data, int dataSizeInBytes) where T : struct;
    }
}
