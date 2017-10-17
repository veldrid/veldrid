using System;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace Vd2.D3D11
{
    internal class D3D11CommandList : CommandList
    {
        private readonly DeviceContext _context;

        private RawViewportF[] _viewports = new RawViewportF[0];
        private RawRectangle[] _scissors = new RawRectangle[0];
        private D3D11Framebuffer _fb;

        public D3D11CommandList(Device device, ref CommandListDescription description)
            : base(ref description)
        {
            _context = new DeviceContext(device);
        }

        public SharpDX.Direct3D11.CommandList DeviceCommandList { get; private set; }

        public override void Begin()
        {
            DeviceCommandList?.Dispose();
            DeviceCommandList = null;
            _context.ClearState();
        }

        public override void SetIndexBuffer(IndexBuffer ib)
        {
            D3D11IndexBuffer d3d11Buffer = Util.AssertSubtype<IndexBuffer, D3D11IndexBuffer>(ib);
            _context.InputAssembler.SetIndexBuffer(d3d11Buffer.Buffer, D3D11Formats.ToDxgiFormat(ib.Format), 0);
        }

        public override void SetPipeline(Pipeline pipeline)
        {
            D3D11Pipeline dp = Util.AssertSubtype<Pipeline, D3D11Pipeline>(pipeline);
            _context.OutputMerger.SetBlendState(dp.BlendState);
            _context.OutputMerger.SetDepthStencilState(dp.DepthStencilState);
            _context.Rasterizer.State = dp.RasterizerState;
            _context.InputAssembler.PrimitiveTopology = dp.PrimitiveTopology;
            _context.InputAssembler.InputLayout = dp.InputLayout;
            _context.VertexShader.Set(dp.VertexShader);
            _context.GeometryShader.Set(dp.GeometryShader);
            _context.HullShader.Set(dp.HullShader);
            _context.DomainShader.Set(dp.DomainShader);
            _context.PixelShader.Set(dp.PixelShader);
        }

        public override void SetResourceSet(ResourceSet rs)
        {
            D3D11ResourceSet d3d11RS = Util.AssertSubtype<ResourceSet, D3D11ResourceSet>(rs);
            D3D11ResourceLayout layout = d3d11RS.Layout;
            BindableResource[] resources = d3d11RS.Resources;
            for (int i = 0; i < resources.Length; i++)
            {
                BindableResource resource = resources[i];
                (int slot, ShaderStages stages) = layout.GetDeviceSlotIndex(i);
                if (resource is D3D11TextureView texView)
                {
                    BindTextureView(texView, slot, stages);
                }
                else if (resource is D3D11UniformBuffer ub)
                {
                    BindUniformBuffer(ub, slot, stages);
                }
                else if (resource is D3D11Sampler sampler)
                {
                    BindSampler(sampler, slot, stages);
                }
            }
        }

        public override void SetVertexBuffer(uint index, VertexBuffer vb, uint vertexStrideInBytes)
        {
            D3D11VertexBuffer d3d11Buffer = Util.AssertSubtype<VertexBuffer, D3D11VertexBuffer>(vb);
            VertexBufferBinding vbb = new VertexBufferBinding(d3d11Buffer.Buffer, (int)vertexStrideInBytes, 0);
            _context.InputAssembler.SetVertexBuffers((int)index, vbb);
        }

        public override void Draw(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            _context.Rasterizer.SetViewports(_viewports, _viewports.Length);
            if (_scissors.Length > 0)
            {
                _context.Rasterizer.SetScissorRectangles(_scissors);
            }

            if (instanceCount == 1)
            {
                _context.DrawIndexed((int)indexCount, (int)indexStart, vertexOffset);
            }
            else
            {
                _context.DrawIndexedInstanced((int)indexCount, (int)instanceCount, (int)indexStart, vertexOffset, (int)instanceStart);
            }
        }

        public override void End()
        {
            if (DeviceCommandList != null)
            {
                throw new VdException("Invalid use of End().");
            }

            DeviceCommandList = _context.FinishCommandList(true);
        }

        public override void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            Util.EnsureArraySize(ref _scissors, index + 1);
            _scissors[index] = new RawRectangle((int)x, (int)y, (int)(x + width), (int)(y + height));
        }

        public override void SetViewport(uint index, ref Viewport viewport)
        {
            Util.EnsureArraySize(ref _viewports, index + 1);
            _viewports[index] = new RawViewportF
            {
                X = viewport.X,
                Y = viewport.Y,
                Width = viewport.Width,
                Height = viewport.Height,
                MinDepth = viewport.MinDepth,
                MaxDepth = viewport.MaxDepth
            };
        }

        private void BindTextureView(D3D11TextureView texView, int slot, ShaderStages stages)
        {
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                _context.VertexShader.SetShaderResource(slot, texView.ShaderResourceView);
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                _context.GeometryShader.SetShaderResource(slot, texView.ShaderResourceView);
            }
            if ((stages & ShaderStages.TesselationControl) == ShaderStages.TesselationControl)
            {
                _context.HullShader.SetShaderResource(slot, texView.ShaderResourceView);
            }
            if ((stages & ShaderStages.TesselationEvaluation) == ShaderStages.TesselationEvaluation)
            {
                _context.DomainShader.SetShaderResource(slot, texView.ShaderResourceView);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                _context.PixelShader.SetShaderResource(slot, texView.ShaderResourceView);
            }
        }

        private void BindUniformBuffer(D3D11UniformBuffer ub, int slot, ShaderStages stages)
        {
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                _context.VertexShader.SetConstantBuffer(slot, ub.Buffer);
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                _context.GeometryShader.SetConstantBuffer(slot, ub.Buffer);
            }
            if ((stages & ShaderStages.TesselationControl) == ShaderStages.TesselationControl)
            {
                _context.HullShader.SetConstantBuffer(slot, ub.Buffer);
            }
            if ((stages & ShaderStages.TesselationEvaluation) == ShaderStages.TesselationEvaluation)
            {
                _context.DomainShader.SetConstantBuffer(slot, ub.Buffer);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                _context.PixelShader.SetConstantBuffer(slot, ub.Buffer);
            }
        }

        private void BindSampler(D3D11Sampler sampler, int slot, ShaderStages stages)
        {
            if ((stages & ShaderStages.Vertex) == ShaderStages.Vertex)
            {
                _context.VertexShader.SetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.Geometry) == ShaderStages.Geometry)
            {
                _context.GeometryShader.SetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.TesselationControl) == ShaderStages.TesselationControl)
            {
                _context.HullShader.SetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.TesselationEvaluation) == ShaderStages.TesselationEvaluation)
            {
                _context.DomainShader.SetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                _context.PixelShader.SetSampler(slot, sampler.DeviceSampler);
            }
        }

        public override void SetFramebuffer(Framebuffer vfb)
        {
            D3D11Framebuffer fb = Util.AssertSubtype<Framebuffer, D3D11Framebuffer>(vfb);
            _fb = fb;
            _context.OutputMerger.SetRenderTargets(fb.DepthStencilView, fb.RenderTargetViews);
        }

        public override void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            _context.ClearRenderTargetView(_fb.RenderTargetViews[index], new RawColor4(clearColor.R, clearColor.G, clearColor.B, clearColor.A));
        }

        public override void ClearDepthTarget(float depth)
        {
            _context.ClearDepthStencilView(_fb.DepthStencilView, DepthStencilClearFlags.Depth, depth, 0);
        }

        public override void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            D3D11Buffer d3dBuffer = Util.AssertSubtype<Buffer, D3D11Buffer>(buffer);
            if (sizeInBytes == 0)
            {
                return;
            }

            ResourceRegion? subregion = null;
            if ((d3dBuffer.Buffer.Description.BindFlags & BindFlags.ConstantBuffer) != BindFlags.ConstantBuffer)
            {
                // For a shader-constant buffer; set pDstBox to null. It is not possible to use
                // this method to partially update a shader-constant buffer

                subregion = new ResourceRegion()
                {
                    Left = (int)bufferOffsetInBytes,
                    Right = (int)(sizeInBytes + bufferOffsetInBytes),
                    Bottom = 1,
                    Back = 1
                };
            }

            _context.UpdateSubresource(d3dBuffer.Buffer, 0, subregion, source, 0, 0);
        }

        public override void UpdateTexture2D(
            Texture2D texture2D,
            IntPtr source,
            uint sizeInBytes,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            SharpDX.Direct3D11.Texture2D deviceTexture = Util.AssertSubtype<Texture2D, D3D11Texture2D>(texture2D).DeviceTexture;
            ResourceRegion resourceRegion = new ResourceRegion(
                left: (int)x,
                top: (int)y,
                front: 0,
                right: (int)x + (int)width,
                bottom: (int)y + (int)height,
                back: 1);
            uint srcRowPitch = FormatHelpers.GetSizeInBytes(texture2D.Format) * width;
            _context.UpdateSubresource(deviceTexture, (int)mipLevel, resourceRegion, source, (int)srcRowPitch, 0);
        }

        public override void UpdateTextureCube(
            TextureCube textureCube,
            IntPtr source,
            uint sizeInBytes,
            CubeFace face,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            SharpDX.Direct3D11.Texture2D deviceTexture = Util.AssertSubtype<TextureCube, D3D11TextureCube>(textureCube).DeviceTexture;

            ResourceRegion resourceRegion = new ResourceRegion(
                left: (int)x,
                right: (int)x + (int)width,
                top: (int)y,
                bottom: (int)y + (int)height,
                front: 0,
                back: 1);
            uint srcRowPitch = FormatHelpers.GetSizeInBytes(textureCube.Format) * width;
            int subresource = GetSubresource(face, mipLevel, textureCube.MipLevels);
            _context.UpdateSubresource(deviceTexture, subresource, resourceRegion, source, (int)srcRowPitch, 0);
        }

        private int GetSubresource(CubeFace face, uint level, uint totalLevels)
        {
            int faceOffset;
            switch (face)
            {
                case CubeFace.NegativeX:
                    faceOffset = 1;
                    break;
                case CubeFace.PositiveX:
                    faceOffset = 0;
                    break;
                case CubeFace.NegativeY:
                    faceOffset = 3;
                    break;
                case CubeFace.PositiveY:
                    faceOffset = 2;
                    break;
                case CubeFace.NegativeZ:
                    faceOffset = 4;
                    break;
                case CubeFace.PositiveZ:
                    faceOffset = 5;
                    break;
                default:
                    throw Illegal.Value<CubeFace>();
            }

            return faceOffset * (int)totalLevels + (int)level;
        }

        public override void Dispose()
        {
            DeviceCommandList?.Dispose();
            _context.Dispose();
        }
    }
}