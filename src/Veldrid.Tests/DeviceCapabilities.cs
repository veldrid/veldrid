using Xunit;
using Xunit.Abstractions;

namespace Veldrid.Tests
{
    public abstract class DeviceCapabilitiesBase<T> : GraphicsDeviceTestBase<T> where T : GraphicsDeviceCreator
    {
        private ITestOutputHelper _output;
        public DeviceCapabilitiesBase(ITestOutputHelper outputHelper)
        {
            _output = outputHelper;
        }

        [SkippableFact]
        public void DeviceInfo()
        {
            _output.WriteLine($"Backend: {GD.BackendType}");
            switch (GD.BackendType)
            {
                case GraphicsBackend.OpenGL:
                case GraphicsBackend.OpenGLES:
                    {
                        GD.GetOpenGLInfo(out var glInfo);
                        _output.WriteLine($"{nameof(glInfo.Version)}: {glInfo.Version}");
                        _output.WriteLine($"{nameof(glInfo.ShadingLanguageVersion)}: {glInfo.ShadingLanguageVersion}");

                        _output.WriteLine($"{nameof(glInfo.Extensions)}:");
                        foreach (var ext in glInfo.Extensions)
                            _output.WriteLine($"\t{ext}");

                        break;
                    }
                case GraphicsBackend.Vulkan:
                    {
                        GD.GetVulkanInfo(out var vkInfo);
                        _output.WriteLine($"{nameof(vkInfo.DriverName)}: {vkInfo.DriverName}");
                        _output.WriteLine($"{nameof(vkInfo.DriverInfo)}: {vkInfo.DriverInfo}");

                        _output.WriteLine($"{nameof(vkInfo.AvailableDeviceExtensions)}:");
                        foreach (var ext in vkInfo.AvailableDeviceExtensions)
                            _output.WriteLine($"\t{ext}");

                        _output.WriteLine($"{nameof(vkInfo.AvailableInstanceExtensions)}:");
                        foreach (var ext in vkInfo.AvailableInstanceExtensions)
                            _output.WriteLine($"\t{ext}");

                        _output.WriteLine($"{nameof(vkInfo.AvailableInstanceLayers)}:");
                        foreach (var ext in vkInfo.AvailableInstanceLayers)
                            _output.WriteLine($"\t{ext}");

                        break;
                    }
                case GraphicsBackend.Direct3D11:
                    {
                        GD.GetD3D11Info(out var d3dInfo);

                        break;
                    }
                case GraphicsBackend.Metal:
                    {
                        GD.GetMetalInfo(out var mtlInfo);
                        _output.WriteLine($"{nameof(mtlInfo.FeatureSet)}: {mtlInfo.FeatureSet}");
                        _output.WriteLine($"{nameof(mtlInfo.MaxFeatureSet)}: {mtlInfo.MaxFeatureSet}");
                        break;
                    }
            }
        }
    }

    [Trait("Backend", "OpenGL")]
    public class OpenGLDeviceCapabilities : DeviceCapabilitiesBase<OpenGLDeviceCreator>
    {
        public OpenGLDeviceCapabilities(ITestOutputHelper output) : base(output) { }
    }

    [Trait("Backend", "OpenGLES")]
    public class OpenGLESDeviceCapabilities : DeviceCapabilitiesBase<OpenGLESDeviceCreator>
    {
        public OpenGLESDeviceCapabilities(ITestOutputHelper output) : base(output) { }
    }

    [Trait("Backend", "Vulkan")]
    public class VulkanDeviceCapabilities : DeviceCapabilitiesBase<VulkanDeviceCreator>
    {
        public VulkanDeviceCapabilities(ITestOutputHelper output) : base(output) { }
    }

    [Trait("Backend", "D3D11")]
    public class D3D11DeviceCapabilities : DeviceCapabilitiesBase<D3D11DeviceCreator>
    {
        public D3D11DeviceCapabilities(ITestOutputHelper output) : base(output) { }
    }

    [Trait("Backend", "Metal")]
    public class MetalDeviceCapabilities : DeviceCapabilitiesBase<MetalDeviceCreator>
    {
        public MetalDeviceCapabilities(ITestOutputHelper output) : base(output) { }
    }
}
