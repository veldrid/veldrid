//using ImGuiNET;
//using System.Numerics;
//using Veldrid.Graphics;

//namespace Veldrid.RenderDemo
//{
//    public class ImGuiRenderer
//    {
//        private readonly Texture _fontTexture;
//        private readonly ConstantDataProvider<Matrix4x4> _projectionMatrixProvider;

//        public ImGuiRenderer(RenderContext rc)
//        {
//            ResourceFactory factory = rc.ResourceFactory;
//            CreateFontsTexture(factory);
//            _projectionMatrixProvider = new ConstantDataProvider<Matrix4x4>(Matrix4x4.CreateOrthographic(rc.Window.Width, rc.Window.Height, 1, 1000));

//            Material mat = factory.CreateMaterial("imgui-vertex", "imgui-frag",
//                new MaterialVertexInput(32, new MaterialVertexInputElement[]
//                {
//                    new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float2),
//                    new MaterialVertexInputElement("in_texcoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2),
//                    new MaterialVertexInputElement("in_color", 0, VertexElementFormat.Float4)
//                }),
//                new MaterialInputs<MaterialGlobalInputElement>(new MaterialGlobalInputElement[]
//                {
//                    new MaterialGlobalInputElement("ProjectionMatrixBuffer", MaterialInputType.Matrix4x4, _projectionMatrixProvider)
//                }),
//                MaterialInputs<MaterialPerObjectInputElement>.Empty,
//                new MaterialTextureInputs(new MaterialTextureInputElement[]
//                {
//                    new MaterialTextureInputElement("texture0", _fontTexture)
//                }));


//            //// Create the blending setup
//            //{
//            //    BlendStateDescription desc = new BlendStateDescription(); //BlendStateDescription.Default();

//            //    desc.AlphaToCoverageEnable = false;
//            //    desc.RenderTarget[0].IsBlendEnabled = true;
//            //    desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
//            //    desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
//            //    desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
//            //    desc.RenderTarget[0].SourceAlphaBlend = BlendOption.InverseSourceAlpha;
//            //    desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
//            //    desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
//            //    desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
//            //    _blendState = new BlendState(_device, desc);
//            //}

//            //// Create the rasterizer state
//            //{
//            //    RasterizerStateDescription desc = new RasterizerStateDescription();
//            //    desc.FillMode = FillMode.Solid;
//            //    desc.CullMode = CullMode.None;
//            //    desc.IsScissorEnabled = true;
//            //    desc.IsDepthClipEnabled = true;
//            //    _rasterizerState = new RasterizerState(_device, desc);
//            //}
//        }

//        private void CreateFontsTexture()
//        {
//            IO io = ImGui.GetIO();

//            // Build
//            var textureData = io.FontAtlas.GetTexDataAsRGBA32();

//            // Create DX11 texture
//            {
//                Texture2DDescription texDesc = new Texture2DDescription();
//                texDesc.Width = textureData.Width;
//                texDesc.Height = textureData.Height;
//                texDesc.MipLevels = 1;
//                texDesc.ArraySize = 1;
//                texDesc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
//                texDesc.SampleDescription.Count = 1;
//                texDesc.Usage = ResourceUsage.Default;
//                texDesc.BindFlags = BindFlags.ShaderResource;
//                texDesc.CpuAccessFlags = CpuAccessFlags.None;

//                SharpDX.Direct3D11.Texture2D pTexture;
//                DataRectangle subResource = new DataRectangle(new IntPtr(textureData.Pixels), texDesc.Width * 4);
//                //subResource.SlicePitch = 0;
//                pTexture = new SharpDX.Direct3D11.Texture2D(_device, texDesc, subResource);

//                // Create texture view
//                ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription();
//                srvDesc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
//                srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
//                srvDesc.Texture2D.MipLevels = texDesc.MipLevels;
//                srvDesc.Texture2D.MostDetailedMip = 0;
//                _fontTextureView = new ShaderResourceView(_device, pTexture, srvDesc);
//                pTexture.Dispose();
//            }

//            // Store our identifier
//            io.FontAtlas.SetTexID(_fontTextureView.NativePointer);

//            // Create texture sampler
//            {
//                SamplerStateDescription samplerDesc = SamplerStateDescription.Default();
//                samplerDesc.Filter = Filter.MinMagMipLinear;
//                samplerDesc.AddressU = TextureAddressMode.Wrap;
//                samplerDesc.AddressV = TextureAddressMode.Wrap;
//                samplerDesc.AddressW = TextureAddressMode.Wrap;
//                samplerDesc.MipLodBias = 0f;
//                samplerDesc.ComparisonFunction = Comparison.Always;
//                samplerDesc.MinimumLod = 0f;
//                samplerDesc.MaximumLod = 0f;
//                _fontSampler = new SamplerState(_device, samplerDesc);
//            }

//            // Cleanup (don't clear the input data if you want to append new fonts later)
//            io.FontAtlas.ClearTexData();
//        }
//    }
//}
//}
