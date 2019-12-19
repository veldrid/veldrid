#if !EXCLUDE_D3D11_BACKEND
using System;
using Veldrid.D3D11;

namespace Veldrid
{
    /// <summary>
    /// Exposes Direct3D 11-specific functionality, useful for interoperating with native components which interface directly with
    /// Direct3D 11. Can only be used on a GraphicsDevice whose GraphicsBackend is D3D11.
    /// </summary>
    public class BackendInfoD3D11
    {
        private readonly D3D11GraphicsDevice _gd;

        internal BackendInfoD3D11(D3D11GraphicsDevice gd)
        {
            _gd = gd;
        }

        /// <summary>
        /// Creates a new <see cref="GraphicsDevice"/> using Direct3D 11.
        /// </summary>
        /// <param name="adapter">DXGI adapter instance or null for default adapter.</param>
        /// <param name="flags">Describes properties of the Direct3D 11 graphics device.</param>
        /// <param name="swapchainDesc">Swapchain description.</param>
        /// <returns>A new <see cref="GraphicsDevice"/> using the Direct3D 11 API.</returns>
        public static GraphicsDevice CreateD3D11(IntPtr adapterPtr, SharpDX.Direct3D11.DeviceCreationFlags flags, SwapchainDescription? swapchainDesc = null)
        {
            return new D3D11.D3D11GraphicsDevice(adapterPtr, flags, swapchainDesc);
        }

        /// <summary>
        /// Gets a pointer to the ID3D11Device controlled by the GraphicsDevice.
        /// </summary>
        public IntPtr Device => _gd.Device.NativePointer;

        public IntPtr Adapter => _gd.Adapter.NativePointer;

        /// <summary>
        /// Gets a pointer to the native texture wrapped by the given Veldrid Texture. Depending on the instance's TextureType,
        /// this will be a pointer to an ID3D11Texture1D, an ID3D11Texture2D, or an ID3D11Texture3D.
        /// </summary>
        /// <returns>A pointer to the Veldrid Texture's underlying ID3D11Texture1D, ID3D11Texture2D, or ID3D11Texture3D. The type
        /// of this object depends on the parameter's TextureType.</returns>
        public IntPtr GetTexturePointer(Texture texture)
            => Util.AssertSubtype<Texture, D3D11Texture>(texture).DeviceTexture.NativePointer;
    }
}
#endif
