using System;

namespace Veldrid.Graphics
{
    public abstract class ResourceFactory : DeviceTextureCreator
    {
        public abstract VertexBuffer CreateVertexBuffer(int sizeInBytes, bool isDynamic);

        public abstract IndexBuffer CreateIndexBuffer(int sizeInBytes);

        public abstract Material CreateMaterial(
            string vertexShaderName,
            string pixelShaderName,
            MaterialVertexInput vertexInputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            MaterialTextureInputs textureInputs);

        public abstract ConstantBuffer CreateConstantBuffer(int sizeInBytes);

        public abstract Framebuffer CreateFramebuffer(int width, int height);

        public abstract DeviceTexture CreateTexture<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format) where T : struct;

        public abstract DeviceTexture CreateTexture(IntPtr pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format);
    }
}
