namespace Veldrid.Graphics
{
    public interface MeshData
    {
        VertexBuffer CreateVertexBuffer(ResourceFactory factory);
        IndexBuffer CreateIndexBuffer(ResourceFactory factory, out int indexCount);
        BoundingSphere GetBoundingSphere();
        BoundingBox GetBoundingBox();
    }
}
