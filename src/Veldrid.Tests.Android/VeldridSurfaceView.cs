using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;

namespace Veldrid.Tests.Android
{
    public class VeldridSurfaceView : SurfaceView, ISurfaceHolderCallback
    {
        private readonly GraphicsBackend _backend;
        protected GraphicsDeviceOptions DeviceOptions { get; }

        public GraphicsDevice? GraphicsDevice { get; protected set; }
        public Swapchain? MainSwapchain { get; protected set; }

        public event Action? DeviceCreated;
        public event Action? DeviceDisposed;
        public event Action? Resized;

        private ManualResetEventSlim _surfaceCreated = new ManualResetEventSlim();
        private ManualResetEventSlim _surfaceDestroyed = new ManualResetEventSlim();

        public VeldridSurfaceView(Context context, GraphicsBackend backend, GraphicsDeviceOptions deviceOptions) : base(context)
        {
            if (!(backend == GraphicsBackend.Vulkan || backend == GraphicsBackend.OpenGLES))
            {
                throw new NotSupportedException($"{backend} is not supported on Android.");
            }

            _backend = backend;
            DeviceOptions = deviceOptions;
        }

        public void AddViewToActivity(Activity activity, ViewGroup.LayoutParams layoutParams)
        {
            activity.RunOnUiThread(() =>
            {
                OnResume();
                activity.AddContentView(this, layoutParams);
            });
            _surfaceCreated.Wait();
            bool deviceCreated = false;
            var surfaceHandle = Holder!.Surface!.Handle;
            if (_backend == GraphicsBackend.Vulkan)
            {
                System.Diagnostics.Debug.Assert(MainSwapchain == null);
                SwapchainSource ss = SwapchainSource.CreateAndroidSurface(surfaceHandle, JNIEnv.Handle);
                SwapchainDescription sd = new SwapchainDescription(
                    ss,
                    (uint)Width,
                    (uint)Height,
                    DeviceOptions.SwapchainDepthFormat,
                    DeviceOptions.SyncToVerticalBlank);

                if (GraphicsDevice == null)
                {
                    GraphicsDevice = GraphicsDevice.CreateVulkan(DeviceOptions, sd);
                    deviceCreated = true;
                }
                MainSwapchain = GraphicsDevice.MainSwapchain;
            }
            else
            {
                System.Diagnostics.Debug.Assert(GraphicsDevice == null && MainSwapchain == null);
                SwapchainSource ss = SwapchainSource.CreateAndroidSurface(surfaceHandle, JNIEnv.Handle);
                SwapchainDescription sd = new SwapchainDescription(
                    ss,
                    (uint)Width,
                    (uint)Height,
                    DeviceOptions.SwapchainDepthFormat,
                    DeviceOptions.SyncToVerticalBlank);
                GraphicsDevice = GraphicsDevice.CreateOpenGLES(DeviceOptions, sd);
                MainSwapchain = GraphicsDevice.MainSwapchain;
                deviceCreated = true;
            }

            if (deviceCreated)
            {
                DeviceCreated?.Invoke();
            }
        }

        public void RemoveViewFromActivity(Activity activity)
        {
            activity.RunOnUiThread(() =>
            {
                var parent = Parent as ViewGroup;
                if (parent is null)
                    throw new NullReferenceException($"Could not get {nameof(VeldridSurfaceView)}'s parent in {nameof(RemoveViewFromActivity)}");
                parent?.RemoveView(this);
            });

            _surfaceDestroyed.Wait();

            GraphicsDevice?.WaitForIdle();
            MainSwapchain?.Dispose();
            MainSwapchain = null;
            GraphicsDevice?.Dispose();
            GraphicsDevice = null;
            DeviceDisposed?.Invoke();
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            _surfaceCreated.Set();
            _surfaceDestroyed.Reset();
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            _surfaceDestroyed.Set();
            _surfaceCreated.Reset();
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            //MainSwapchain?.Resize((uint)Width, (uint)Height);
            Resized?.Invoke();
        }

        public void OnResume()
        {
            Holder?.AddCallback(this);
        }

        public void OnPause()
        {
            Holder?.RemoveCallback(this);
        }
    }
}
