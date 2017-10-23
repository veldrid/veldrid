using System;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace Vd2.D3D11
{
    internal class D3D11CommandList : CommandList
    {
        private readonly D3D11GraphicsDevice _gd;
        private readonly DeviceContext _context;

        private RawViewportF[] _viewports = new RawViewportF[0];
        private RawRectangle[] _scissors = new RawRectangle[0];
        private D3D11Framebuffer _fb;

        private uint _numVertexBindings = 0;
        private readonly SharpDX.Direct3D11.Buffer[] _vertexBindings = new SharpDX.Direct3D11.Buffer[10];
        private int[] _vertexStrides;
        private int[] _vertexOffsets = new int[10];
        private bool _begun;

        // Cached State
        private D3D11Pipeline _pipeline;
        private BlendState _blendState;
        private DepthStencilState _depthStencilState;
        private RasterizerState _rasterizerState;
        private SharpDX.Direct3D.PrimitiveTopology _primitiveTopology;
        private InputLayout _inputLayout;
        private VertexShader _vertexShader;
        private GeometryShader _geometryShader;
        private HullShader _hullShader;
        private DomainShader _domainShader;
        private PixelShader _pixelShader;

        public D3D11CommandList(D3D11GraphicsDevice gd, ref CommandListDescription description)
            : base(ref description)
        {
            _gd = gd;
            _context = new DeviceContext(gd.Device);
        }

        public SharpDX.Direct3D11.CommandList DeviceCommandList { get; set; }

        public override void Begin()
        {
            DeviceCommandList?.Dispose();
            DeviceCommandList = null;
            ClearState();
            _begun = true;
        }

        private void ClearState()
        {
            _context.ClearState();
            ResetManagedState();
        }

        private void ResetManagedState()
        {
            _numVertexBindings = 0;
            Array.Clear(_vertexBindings, 0, _vertexBindings.Length);
            _vertexStrides = null;
            Array.Clear(_vertexOffsets, 0, _vertexOffsets.Length);

            _fb = null;

            Array.Clear(_viewports, 0, _viewports.Length);
            Array.Clear(_scissors, 0, _scissors.Length);

            _pipeline = null;
            _blendState = null;
            _depthStencilState = null;
            _rasterizerState = null;
            _primitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            _inputLayout = null;
            _vertexShader = null;
            _geometryShader = null;
            _hullShader = null;
            _domainShader = null;
            _pixelShader = null;
        }

        public override void End()
        {
            if (DeviceCommandList != null)
            {
                throw new VdException("Invalid use of End().");
            }

            DeviceCommandList = _context.FinishCommandList(false);
            ResetManagedState();
            _begun = false;
        }

        public void Reset()
        {
            if (DeviceCommandList != null)
            {
                DeviceCommandList.Dispose();
                DeviceCommandList = null;
            }
            else if (_begun)
            {
                _context.ClearState();
                SharpDX.Direct3D11.CommandList cl = _context.FinishCommandList(false);
                cl.Dispose();
            }

            ResetManagedState();
        }

        public override void SetIndexBuffer(IndexBuffer ib)
        {
            D3D11IndexBuffer d3d11Buffer = Util.AssertSubtype<IndexBuffer, D3D11IndexBuffer>(ib);
            _context.InputAssembler.SetIndexBuffer(d3d11Buffer.Buffer, D3D11Formats.ToDxgiFormat(ib.Format), 0);
        }

        public override void SetPipeline(Pipeline pipeline)
        {
            D3D11Pipeline d3dPipeline = Util.AssertSubtype<Pipeline, D3D11Pipeline>(pipeline);
            if (_pipeline != d3dPipeline)
            {
                _pipeline = d3dPipeline;

                BlendState blendState = d3dPipeline.BlendState;
                if (_blendState != blendState)
                {
                    _blendState = blendState;
                    _context.OutputMerger.SetBlendState(blendState);
                }

                DepthStencilState depthStencilState = d3dPipeline.DepthStencilState;
                if (_depthStencilState != depthStencilState)
                {
                    _depthStencilState = depthStencilState;
                    _context.OutputMerger.SetDepthStencilState(depthStencilState);
                }

                RasterizerState rasterizerState = d3dPipeline.RasterizerState;
                if (_rasterizerState != rasterizerState)
                {
                    _rasterizerState = rasterizerState;
                    _context.Rasterizer.State = rasterizerState;
                }

                SharpDX.Direct3D.PrimitiveTopology primitiveTopology = d3dPipeline.PrimitiveTopology;
                if (_primitiveTopology != primitiveTopology)
                {
                    _primitiveTopology = primitiveTopology;
                    _context.InputAssembler.PrimitiveTopology = primitiveTopology;
                }

                InputLayout inputLayout = d3dPipeline.InputLayout;
                if (_inputLayout != inputLayout)
                {
                    _inputLayout = inputLayout;
                    _context.InputAssembler.InputLayout = inputLayout;
                }

                VertexShader vertexShader = d3dPipeline.VertexShader;
                if (_vertexShader != vertexShader)
                {
                    _vertexShader = vertexShader;
                    _context.VertexShader.Set(vertexShader);
                }

                GeometryShader geometryShader = d3dPipeline.GeometryShader;
                if (_geometryShader != geometryShader)
                {
                    _geometryShader = geometryShader;
                    _context.GeometryShader.Set(geometryShader);
                }

                HullShader hullShader = d3dPipeline.HullShader;
                if (_hullShader != hullShader)
                {
                    _hullShader = hullShader;
                    _context.HullShader.Set(hullShader);
                }

                DomainShader domainShader = d3dPipeline.DomainShader;
                if (_domainShader != domainShader)
                {
                    _domainShader = domainShader;
                    _context.DomainShader.Set(domainShader);
                }

                PixelShader pixelShader = d3dPipeline.PixelShader;
                if (_pixelShader != pixelShader)
                {
                    _pixelShader = pixelShader;
                    _context.PixelShader.Set(pixelShader);
                }

                _vertexStrides = d3dPipeline.VertexStrides;
            }
        }

        public override void SetResourceSet(ResourceSet rs)
        {
            D3D11ResourceSet d3d11RS = Util.AssertSubtype<ResourceSet, D3D11ResourceSet>(rs);
            D3D11ResourceLayout layout = d3d11RS.Layout;
            BindableResource[] resources = d3d11RS.Resources;
            for (int i = 0; i < resources.Length; i++)
            {
                BindableResource resource = resources[i];
                D3D11ResourceLayout.ResourceBindingInfo rbi = layout.GetDeviceSlotIndex(i);
                if (resource is D3D11TextureView texView)
                {
                    BindTextureView(texView, rbi.Slot, rbi.Stages);
                }
                else if (resource is D3D11UniformBuffer ub)
                {
                    BindUniformBuffer(ub, rbi.Slot, rbi.Stages);
                }
                else if (resource is D3D11Sampler sampler)
                {
                    BindSampler(sampler, rbi.Slot, rbi.Stages);
                }
            }
        }

        public override void SetVertexBuffer(uint index, VertexBuffer vb)
        {
            D3D11VertexBuffer d3d11Buffer = Util.AssertSubtype<VertexBuffer, D3D11VertexBuffer>(vb);
            _vertexBindings[index] = d3d11Buffer.Buffer;
            _numVertexBindings = Math.Max((index + 1), _numVertexBindings);
        }

        public override void Draw(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            FlushViewports();
            FlushScissorRects();
            FlushVertexBindings();

            if (instanceCount == 1)
            {
                _context.DrawIndexed((int)indexCount, (int)indexStart, vertexOffset);
            }
            else
            {
                _context.DrawIndexedInstanced((int)indexCount, (int)instanceCount, (int)indexStart, vertexOffset, (int)instanceStart);
            }
        }

        private void FlushViewports()
        {
            _context.Rasterizer.SetViewports(_viewports, _viewports.Length);
        }

        private void FlushScissorRects()
        {
            if (_scissors.Length > 0)
            {
                _context.Rasterizer.SetScissorRectangles(_scissors);
            }
        }

        private unsafe void FlushVertexBindings()
        {
            IntPtr* buffersPtr = stackalloc IntPtr[(int)_numVertexBindings];
            for (int i = 0; i < _numVertexBindings; i++)
            {
                buffersPtr[i] = _vertexBindings[i].NativePointer;
            }
            fixed (int* stridesPtr = _vertexStrides)
            fixed (int* offsetsPtr = _vertexOffsets)
            {
                _context.InputAssembler.SetVertexBuffers(0, (int)_numVertexBindings, (IntPtr)buffersPtr, (IntPtr)stridesPtr, (IntPtr)offsetsPtr);
            }
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
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                _context.HullShader.SetShaderResource(slot, texView.ShaderResourceView);
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
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
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                _context.HullShader.SetConstantBuffer(slot, ub.Buffer);
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
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
            if ((stages & ShaderStages.TessellationControl) == ShaderStages.TessellationControl)
            {
                _context.HullShader.SetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.TessellationEvaluation) == ShaderStages.TessellationEvaluation)
            {
                _context.DomainShader.SetSampler(slot, sampler.DeviceSampler);
            }
            if ((stages & ShaderStages.Fragment) == ShaderStages.Fragment)
            {
                _context.PixelShader.SetSampler(slot, sampler.DeviceSampler);
            }
        }

        public override void SetFramebuffer(Framebuffer fb)
        {
            D3D11Framebuffer d3dFB = Util.AssertSubtype<Framebuffer, D3D11Framebuffer>(fb);
            if (d3dFB.IsSwapchainFramebuffer)
            {
                _gd.CommandListsReferencingSwapchain.Add(this);
            }

            if (_fb != d3dFB)
            {
                _fb = d3dFB;
                _context.OutputMerger.SetRenderTargets(d3dFB.DepthStencilView, d3dFB.RenderTargetViews);
            }
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