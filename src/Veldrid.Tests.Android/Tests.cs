using Xunit;

namespace Veldrid.Tests.Android
{
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESBufferTests : BufferTestBase<AndroidOpenGLESDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESComputeTests : ComputeTests<AndroidOpenGLESDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESDisposalTests : DisposalTestBase<AndroidOpenGLESDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESFramebufferTests : FramebufferTests<AndroidOpenGLESDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESSwapchainFramebufferTests : SwapchainFramebufferTests<AndroidOpenGLESDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESPipelineTests : PipelineTests<AndroidOpenGLESDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESRenderTests : RenderTests<AndroidOpenGLESDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESResourceSetTests : ResourceSetTests<AndroidOpenGLESDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESTextureTests : TextureTestBase<AndroidOpenGLESDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class OpenGLESVertexLayoutTests : VertexLayoutTests<AndroidOpenGLESDeviceCreator> { }


    [Trait("Backend", "Vulkan")]
    public class VulkanBufferTests : BufferTestBase<AndroidVulkanDeviceCreator> { }
    [Trait("Backend", "Vulkan")]
    public class VulkanComputeTests : ComputeTests<AndroidVulkanDeviceCreator> { }
    [Trait("Backend", "Vulkan")]
    public class VulkanDisposalTests : DisposalTestBase<AndroidVulkanDeviceCreator> { }
    [Trait("Backend", "Vulkan")]
    public class VulkanFramebufferTests : FramebufferTests<AndroidVulkanDeviceCreator> { }
    [Trait("Backend", "Vulkan")]
    public class VulkanSwapchainFramebufferTests : SwapchainFramebufferTests<AndroidVulkanDeviceCreator> { }
    [Trait("Backend", "OpenGLES")]
    public class VulkanPipelineTests : PipelineTests<AndroidVulkanDeviceCreator> { }
    [Trait("Backend", "Vulkan")]
    public class VulkanRenderTests : RenderTests<AndroidVulkanDeviceCreator> { }
    [Trait("Backend", "Vulkan")]
    public class VulkanResourceSetTests : ResourceSetTests<AndroidVulkanDeviceCreator> { }
    [Trait("Backend", "Vulkan")]
    public class VulkanTextureTests : TextureTestBase<AndroidVulkanDeviceCreator> { }
    [Trait("Backend", "Vulkan")]
    public class VulkanVertexLayoutTests : VertexLayoutTests<AndroidVulkanDeviceCreator> { }
}
