using System;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using System.IO;
using System.Diagnostics;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DMaterial : Material, IDisposable
    {
        private readonly Device _device;
        private readonly VertexShader _vertexShader;
        private readonly PixelShader _pixelShader;
        private readonly InputLayout _inputLayout;
        private readonly GlobalConstantBufferBinding[] _constantBufferBindings;
        private readonly PerObjectConstantBufferBinding[] _perObjectBufferBindings;
        private readonly ResourceViewBinding?[] _resourceViewBindings;
        private readonly ShaderTextureBinding[] _currentTextureBindings;

        private const ShaderFlags defaultShaderFlags
#if DEBUG
            = ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#else
            = ShaderFlags.OptimizationLevel3;
#endif

        public D3DMaterial(
            Device device,
            D3DRenderContext rc,
            string vertexShaderPath,
            string pixelShaderPath,
            MaterialVertexInput vertexInputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            MaterialTextureInputs textureInputs)
        {
            _device = device;

            string vsSource = File.ReadAllText(vertexShaderPath);
            string psSource = (vertexShaderPath == pixelShaderPath) ? vsSource : File.ReadAllText(pixelShaderPath);

            CompilationResult vsCompilation = ShaderBytecode.Compile(vsSource, "VS", "vs_5_0", defaultShaderFlags, sourceFileName: vertexShaderPath);
            CompilationResult psCompilation = ShaderBytecode.Compile(psSource, "PS", "ps_5_0", defaultShaderFlags, sourceFileName: pixelShaderPath);

            if (vsCompilation.HasErrors || vsCompilation.Message != null)
            {
                throw new InvalidOperationException("Error compiling shader: " + vsCompilation.Message);
            }
            if (psCompilation.HasErrors || psCompilation.Message != null)
            {
                throw new InvalidOperationException("Error compiling shader: " + psCompilation.Message);
            }

            _vertexShader = new VertexShader(device, vsCompilation.Bytecode.Data);
            _pixelShader = new PixelShader(device, psCompilation.Bytecode.Data);

            _inputLayout = CreateLayout(device, vsCompilation.Bytecode.Data, vertexInputs);

            int numGlobalElements = globalInputs.Elements.Length;
            _constantBufferBindings =
                (numGlobalElements > 0)
                ? new GlobalConstantBufferBinding[numGlobalElements]
                : Array.Empty<GlobalConstantBufferBinding>();
            for (int i = 0; i < numGlobalElements; i++)
            {
                var genericElement = globalInputs.Elements[i];
                BufferProviderPair pair;
                GlobalConstantBufferBinding cbb;
                if (genericElement.UseGlobalNamedBuffer)
                {
                    pair = rc.GetNamedGlobalBufferProviderPair(genericElement.GlobalProviderName);
                    cbb = new GlobalConstantBufferBinding(i, pair, false);
                }
                else
                {
                    D3DConstantBuffer constantBuffer = new D3DConstantBuffer(device, genericElement.DataProvider.DataSizeInBytes);
                    pair = new BufferProviderPair(constantBuffer, genericElement.DataProvider);
                    cbb = new GlobalConstantBufferBinding(i, pair, true);
                }

                _constantBufferBindings[i] = cbb;
            }

            int numPerObjectInputs = perObjectInputs.Elements.Length;
            _perObjectBufferBindings =
                (numPerObjectInputs > 0)
                ? new PerObjectConstantBufferBinding[numPerObjectInputs]
                : Array.Empty<PerObjectConstantBufferBinding>();
            for (int i = 0; i < numPerObjectInputs; i++)
            {
                var genericElement = perObjectInputs.Elements[i];
                D3DConstantBuffer constantBuffer = new D3DConstantBuffer(device, genericElement.BufferSizeInBytes);
                PerObjectConstantBufferBinding pocbb = new PerObjectConstantBufferBinding(i + numGlobalElements, constantBuffer);
                _perObjectBufferBindings[i] = pocbb;
            }

            int numTextures = textureInputs.Elements.Length;
            _resourceViewBindings = new ResourceViewBinding?[numTextures];
            _currentTextureBindings = new D3DTextureBinding[numTextures];
            for (int i = 0; i < numTextures; i++)
            {
                var genericElement = textureInputs.Elements[i];
                D3DTexture2D texture = (D3DTexture2D)genericElement.GetDeviceTexture(rc);
                if (texture != null)
                {
                    ShaderResourceViewDescription srvd = new ShaderResourceViewDescription();
                    srvd.Format = D3DFormats.MapFormatForShaderResourceView(texture.DeviceTexture.Description.Format);
                    srvd.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
                    srvd.Texture2D.MipLevels = texture.DeviceTexture.Description.MipLevels;
                    srvd.Texture2D.MostDetailedMip = 0;
                    ShaderResourceView resourceView = new ShaderResourceView(device, texture.DeviceTexture, srvd);
                    D3DTextureBinding binding = new D3DTextureBinding(resourceView, texture);
                    _resourceViewBindings[i] = new ResourceViewBinding(i, binding);
                }
                else
                {
                    _resourceViewBindings[i] = new ResourceViewBinding(i, null);
                }
            }
        }

        public void Apply()
        {
            _device.ImmediateContext.InputAssembler.InputLayout = _inputLayout;
            _device.ImmediateContext.VertexShader.Set(_vertexShader);
            _device.ImmediateContext.PixelShader.Set(_pixelShader);

            foreach (GlobalConstantBufferBinding cbBinding in _constantBufferBindings)
            {
                cbBinding.UpdateBuffer();
                _device.ImmediateContext.VertexShader.SetConstantBuffer(cbBinding.Slot, cbBinding.ConstantBuffer.Buffer);
                _device.ImmediateContext.PixelShader.SetConstantBuffer(cbBinding.Slot, cbBinding.ConstantBuffer.Buffer);
            }

            for (int i = 0; i < _perObjectBufferBindings.Length; i++)
            {
                PerObjectConstantBufferBinding binding = _perObjectBufferBindings[i];
                _device.ImmediateContext.VertexShader.SetConstantBuffer(binding.Slot, binding.ConstantBuffer.Buffer);
                _device.ImmediateContext.PixelShader.SetConstantBuffer(binding.Slot, binding.ConstantBuffer.Buffer);
            }

            ApplyDefaultTextureBindings();
        }

        private void ApplyDefaultTextureBindings()
        {
            foreach (ResourceViewBinding? rvBinding in _resourceViewBindings)
            {
                if (rvBinding.Value.TextureBinding != null)
                {
                    CoreUseTexture(rvBinding.Value.Slot, rvBinding.Value.TextureBinding, false);
                }
                else
                {
                    _currentTextureBindings[rvBinding.Value.Slot] = null;
                }
            }
        }

        private static InputLayout CreateLayout(Device device, byte[] shaderBytecode, MaterialVertexInput vertexInputs)
        {
            int numElements = vertexInputs.Elements.Length;
            InputElement[] elements = new InputElement[numElements];
            int currentOffset = 0;
            for (int i = 0; i < numElements; i++)
            {
                var genericElement = vertexInputs.Elements[i];
                elements[i] = new InputElement(
                    GetSemanticName(genericElement.SemanticType),
                    0,
                    ConvertGenericFormat(genericElement.ElementFormat),
                    currentOffset, 0);
                currentOffset += genericElement.SizeInBytes;
            }

            return new InputLayout(device, shaderBytecode, elements);
        }

        private static string GetSemanticName(VertexSemanticType semanticType)
        {
            switch (semanticType)
            {
                case VertexSemanticType.Position:
                    return "POSITION";
                case VertexSemanticType.TextureCoordinate:
                    return "TEXCOORD";
                case VertexSemanticType.Normal:
                    return "NORMAL";
                case VertexSemanticType.Color:
                    return "COLOR";
                default:
                    throw Illegal.Value<VertexSemanticType>();
            }
        }

        private static SharpDX.DXGI.Format ConvertGenericFormat(VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Float1:
                    return SharpDX.DXGI.Format.R32_Float;
                case VertexElementFormat.Float2:
                    return SharpDX.DXGI.Format.R32G32_Float;
                case VertexElementFormat.Float3:
                    return SharpDX.DXGI.Format.R32G32B32_Float;
                case VertexElementFormat.Float4:
                    return SharpDX.DXGI.Format.R32G32B32A32_Float;
                case VertexElementFormat.Byte1:
                    return SharpDX.DXGI.Format.A8_UNorm;
                case VertexElementFormat.Byte4:
                    return SharpDX.DXGI.Format.R8G8B8A8_UNorm;
                default:
                    throw Illegal.Value<VertexElementFormat>();
            }
        }

        public void ApplyPerObjectInput(ConstantBufferDataProvider dataProvider)
        {
            if (_perObjectBufferBindings.Length != 1)
            {
                throw new InvalidOperationException(
                    "ApplyPerObjectInput can only be used when a material has exactly one per-object input.");
            }

            PerObjectConstantBufferBinding binding = _perObjectBufferBindings[0];
            dataProvider.SetData(binding.ConstantBuffer);
        }

        public void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders)
        {
            if (_perObjectBufferBindings.Length != dataProviders.Length)
            {
                throw new InvalidOperationException(
                    "dataProviders must contain the exact number of per-object buffer bindings used in the material.");
            }

            for (int i = 0; i < _perObjectBufferBindings.Length; i++)
            {
                PerObjectConstantBufferBinding binding = _perObjectBufferBindings[i];
                ConstantBufferDataProvider provider = dataProviders[i];
                provider.SetData(binding.ConstantBuffer);
            }
        }

        public void UseDefaultTextures()
        {
            ApplyDefaultTextureBindings();
        }

        public void UseTexture(int slot, ShaderTextureBinding binding)
        {
            if (!(binding is D3DTextureBinding))
            {
                throw new InvalidOperationException("Illegal shader texture binding used.");
            }

            CoreUseTexture(slot, binding, true);
        }

        private void CoreUseTexture(int slot, ShaderTextureBinding binding, bool useCache)
        {
            if (!useCache || _currentTextureBindings[slot] != binding)
            {
                var srv = ((D3DTextureBinding)binding).ResourceView;
                _device.ImmediateContext.PixelShader.SetShaderResource(slot, srv);
            }

            _currentTextureBindings[slot] = binding;
        }

        internal void ClearTextureBindings()
        {
            for (int i = 0; i < _currentTextureBindings.Length; i++)
            {
                _currentTextureBindings[i] = null;
            }
        }

        public void Dispose()
        {
            _vertexShader.Dispose();
            _pixelShader.Dispose();
            _inputLayout.Dispose();
            foreach (var binding in _constantBufferBindings)
            {
                binding.ConstantBuffer.Dispose();
            }
            foreach (var binding in _perObjectBufferBindings)
            {
                binding.ConstantBuffer.Dispose();
            }
            foreach (var binding in _resourceViewBindings)
            {
                binding.Value.TextureBinding?.Dispose();
            }
        }

        private struct GlobalConstantBufferBinding
        {
            // Is this binding local to this Material, or shared in the RenderContext?
            private readonly bool _isLocalBinding;

            public int Slot { get; }
            public BufferProviderPair Pair { get; }
            public D3DConstantBuffer ConstantBuffer => (D3DConstantBuffer)Pair.ConstantBuffer;

            public GlobalConstantBufferBinding(int slot, BufferProviderPair pair, bool isLocalBinding)
            {
                Slot = slot;
                Pair = pair;
                _isLocalBinding = isLocalBinding;
            }

            public void UpdateBuffer()
            {
                if (_isLocalBinding)
                {
                    Pair.UpdateData();
                }
            }
        }

        private struct PerObjectConstantBufferBinding
        {
            public int Slot { get; }
            public D3DConstantBuffer ConstantBuffer { get; }

            public PerObjectConstantBufferBinding(int slot, D3DConstantBuffer constantBuffer)
            {
                Slot = slot;
                ConstantBuffer = constantBuffer;
            }
        }

        private struct ResourceViewBinding
        {
            public int Slot { get; }
            public D3DTextureBinding TextureBinding { get; }

            public ResourceViewBinding(int slot, D3DTextureBinding binding)
            {
                Slot = slot;
                TextureBinding = binding;
            }
        }
    }
}