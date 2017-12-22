using System;
using Xunit;

namespace Veldrid.Tests
{
    public abstract class SemaphoreTests<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        [Fact]
        public void Semaphore_OutOfOrderSubmission_Fails()
        {
            Semaphore s = RF.CreateSemaphore();
            CommandList a = RF.CreateCommandList();
            a.Begin();
            a.End();
            Assert.Throws<VeldridException>(() => GD.SubmitCommands(a, s, null, null));
        }

        [Fact]
        public void Semaphore_InOrderSubmission_Succeeds()
        {
            Semaphore s = RF.CreateSemaphore();
            for (int i = 0; i < 10; i++)
            {
                CommandList a = RF.CreateCommandList();
                a.Begin();
                a.End();
                CommandList b = RF.CreateCommandList();
                b.Begin();
                b.End();
                GD.SubmitCommands(a, null, s, null);
                GD.SubmitCommands(b, s, null, null);
            }
        }
    }

    public class OpenGLSemaphoreTests : SemaphoreTests<OpenGLDeviceCreator> { }
#if TEST_VULKAN
    public class VulkanSemaphoreTests : SemaphoreTests<VulkanDeviceCreator> { }
#endif
#if TEST_D3D11
    public class D3D11SemaphoreTests : SemaphoreTests<D3D11DeviceCreator> { }
#endif
}
