using System;
using System.Collections.Generic;
using System.Numerics;

using static Veldrid.VirtualReality.Oculus.LibOvrNative;

namespace Veldrid.VirtualReality.Oculus
{
    internal class OculusMirrorTexture : IDisposable
    {
        public static readonly Guid s_d3d11Tex2DGuid = new Guid("6f15aaf2-d208-4e89-9ab4-489535d34f9c");

        private readonly OculusContext _context;
        private readonly Dictionary<OutputDescription, TextureBlitter> _blitters
            = new Dictionary<OutputDescription, TextureBlitter>();

        private (uint width, uint height, MirrorTextureEyeSource source) _texProperties;
        private ovrMirrorTexture _ovrMirrorTex;
        private Texture _vdMirrorTex;
        private TextureView _vdMirrorTexView;
        private ResourceSet _set;
        private Texture _vkTrueMirrorTex;

        public OculusMirrorTexture(OculusContext oculusContext)
        {
            _context = oculusContext;
        }

        private void CreateMirrorTex(uint width, uint height, MirrorTextureEyeSource source)
        {
            _set?.Dispose();
            _set = null;
            _vdMirrorTexView?.Dispose();
            if (_vkTrueMirrorTex != null)
            {
                _vdMirrorTex.Dispose();
            }
            if (!_ovrMirrorTex.IsNull)
            {
                ovr_DestroyMirrorTexture(_context.Session, _ovrMirrorTex);
            }

            switch (_context.GraphicsDevice.BackendType)
            {
                case GraphicsBackend.Direct3D11:
                    CreateMirrorTextureD3D11(width, height, source);
                    break;
                case GraphicsBackend.Vulkan:
                    CreateMirrorTextureVulkan(width, height, source);
                    break;
                case GraphicsBackend.OpenGL:
                case GraphicsBackend.OpenGLES:
                    CreateMirrorTextureGL(width, height, source);
                    break;
                default:
                    throw new VeldridException("This backend does not support VR.");
            }

            _texProperties = (width, height, source);
        }

        private unsafe void CreateMirrorTextureGL(uint width, uint height, MirrorTextureEyeSource source)
        {
            GraphicsDevice gd = _context.GraphicsDevice;

            uint glID = default;

            ovrMirrorTextureDesc desc;
            desc.Format = ovrTextureFormat.R8G8B8A8_UNORM_SRGB;
            desc.Width = (int)width;
            desc.Height = (int)height;
            desc.MiscFlags = ovrTextureMiscFlags.DX_Typeless;
            desc.MirrorOptions = source == MirrorTextureEyeSource.LeftEye
                ? ovrMirrorOptions.LeftEyeOnly
                : source == MirrorTextureEyeSource.RightEye
                    ? ovrMirrorOptions.RightEyeOnly
                    : ovrMirrorOptions.Default;

            gd.GetOpenGLInfo().ExecuteOnGLThread(() =>
            {
                ovrMirrorTextureDesc localDesc = desc;
                localDesc.MiscFlags = localDesc.MiscFlags & ~(ovrTextureMiscFlags.DX_Typeless | ovrTextureMiscFlags.AllowGenerateMips);
                ovrMirrorTexture localTex;
                var result = ovr_CreateMirrorTextureWithOptionsGL(_context.Session, &localDesc, &localTex);
                if (result != ovrResult.Success)
                {
                    return;
                }
                _ovrMirrorTex = localTex;

                uint localID;
                result = ovr_GetMirrorTextureBufferGL(_context.Session, _ovrMirrorTex, &localID);
                if (result != ovrResult.Success)
                {
                    return;
                }
                glID = localID;
            });

            if (_ovrMirrorTex.IsNull)
            {
                throw new VeldridException("Failed to create OpenGL Mirror Texture");
            }

            _vdMirrorTex = gd.ResourceFactory.CreateTexture(
                glID,
                OculusUtil.GetVeldridTextureDescription(desc));
            _vdMirrorTexView = _context.GraphicsDevice.ResourceFactory.CreateTextureView(_vdMirrorTex);
        }

        private unsafe void CreateMirrorTextureVulkan(uint width, uint height, MirrorTextureEyeSource source)
        {
            GraphicsDevice gd = _context.GraphicsDevice;

            ovrMirrorTextureDesc desc;
            desc.Format = ovrTextureFormat.R8G8B8A8_UNORM_SRGB;
            desc.Width = (int)width;
            desc.Height = (int)height;
            desc.MiscFlags = ovrTextureMiscFlags.DX_Typeless;
            desc.MirrorOptions = source == MirrorTextureEyeSource.LeftEye
                ? ovrMirrorOptions.LeftEyeOnly
                : source == MirrorTextureEyeSource.RightEye
                    ? ovrMirrorOptions.RightEyeOnly
                    : ovrMirrorOptions.Default;

            desc.MiscFlags = desc.MiscFlags & ~(ovrTextureMiscFlags.DX_Typeless);

            ovrMirrorTexture mirrorTex;
            ovrResult result = ovr_CreateMirrorTextureWithOptionsVk(
                _context.Session,
                gd.GetVulkanInfo().Device,
                &desc,
                &mirrorTex);
            if (result != ovrResult.Success)
            {
                throw new VeldridException($"Failed to create Vulkan mirror texture: {result}");
            }
            _ovrMirrorTex = mirrorTex;

            ulong vkImage;
            result = ovr_GetMirrorTextureBufferVk(_context.Session, mirrorTex, &vkImage);
            if (result != ovrResult.Success)
            {
                throw new VeldridException($"Failed to get Vulkan Mirror Texture image: {result}.");
            }

            _vkTrueMirrorTex = gd.ResourceFactory.CreateTexture(
                vkImage,
                OculusUtil.GetVeldridTextureDescription(desc));
            gd.GetVulkanInfo().OverrideImageLayout(
                _vkTrueMirrorTex,
                (uint)Vulkan.VkImageLayout.TransferSrcOptimal);

            _vdMirrorTex = gd.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(
                    _vkTrueMirrorTex.Width, _vkTrueMirrorTex.Height,
                    _vkTrueMirrorTex.MipLevels, _vkTrueMirrorTex.ArrayLayers,
                    _vkTrueMirrorTex.Format,
                    TextureUsage.Sampled));
            _vdMirrorTexView = _context.GraphicsDevice.ResourceFactory.CreateTextureView(_vdMirrorTex);
        }

        private unsafe void CreateMirrorTextureD3D11(uint width, uint height, MirrorTextureEyeSource source)
        {
            ovrMirrorTextureDesc desc;
            desc.Format = ovrTextureFormat.R8G8B8A8_UNORM_SRGB;
            desc.Width = (int)width;
            desc.Height = (int)height;
            desc.MiscFlags = ovrTextureMiscFlags.DX_Typeless;
            desc.MirrorOptions = source == MirrorTextureEyeSource.LeftEye
                ? ovrMirrorOptions.LeftEyeOnly
                : source == MirrorTextureEyeSource.RightEye
                    ? ovrMirrorOptions.RightEyeOnly
                    : ovrMirrorOptions.Default;

            ovrMirrorTexture mirrorTexture;
            ovrResult result = ovr_CreateMirrorTextureWithOptionsDX(
                _context.Session,
                _context.GraphicsDevice.GetD3D11Info().Device,
                &desc,
                &mirrorTexture);
            if (result != ovrResult.Success)
            {
                throw new VeldridException($"Failed to create DX mirror texture: {result}");
            }
            _ovrMirrorTex = mirrorTexture;

            IntPtr mirrord3d11Tex;
            result = ovr_GetMirrorTextureBufferDX(_context.Session, mirrorTexture, s_d3d11Tex2DGuid, &mirrord3d11Tex);
            if (result != ovrResult.Success)
            {
                throw new VeldridException($"Failed to get D3D11 mirror texture handle: {result}");
            }

            _vdMirrorTex = _context.GraphicsDevice.ResourceFactory.CreateTexture(
                (ulong)mirrord3d11Tex,
                OculusUtil.GetVeldridTextureDescription(desc));
            _vdMirrorTexView = _context.GraphicsDevice.ResourceFactory.CreateTextureView(_vdMirrorTex);
        }

        public void Render(CommandList cl, Framebuffer fb, MirrorTextureEyeSource source)
        {
            EnsureMirrorTexture(fb.Width, fb.Height, source);
            TextureBlitter blitter = GetBlitter(fb.OutputDescription);
            ResourceSet set = GetResourceSet(blitter);

            if (_vkTrueMirrorTex != null)
            {
                cl.CopyTexture(_vkTrueMirrorTex, _vdMirrorTex);
            }

            cl.SetFramebuffer(fb);
            blitter.Render(cl, set, Vector2.Zero, Vector2.One);
        }

        private void EnsureMirrorTexture(uint width, uint height, MirrorTextureEyeSource source)
        {
            if (_texProperties != (width, height, source))
            {
                CreateMirrorTex(width, height, source);
            }
        }

        private ResourceSet GetResourceSet(TextureBlitter blitter)
        {
            if (_set == null)
            {
                _set = _context.GraphicsDevice.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(blitter.ResourceLayout, _vdMirrorTexView, _context.GraphicsDevice.PointSampler));
            }

            return _set;
        }

        private TextureBlitter GetBlitter(OutputDescription outputDescription)
        {
            if (!_blitters.TryGetValue(outputDescription, out TextureBlitter ret))
            {
                ret = new TextureBlitter(
                    _context.GraphicsDevice,
                    _context.GraphicsDevice.ResourceFactory,
                    outputDescription,
                    srgbOutput: false);

                _blitters.Add(outputDescription, ret);
            }

            return ret;
        }

        public void Dispose()
        {
            foreach (KeyValuePair<OutputDescription, TextureBlitter> kvp in _blitters)
            {
                kvp.Value.Dispose();
            }
            _set?.Dispose();
            _vdMirrorTex?.Dispose();
            _vdMirrorTexView?.Dispose();
            ovr_DestroyMirrorTexture(_context.Session, _ovrMirrorTex);
        }
    }
}