using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;
using Windows.UI;
using SharpDX.Mathematics.Interop;
using Veldrid.Graphics.Direct3D;
using Veldrid.Graphics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RenderDemo.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private D3D11.Device2 device;
        private D3D11.DeviceContext2 deviceContext;
        private DXGI.SwapChain2 swapChain;
        private D3D11.Texture2D backBufferTexture;
        private D3D11.RenderTargetView backBufferView;
        private D3DRenderContext _context;

        public MainPage()
        {
            this.InitializeComponent();

        }

        private void SwapChainPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // Create a new Direct3D hardware device and ask for Direct3D 11.2 support
            using (D3D11.Device defaultDevice = new D3D11.Device(D3D.DriverType.Hardware, D3D11.DeviceCreationFlags.Debug))
            {
                this.device = defaultDevice.QueryInterface<D3D11.Device2>();
            }

            // Save the context instance
            this.deviceContext = this.device.ImmediateContext2;

            // We have to take into account pixel scaling; Windows Phone 8.1 uses virtual resolutions smaller than the physical screen size.
            float pixelScale = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi / 96.0f;

            // Properties of the swap chain
            DXGI.SwapChainDescription1 swapChainDescription = new DXGI.SwapChainDescription1()
            {
                // No transparency.
                AlphaMode = DXGI.AlphaMode.Ignore,
                // Double buffer.
                BufferCount = 2,
                // BGRA 32bit pixel format.
                Format = DXGI.Format.B8G8R8A8_UNorm,
                // Unlike in CoreWindow swap chains, the dimensions must be set.
                Height = (int)(this.SwapChainPanel.RenderSize.Height * pixelScale),
                Width = (int)(this.SwapChainPanel.RenderSize.Width * pixelScale),
                // Default multisampling.
                SampleDescription = new DXGI.SampleDescription(1, 0),
                // In case the control is resized, stretch the swap chain accordingly.
                Scaling = DXGI.Scaling.Stretch,
                // No support for stereo display.
                Stereo = false,
                // Sequential displaying for double buffering.
                SwapEffect = DXGI.SwapEffect.FlipSequential,
                // This swapchain is going to be used as the back buffer.
                Usage = DXGI.Usage.BackBuffer | DXGI.Usage.RenderTargetOutput,
            };

            // Retrive the DXGI device associated to the Direct3D device.
            using (DXGI.Device3 dxgiDevice3 = this.device.QueryInterface<DXGI.Device3>())
            {
                // Get the DXGI factory automatically created when initializing the Direct3D device.
                using (DXGI.Factory3 dxgiFactory3 = dxgiDevice3.Adapter.GetParent<DXGI.Factory3>())
                {
                    // Create the swap chain and get the highest version available.
                    using (DXGI.SwapChain1 swapChain1 = new DXGI.SwapChain1(dxgiFactory3, this.device, ref swapChainDescription))
                    {
                        this.swapChain = swapChain1.QueryInterface<DXGI.SwapChain2>();
                    }
                }
            }

            // Obtain a reference to the native COM object of the SwapChainPanel.
            using (DXGI.ISwapChainPanelNative nativeObject = ComObject.As<DXGI.ISwapChainPanelNative>(this.SwapChainPanel))
            {
                // Set its swap chain.
                nativeObject.SwapChain = this.swapChain;
            }

            // Create a Texture2D from the existing swap chain to use as 
            this.backBufferTexture = D3D11.Texture2D.FromSwapChain<D3D11.Texture2D>(this.swapChain, 0);
            this.backBufferView = new D3D11.RenderTargetView(this.device, this.backBufferTexture);

            // This event is fired when the application requests a new frame from the DirectX interop controls.
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            Veldrid.RenderDemo.Program.Main(new UwpWindow(this, SwapChainPanel), device, swapChain);
        }

        private void CompositionTarget_Rendering(object sender, object e)
        {
            //// Set the active back buffer and clear it.
            //this.deviceContext.OutputMerger.SetRenderTargets(this.backBufferView);
            //this.deviceContext.ClearRenderTargetView(this.backBufferView, new RawColor4(.9f, .3f, .9f, 1f));

            //// Tell the swap chain to present the buffer.
            //this.swapChain.Present(1, DXGI.PresentFlags.None, new DXGI.PresentParameters());
            //_context.ClearBuffer(RgbaFloat.Red);
            //_context.SwapBuffers();
        }
    }
}
