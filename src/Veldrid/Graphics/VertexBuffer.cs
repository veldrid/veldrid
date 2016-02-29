namespace Veldrid.Graphics
{
    public interface VertexBuffer : RenderStateModifier
    {
        void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor) where T : struct;
    }
}