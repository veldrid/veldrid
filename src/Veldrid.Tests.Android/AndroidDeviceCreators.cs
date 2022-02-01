using Xunit;

namespace Veldrid.Tests.Android
{
    public abstract class AndroidDeviceCreator : GraphicsDeviceCreator
    {
        public static MainActivity? Activity { get; set; }

        public VeldridSurfaceView SurfaceView { get; }

        public GraphicsDevice? GraphicsDevice => SurfaceView?.GraphicsDevice;

        public AndroidDeviceCreator(GraphicsBackend backend)
        {
            Skip.If(!GraphicsDevice.IsBackendSupported(backend));
            if(Activity is null)
                throw new NullReferenceException(nameof(Activity));
            SurfaceView = new VeldridSurfaceView(Activity, backend, new GraphicsDeviceOptions() { SwapchainDepthFormat = PixelFormat.R16_UNorm });
            Activity.Paused += SurfaceView.OnPause;
            Activity.Resumed += SurfaceView.OnResume;
            var layoutParams = new RelativeLayout.LayoutParams(10, 10);
            try
            {
                SurfaceView.AddViewToActivity(Activity, layoutParams);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            var activity = Activity ?? throw new NullReferenceException(nameof(Activity));
            SurfaceView.RemoveViewFromActivity(activity);
            activity.Paused -= SurfaceView.OnPause;
            activity.Resumed -= SurfaceView.OnResume;
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
