using System;

namespace Veldrid.Graphics
{
    public abstract class ResourceFactory : DeviceTextureCreator
    {
        public abstract VertexBuffer CreateVertexBuffer(int sizeInBytes, bool isDynamic);

        public abstract IndexBuffer CreateIndexBuffer(int sizeInBytes, bool isDynamic);

        public abstract Material CreateMaterial(
            string vertexShaderName,
            string pixelShaderName,
            MaterialVertexInput vertexInputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            MaterialTextureInputs textureInputs);

        public abstract ConstantBuffer CreateConstantBuffer(int sizeInBytes);

        public abstract Framebuffer CreateFramebuffer();

        public abstract Framebuffer CreateFramebuffer(int width, int height);

        public abstract DeviceTexture CreateTexture<T>(T[] pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format) where T : struct;

        public abstract DeviceTexture CreateTexture(IntPtr pixelData, int width, int height, int pixelSizeInBytes, PixelFormat format);

        public abstract DeviceTexture CreateDepthTexture(int width, int height, int pixelSizeInBytes, PixelFormat format);

        public abstract BlendState CreateCustomBlendState(
            bool isBlendEnabled,
            Blend srcBlend, Blend destBlend, BlendFunction blendFunc);


        public abstract BlendState CreateCustomBlendState(
            bool isBlendEnabled,
            Blend srcAlpha, Blend destAlpha, BlendFunction alphaBlendFunc,
            Blend srcColor, Blend destColor, BlendFunction colorBlendFunc);

        public abstract DepthStencilState CreateDepthStencilState(bool isDepthEnabled, DepthComparison comparison);

        public abstract RasterizerState CreateRasterizerState(
            FaceCullingMode cullMode,
            TriangleFillMode fillMode,
            bool isDepthClipEnabled,
            bool isScissorTestEnabled
            );
    }
}
