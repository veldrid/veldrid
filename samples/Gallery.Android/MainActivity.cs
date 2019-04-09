using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Content.PM;

namespace Veldrid.SampleGallery
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public class MainActivity : AppCompatActivity
    {
        private VeldridSurfaceView _view;

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
            backend = GraphicsBackend.OpenGLES;
            _view = new VeldridSurfaceView(this, backend, options);
            _view.DeviceCreated += OnDeviceCreated;
            SetContentView(_view);
        }

        private void OnDeviceCreated()
        {
            Gallery gallery = new Gallery(_view);
            gallery.LoadExample(new SimpleMeshRender());
            _view.RunContinuousRenderLoop();
        }
    }
}
