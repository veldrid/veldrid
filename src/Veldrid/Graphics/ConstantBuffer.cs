namespace Veldrid.Graphics
{
    public interface ConstantBuffer
    {
        void SetData<T>(ref T data, int dataSizeInBytes) where T : struct;
    }
}
