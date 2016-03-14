using ImGuiNET;
using OpenTK;
using OpenTK.Input;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using System;
using System.Numerics;
using Veldrid.Graphics;
using Veldrid.Graphics.Direct3D;
using Veldrid.Platform;

namespace Veldrid.RenderDemo
{
    public class ImGuiRenderer
    {
        private readonly DynamicDataProvider<Matrix4x4> _projectionMatrixProvider;
        private readonly Material _material;
        private TextureData _fontTexture;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private float _wheelPosition;
        private BlendState _blendState;
        private RasterizerState _rasterizerState;
        private SamplerState _fontSampler;

        public ImGuiRenderer(RenderContext rc)
        {
            ResourceFactory factory = rc.ResourceFactory;
            _vertexBuffer = factory.CreateVertexBuffer(1);
            _indexBuffer = factory.CreateIndexBuffer(1);
            CreateFontsTexture(rc);
            _projectionMatrixProvider = new DynamicDataProvider<Matrix4x4>(Matrix4x4.CreateOrthographic(rc.Window.Width, rc.Window.Height, 1, 1000));

            _material = factory.CreateMaterial("imgui-vertex", "imgui-frag",
                new MaterialVertexInput(32, new MaterialVertexInputElement[]
                {
                    new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float2),
                    new MaterialVertexInputElement("in_texcoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2),
                    new MaterialVertexInputElement("in_color", VertexSemanticType.Color, VertexElementFormat.Byte4)
                }),
                new MaterialInputs<MaterialGlobalInputElement>(new MaterialGlobalInputElement[]
                {
                    new MaterialGlobalInputElement("ProjectionMatrixUniform", MaterialInputType.Matrix4x4, _projectionMatrixProvider)
                }),
                MaterialInputs<MaterialPerObjectInputElement>.Empty,
                new MaterialTextureInputs(new MaterialTextureInputElement[]
                {
                    new MaterialTextureInputElement("surfaceTexture", _fontTexture)
                }));
        }

        public unsafe void SetPerFrameImGuiData(RenderContext rc)
        {
            IO io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                rc.Window.Width,
                rc.Window.Height);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(1.0f);
            io.DeltaTime = 1f / 60f; // TODO: Wrong value
        }

        public unsafe void UpdateImGuiInput(NativeWindow window)
        {
            IO io = ImGui.GetIO();
            MouseState cursorState = Mouse.GetCursorState();
            MouseState mouseState = Mouse.GetState();

            if (window.Bounds.Contains(cursorState.X, cursorState.Y))
            {
                Point windowPoint = window.PointToClient(new Point(cursorState.X, cursorState.Y));
                io.MousePosition = new System.Numerics.Vector2(
                    windowPoint.X / 1f,
                    windowPoint.Y / 1f);
            }
            else
            {
                io.MousePosition = new System.Numerics.Vector2(-1f, -1f);
            }

            io.MouseDown[0] = mouseState.LeftButton == ButtonState.Pressed;
            io.MouseDown[1] = mouseState.RightButton == ButtonState.Pressed;
            io.MouseDown[2] = mouseState.MiddleButton == ButtonState.Pressed;

            float newWheelPos = mouseState.WheelPrecise;
            float delta = newWheelPos - _wheelPosition;
            _wheelPosition = newWheelPos;
            io.MouseWheel = delta;
        }

        private unsafe void CreateFontsTexture(RenderContext rc)
        {
            ImGui.LoadDefaultFont();
            IO io = ImGui.GetIO();

            // Build
            var textureData = io.FontAtlas.GetTexDataAsRGBA32();
            int[] pixels = new int[textureData.Width * textureData.Height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = ((int*)textureData.Pixels)[i];
            }

            _fontTexture = new RawTextureDataArray<int>(pixels, textureData.Width, textureData.Height, textureData.BytesPerPixel, PixelFormat.R8_G8_B8_A8);

            //// Create DX11 texture
            //{
            //    Texture2DDescription texDesc = new Texture2DDescription();
            //    texDesc.Width = textureData.Width;
            //    texDesc.Height = textureData.Height;
            //    texDesc.MipLevels = 1;
            //    texDesc.ArraySize = 1;
            //    texDesc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            //    texDesc.SampleDescription.Count = 1;
            //    texDesc.Usage = ResourceUsage.Default;
            //    texDesc.BindFlags = BindFlags.ShaderResource;
            //    texDesc.CpuAccessFlags = CpuAccessFlags.None;

            //    SharpDX.Direct3D11.Texture2D pTexture;
            //    DataRectangle subResource = new DataRectangle(new IntPtr(textureData.Pixels), texDesc.Width * 4);
            //    //subResource.SlicePitch = 0;
            //    pTexture = new SharpDX.Direct3D11.Texture2D(_device, texDesc, subResource);

            //    // Create texture view
            //    ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription();
            //    srvDesc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            //    srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
            //    srvDesc.Texture2D.MipLevels = texDesc.MipLevels;
            //    srvDesc.Texture2D.MostDetailedMip = 0;
            //    _fontTextureView = new SharpDX.Direct3D11.ShaderResourceView(_device, pTexture, srvDesc).;
            //    pTexture.Dispose();
            //}

            // Store our identifier
            io.FontAtlas.SetTexID(420);

            // Create texture sampler
            {
                SamplerStateDescription samplerDesc = SamplerStateDescription.Default();
                samplerDesc.Filter = Filter.MinMagMipLinear;
                samplerDesc.AddressU = TextureAddressMode.Wrap;
                samplerDesc.AddressV = TextureAddressMode.Wrap;
                samplerDesc.AddressW = TextureAddressMode.Wrap;
                samplerDesc.MipLodBias = 0f;
                samplerDesc.ComparisonFunction = Comparison.Always;
                samplerDesc.MinimumLod = 0f;
                samplerDesc.MaximumLod = 0f;
                _fontSampler = new SamplerState(((D3DRenderContext)rc)._device, samplerDesc);
            }

            // Cleanup (don't clear the input data if you want to append new fonts later)
            io.FontAtlas.ClearTexData();
        }

        public unsafe void RenderImDrawData(DrawData* draw_data, RenderContext rc)
        {
            D3DRenderContext drc = (D3DRenderContext)rc;
            var deviceContext = drc._device.ImmediateContext;

            {
                // Create the blending setup
                if (_blendState == null)
                {
                    BlendStateDescription desc = new BlendStateDescription(); //BlendStateDescription.Default();

                    desc.AlphaToCoverageEnable = false;
                    desc.RenderTarget[0].IsBlendEnabled = true;
                    desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
                    desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
                    desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
                    desc.RenderTarget[0].SourceAlphaBlend = BlendOption.InverseSourceAlpha;
                    desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
                    desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
                    desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
                    _blendState = new BlendState(drc._device, desc);
                }

                // Create the rasterizer state
                if (_rasterizerState == null)
                {
                    RasterizerStateDescription desc = new RasterizerStateDescription();
                    desc.FillMode = FillMode.Solid;
                    desc.CullMode = CullMode.None;
                    desc.IsScissorEnabled = false; // TODO: Should be true.
                    desc.IsDepthClipEnabled = false;
                    _rasterizerState = new RasterizerState(drc._device, desc);
                }
            }
            //// Create and grow vertex/index buffers if needed
            //if (g_pVB == null || g_VertexBufferSize < draw_data->TotalVtxCount)
            //{
            //    if (g_pVB != null)
            //    {
            //        g_pVB.Dispose();
            //        g_pVB = null;
            //    }

            //    g_VertexBufferSize = draw_data->TotalVtxCount + 5000;
            //    BufferDescription desc = new BufferDescription();
            //    desc.Usage = ResourceUsage.Dynamic;
            //    desc.SizeInBytes = g_VertexBufferSize * sizeof(DrawVert);
            //    desc.BindFlags = BindFlags.VertexBuffer;
            //    desc.CpuAccessFlags = CpuAccessFlags.Write;
            //    desc.OptionFlags = ResourceOptionFlags.None;
            //    g_pVB = new SharpDX.Direct3D11.Buffer(_device, desc);
            //}

            //if (g_pIB == null || g_IndexBufferSize < draw_data->TotalIdxCount)
            //{
            //    if (g_pIB != null) { g_pIB.Dispose(); g_pIB = null; }
            //    g_IndexBufferSize = draw_data->TotalIdxCount + 10000;
            //    BufferDescription bufferDesc = new BufferDescription();
            //    bufferDesc.Usage = ResourceUsage.Dynamic;
            //    bufferDesc.SizeInBytes = g_IndexBufferSize * sizeof(ushort); // sizeof(ImDrawIdx), ImDrawIdx = typedef unsigned short
            //    bufferDesc.BindFlags = BindFlags.IndexBuffer;
            //    bufferDesc.CpuAccessFlags = CpuAccessFlags.Write;
            //    g_pIB = new SharpDX.Direct3D11.Buffer(_device, bufferDesc);
            //}

            // Copy and convert all vertices into a single contiguous buffer

            VertexDescriptor descriptor = new VertexDescriptor((byte)sizeof(DrawVert), 3, 0, IntPtr.Zero);

            int vertexOffsetInBytes = 0;
            int indexOffsetInBytes = 0;

            for (int n = 0; n < draw_data->CmdListsCount; n++)
            {
                DrawList* cmd_list = draw_data->CmdLists[n];
                //System.Buffer.MemoryCopy(
                //    cmd_list->VtxBuffer.Data,
                //    vtx_dst,
                //    cmd_list->VtxBuffer.Size * sizeof(DrawVert),
                //    cmd_list->VtxBuffer.Size * sizeof(DrawVert));

                //System.Buffer.MemoryCopy(
                //    cmd_list->IdxBuffer.Data,
                //    idx_dst,
                //    cmd_list->IdxBuffer.Size * sizeof(ushort),
                //    cmd_list->IdxBuffer.Size * sizeof(ushort));

                //vtx_dst += cmd_list->VtxBuffer.Size;
                //idx_dst += cmd_list->IdxBuffer.Size;

                _vertexBuffer.SetVertexData(new IntPtr(cmd_list->VtxBuffer.Data), descriptor, cmd_list->VtxBuffer.Size, vertexOffsetInBytes);
                _indexBuffer.SetIndices(new IntPtr(cmd_list->IdxBuffer.Data), IndexFormat.UInt16, sizeof(ushort), cmd_list->IdxBuffer.Size);

                vertexOffsetInBytes += cmd_list->VtxBuffer.Size * sizeof(DrawVert);
                indexOffsetInBytes += cmd_list->IdxBuffer.Size * sizeof(ushort);
            }

            // Setup orthographic projection matrix into our constant buffer
            {
                var io = ImGui.GetIO();
                Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                    0.0f,
                    io.DisplaySize.X / io.DisplayFramebufferScale.X,
                    io.DisplaySize.Y / io.DisplayFramebufferScale.Y,
                    0.0f,
                    -1.0f,
                    1.0f);

                _projectionMatrixProvider.Data = mvp;
            }

            // Setup viewport
            {
                RawViewportF vp = new RawViewportF();
                vp.Width = ImGui.GetIO().DisplaySize.X;
                vp.Height = ImGui.GetIO().DisplaySize.Y;
                vp.MinDepth = 0.0f;
                vp.MaxDepth = 1.0f;
                vp.X = 0;
                vp.Y = 0;
                deviceContext.Rasterizer.SetViewport(0, 0, rc.Window.Width, rc.Window.Height, 0, 1);
            }

            // Bind shader and vertex buffers
            //int stride = sizeof(DrawVert);
            //int offset = 0;
            //var ia = deviceContext.InputAssembler;
            //ia.InputLayout = _inputLayout;
            //VertexBufferBinding vbBinding = new VertexBufferBinding(g_pVB, stride, offset);
            //ia.SetVertexBuffers(0, vbBinding);
            //ia.SetIndexBuffer(g_pIB, SharpDX.DXGI.Format.R16_UInt, 0);
            //ia.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

            rc.SetVertexBuffer(_vertexBuffer);
            rc.SetIndexBuffer(_indexBuffer);
            rc.SetMaterial(_material);

            //deviceContext.VertexShader.SetShader(_vertexShader, null, 0);
            //deviceContext.VertexShader.SetConstantBuffer(0, g_pVertexConstantBuffer);

            //deviceContext.PixelShader.SetShader(_pixelShader, null, 0);
            deviceContext.PixelShader.SetSamplers(0, 1, _fontSampler);

            // Setup render state
            RawColor4 blendFactor = new RawColor4(0f, 0f, 0f, 0f);
            deviceContext.OutputMerger.SetBlendState(_blendState, blendFactor, 0xffffffff);

            deviceContext.Rasterizer.State = _rasterizerState;

            ImGui.ScaleClipRects(draw_data, ImGui.GetIO().DisplayFramebufferScale);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data->CmdListsCount; n++)
            {
                DrawList* cmd_list = draw_data->CmdLists[n];
                for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++)
                {
                    DrawCmd* pcmd = &(((DrawCmd*)cmd_list->CmdBuffer.Data)[cmd_i]);
                    if (pcmd->UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        //deviceContext.PixelShader.SetShaderResources(0, 1, new ShaderResourceView(pcmd->TextureId));
                        Console.WriteLine(pcmd->TextureId);
                        rc.SetScissorRectangle(
                            (int)pcmd->ClipRect.X,
                            (int)pcmd->ClipRect.Y,
                            (int)pcmd->ClipRect.Z,
                            (int)pcmd->ClipRect.W);

                        rc.DrawIndexedPrimitives(idx_offset, (int)pcmd->ElemCount, vtx_offset);
                        //deviceContext.DrawIndexed((int)pcmd->ElemCount, idx_offset, vtx_offset);
                    }

                    idx_offset += (int)pcmd->ElemCount;
                }
                vtx_offset += cmd_list->VtxBuffer.Size;
            }
        }
    }
}
