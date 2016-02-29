namespace Veldrid.Graphics
{
    public abstract class ResourceFactory
    {
        public abstract VertexBuffer CreateVertexBuffer(int sizeInBytes);

        public abstract IndexBuffer CreateIndexBuffer(int sizeInBytes);

        public abstract Material CreateMaterial(
            string vertexShaderName,
            string pixelShaderName,
            MaterialVertexInput vertexInputs,
            MaterialGlobalInputs globalInputs,
            MaterialTextureInputs textureInputs);

        public abstract ConstantBuffer CreateConstantBuffer(int sizeInBytes);
    }
}
