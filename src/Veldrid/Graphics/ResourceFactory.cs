namespace Veldrid.Graphics
{
    public abstract class ResourceFactory
    {
        public abstract VertexBuffer CreateVertexBuffer();

        public abstract IndexBuffer CreateIndexBuffer();

        public abstract Material CreateMaterial(
            string vertexShaderName,
            string pixelShaderName,
            MaterialVertexInput vertexInputs,
            MaterialGlobalInputs globalInputs,
            MaterialTextureInputs textureInputs);

        public abstract ConstantBuffer CreateConstantBuffer();
    }
}
