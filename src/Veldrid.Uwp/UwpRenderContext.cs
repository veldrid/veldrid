using SharpDX;
using System.Threading;
using Veldrid.Graphics.Direct3D;
using Veldrid.Platform;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Veldrid
{
    public class UwpRenderContext : D3DRenderContext
    {
        private readonly ManualResetEvent _xamlFrameRequestedEvent;

        internal UwpRenderContext(Window window, SharpDX.Direct3D11.Device existingDevice, SharpDX.DXGI.SwapChain existingSwapchain, SwapChainPanel swp)
            : base(window, existingDevice, existingSwapchain)
        {
            _xamlFrameRequestedEvent = new ManualResetEvent(false);
            CompositionTarget.Rendering += (sender, e) => _xamlFrameRequestedEvent.Set();
        }

        public static D3DRenderContext CreateFromSwapChainPanel(SwapChainPanel swp)
        {
            SharpDX.Direct3D11.Device device;
            SharpDX.DXGI.SwapChain swapChain;

            // Create a new Direct3D hardware device and ask for Direct3D 11.2 support
            using (SharpDX.Direct3D11.Device defaultDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, SharpDX.Direct3D11.DeviceCreationFlags.Debug))
            {
                device = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device2>();
            }

            // We have to take into account pixel scaling; Windows Phone 8.1 uses virtual resolutions smaller than the physical screen size.
            float pixelScale = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi / 96.0f;

            // Properties of the swap chain
            SharpDX.DXGI.SwapChainDescription1 swapChainDescription = new SharpDX.DXGI.SwapChainDescription1()
            {
                // No transparency.
                AlphaMode = SharpDX.DXGI.AlphaMode.Ignore,
                // Double buffer.
                BufferCount = 2,
                // BGRA 32bit pixel format.
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                // Unlike in CoreWindow swap chains, the dimensions must be set.
                Height = (int)(swp.RenderSize.Height * pixelScale),
                Width = (int)(swp.RenderSize.Width * pixelScale),
                // Default multisampling.
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                // In case the control is resized, stretch the swap chain accordingly.
                Scaling = SharpDX.DXGI.Scaling.Stretch,
                // No support for stereo display.
                Stereo = false,
                // Sequential displaying for double buffering.
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,
                // This swapchain is going to be used as the back buffer.
                Usage = SharpDX.DXGI.Usage.BackBuffer | SharpDX.DXGI.Usage.RenderTargetOutput,
            };

            // Retrive the SharpDX.DXGI device associated to the Direct3D device.
            using (SharpDX.DXGI.Device3 dxgiDevice3 = device.QueryInterface<SharpDX.DXGI.Device3>())
            {
                // Get the SharpDX.DXGI factory automatically created when initializing the Direct3D device.
                using (SharpDX.DXGI.Factory3 dxgiFactory3 = dxgiDevice3.Adapter.GetParent<SharpDX.DXGI.Factory3>())
                {
                    // Create the swap chain and get the highest version available.
                    using (SharpDX.DXGI.SwapChain1 swapChain1 = new SharpDX.DXGI.SwapChain1(dxgiFactory3, device, ref swapChainDescription))
                    {
                        swapChain = swapChain1.QueryInterface<SharpDX.DXGI.SwapChain2>();
                    }
                }
            }

            // Obtain a reference to the native COM object of the SwapChainPanel.
            using (SharpDX.DXGI.ISwapChainPanelNative nativeObject = ComObject.As<SharpDX.DXGI.ISwapChainPanelNative>(swp))
            {
                // Set its swap chain.
                nativeObject.SwapChain = swapChain;
            }

            var window = new UwpWindow(swp, pixelScale);
            return new UwpRenderContext(window, device, swapChain, swp);
        }

        protected override void PlatformSwapBuffers()
        {
            _xamlFrameRequestedEvent.WaitOne();
            base.PlatformSwapBuffers();
        }
    }
}
