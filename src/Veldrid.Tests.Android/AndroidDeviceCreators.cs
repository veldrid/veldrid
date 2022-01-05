using Android.Views;
using Xunit;

namespace Veldrid.Tests.Android
{
    public abstract class AndroidDeviceCreator : GraphicsDeviceCreator
    {
        public static MainActivity? Activity { get; set; }

        public VeldridSurfaceView SurfaceView { get; private set; }

        public GraphicsDevice GraphicsDevice => SurfaceView.GraphicsDevice;

        public AndroidDeviceCreator(GraphicsBackend backend)
        {
            Skip.If(!GraphicsDevice.IsBackendSupported(backend));
            SurfaceView = new VeldridSurfaceView(Activity, backend);
            var layoutParams = new ViewGroup.LayoutParams(200, 200);
            ManualResetEventSlim mre = new ManualResetEventSlim(false);
            SurfaceView.DeviceCreated += mre.Set;
            Activity.RunOnUiThread(() =>
            {
                Activity.AddContentView(SurfaceView, layoutParams);
            });
            mre.Wait();
            SurfaceView.DeviceCreated -= mre.Set;
            SurfaceView.RunContinuousRenderLoop();
        }

        public void Dispose()
        {
            ManualResetEventSlim mre = new ManualResetEventSlim(false);
            SurfaceView.DeviceDisposed += mre.Set;
            Activity.RunOnUiThread(() =>
            {
                ((ViewGroup)SurfaceView.Parent).RemoveView(SurfaceView);
            });
            mre.Wait();
            SurfaceView.DeviceDisposed -= mre.Set;
            SurfaceView.Disable();
        }
    }

    public class AndroidOpenGLESDeviceCreator : AndroidDeviceCreator
    {
        public AndroidOpenGLESDeviceCreator() : base(GraphicsBackend.OpenGLES) { }
    }

    public class AndroidVulkanDeviceCreator : AndroidDeviceCreator
    {
        public AndroidVulkanDeviceCreator() : base(GraphicsBackend.Vulkan) { }
    }
}
