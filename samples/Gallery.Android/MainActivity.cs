using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Veldrid;
using Veldrid.SampleGallery;
using System;
using Android.Content.PM;

namespace Gallery.Android
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AppCompatActivity
    {
        private VeldridSurfaceView _view;
        private SimpleMeshRender _example;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            bool isDebugBuild = false;
#if DEBUG
            isDebugBuild = true;
#endif

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: isDebugBuild,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: false,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true,
                swapchainSrgbFormat: true);
            GraphicsBackend backend = GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan)
                ? GraphicsBackend.Vulkan
                : GraphicsBackend.OpenGLES;
            _view = new VeldridSurfaceView(this, backend, options);
            _view.DeviceCreated += OnDeviceCreated;
            _example = new SimpleMeshRender();
            SetContentView(_view);
        }

        private void OnDeviceCreated()
        {
            _example.Initialize(_view.GraphicsDevice, _view.MainSwapchain);
            _example.LoadResourcesAsync().Wait();
            _view.RunContinuousRenderLoop(() => _example.Render(0.0));
        }
    }
}
