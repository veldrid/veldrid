using Xunit;

namespace Veldrid.Tests
{
    public abstract class ResourceSetTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void ResourceSet_BufferInsteadOfTextureView_Fails()
        {
            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("TV0", ResourceKind.TextureReadOnly, ShaderStages.Vertex)));

            DeviceBuffer ub = RF.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            Assert.Throws<VeldridException>(() =>
            {
                ResourceSet set = RF.CreateResourceSet(new ResourceSetDescription(layout,
                    ub));
            });
        }

        [Fact]
        public void ResourceSet_IncorrectTextureUsage_Fails()
        {
            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("TV0", ResourceKind.TextureReadWrite, ShaderStages.Vertex)));

            Texture t = RF.CreateTexture(new TextureDescription(64, 64, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            TextureView tv = RF.CreateTextureView(t);

            Assert.Throws<VeldridException>(() =>
            {
                ResourceSet set = RF.CreateResourceSet(new ResourceSetDescription(layout, tv));
            });
        }

        [Fact]
        public void ResourceSet_IncorrectBufferUsage_Fails()
        {
            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("RWB0", ResourceKind.StructuredBufferReadWrite, ShaderStages.Vertex)));

            DeviceBuffer readOnlyBuffer = RF.CreateBuffer(new BufferDescription(1024, BufferUsage.StructuredBufferReadOnly, 16));

            Assert.Throws<VeldridException>(() =>
            {
                ResourceSet set = RF.CreateResourceSet(new ResourceSetDescription(layout, readOnlyBuffer));
            });
        }

        [Fact]
        public void ResourceSet_TooFewOrTooManyElements_Fails()
        {
            ResourceLayout layout = RF.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("UB0", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("UB1", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("UB2", ResourceKind.UniformBuffer, ShaderStages.Vertex)));

            DeviceBuffer ub = RF.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            Assert.Throws<VeldridException>(() =>
            {
                RF.CreateResourceSet(new ResourceSetDescription(layout, ub));
            });

            Assert.Throws<VeldridException>(() =>
            {
                RF.CreateResourceSet(new ResourceSetDescription(layout, ub, ub));
            });

            Assert.Throws<VeldridException>(() =>
            {
                RF.CreateResourceSet(new ResourceSetDescription(layout, ub, ub, ub, ub));
            });

            Assert.Throws<VeldridException>(() =>
            {
                RF.CreateResourceSet(new ResourceSetDescription(layout, ub, ub, ub, ub, ub));
            });
        }
    }

    public class OpenGLResourceSetTests : ResourceSetTests<OpenGLDeviceCreator> { }
#if TEST_VULKAN
    public class VulkanResourceSetTests : ResourceSetTests<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    public class D3D11ResourceSetTests : ResourceSetTests<D3D11DeviceCreator> { }
#endif
}
