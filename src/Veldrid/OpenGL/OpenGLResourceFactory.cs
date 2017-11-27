using static Veldrid.OpenGLBinding.OpenGLNative;
using Veldrid.OpenGL;
using Veldrid.OpenGLBinding;

namespace Veldrid.OpenGL
{
    internal class OpenGLResourceFactory : ResourceFactory
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly StagingMemoryPool _pool = new StagingMemoryPool();

        public override GraphicsBackend BackendType => GraphicsBackend.OpenGL;

        public unsafe OpenGLResourceFactory(OpenGLGraphicsDevice gd)
        {
            _gd = gd;
        }

        public override CommandList CreateCommandList(ref CommandListDescription description)
        {
            return new OpenGLCommandList(ref description);
        }

        public override Framebuffer CreateFramebuffer(ref FramebufferDescription description)
        {
            return new OpenGLFramebuffer(_gd, ref description);
        }

        public override Pipeline CreateGraphicsPipeline(ref GraphicsPipelineDescription description)
        {
            return new OpenGLPipeline(_gd, ref description);
        }

        public override Pipeline CreateComputePipeline(ref ComputePipelineDescription description)
        {
            return new OpenGLPipeline(_gd, ref description);
        }

        public override ResourceLayout CreateResourceLayout(ref ResourceLayoutDescription description)
        {
            return new OpenGLResourceLayout(ref description);
        }

        public override ResourceSet CreateResourceSet(ref ResourceSetDescription description)
        {
            return new OpenGLResourceSet(ref description);
        }

        public override Sampler CreateSampler(ref SamplerDescription description)
        {
            return new OpenGLSampler(_gd, ref description);
        }

        public override Shader CreateShader(ref ShaderDescription description)
        {
            StagingBlock stagingBlock = _pool.Stage(description.ShaderBytes);
            return new OpenGLShader(_gd, description.Stage, stagingBlock);
        }

        protected override Texture CreateTextureCore(ref TextureDescription description)
        {
            return new OpenGLTexture(_gd, ref description);
        }

        protected override TextureView CreateTextureViewCore(ref TextureViewDescription description)
        {
            return new OpenGLTextureView(_gd, ref description);
        }

        protected override Buffer CreateBufferCore(ref BufferDescription description)
        {
            return new OpenGLBuffer(
                _gd, 
                description.SizeInBytes, 
                description.Usage);
        }
    }
}