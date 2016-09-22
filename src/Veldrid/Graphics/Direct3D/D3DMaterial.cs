using System;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using System.IO;
using System.Diagnostics;
using System.Linq;

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
            Stream vertexShaderStream,
            string vertexShaderName,
            Stream pixelShaderStream,
            string pixelShaderName,
            MaterialVertexInput[] vertexInputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            MaterialTextureInputs textureInputs)
        {
            _device = device;

            string vsSource, psSource;
            using (var vsSr = new StreamReader(vertexShaderStream))
            {
                vsSource = vsSr.ReadToEnd();
            }
            using (var psSr = new StreamReader(pixelShaderStream))
            {
                psSource = psSr.ReadToEnd();
            }

            CompilationResult vsCompilation = ShaderBytecode.Compile(vsSource, "VS", "vs_5_0", defaultShaderFlags, sourceFileName: vertexShaderName);
            CompilationResult psCompilation = ShaderBytecode.Compile(psSource, "PS", "ps_5_0", defaultShaderFlags, sourceFileName: pixelShaderName);

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

            ShaderReflection vsReflection = new ShaderReflection(vsCompilation.Bytecode.Data);
            ShaderReflection psReflection = new ShaderReflection(psCompilation.Bytecode.Data);

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
                bool vsBuffer = DoesConstantBufferExist(vsReflection, i, genericElement.Name);
                bool psBuffer = DoesConstantBufferExist(psReflection, i, genericElement.Name);

                if (genericElement.UseGlobalNamedBuffer)
                {
                    pair = rc.GetNamedGlobalBufferProviderPair(genericElement.GlobalProviderName);
                    cbb = new GlobalConstantBufferBinding(i, pair, false, vsBuffer, psBuffer);
                }
                else
                {
                    D3DConstantBuffer constantBuffer = new D3DConstantBuffer(device, genericElement.DataProvider.DataSizeInBytes);
                    pair = new BufferProviderPair(constantBuffer, genericElement.DataProvider);
                    cbb = new GlobalConstantBufferBinding(i, pair, true, vsBuffer, psBuffer);
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
                int bufferSlot = i + numGlobalElements;
                bool vsBuffer = DoesConstantBufferExist(vsReflection, bufferSlot, genericElement.Name);
                bool psBuffer = DoesConstantBufferExist(psReflection, bufferSlot, genericElement.Name);
                D3DConstantBuffer constantBuffer = new D3DConstantBuffer(device, genericElement.BufferSizeInBytes);

                PerObjectConstantBufferBinding pocbb = new PerObjectConstantBufferBinding(bufferSlot, constantBuffer, vsBuffer, psBuffer);
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

        private static bool DoesConstantBufferExist(ShaderReflection reflection, int slot, string name)
        {
            InputBindingDescription bindingDesc;
            try
            {
                bindingDesc = reflection.GetResourceBindingDescription(name);

                if (bindingDesc.BindPoint != slot)
                {
                    throw new InvalidOperationException(string.Format("Mismatched binding slot. Expected: {0}, Actual: {1}", slot, bindingDesc.BindPoint));
                }

                return true;
            }
            catch (SharpDX.SharpDXException)
            {
                for (int i = 0; i < reflection.Description.BoundResources; i++)
                {
                    var desc = reflection.GetResourceBindingDescription(i);
                    if (desc.Type == ShaderInputType.ConstantBuffer && desc.BindPoint == slot)
                    {
                        Console.WriteLine("Buffer in slot " + slot + " has wrong name. Expected: " + name + ", Actual: " + desc.Name);
                        bindingDesc = desc;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsValid(SharpDX.D3DCompiler.ConstantBuffer buffer)
        {
            try
            {
                var desc = buffer.Description;
                return true;
            }
            catch
            {
                return false;
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
                cbBinding.BindToShaderSlots(_device.ImmediateContext);
            }

            for (int i = 0; i < _perObjectBufferBindings.Length; i++)
            {
                PerObjectConstantBufferBinding binding = _perObjectBufferBindings[i];
                binding.BindToShaderSlots(_device.ImmediateContext);
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

        private static InputLayout CreateLayout(Device device, byte[] shaderBytecode, MaterialVertexInput[] vertexInputs)
        {
            int count = vertexInputs.Sum(mvi => mvi.Elements.Length);
            int element = 0;
            InputElement[] elements = new InputElement[count];
            for (int slot = 0; slot < vertexInputs.Length; slot++)
            {
                MaterialVertexInput slotInput = vertexInputs[slot];
                int numElements = slotInput.Elements.Length;
                int currentOffset = 0;
                for (int i = 0; i < numElements; i++)
                {
                    var genericElement = slotInput.Elements[i];
                    elements[element] = new InputElement(
                        GetSemanticName(genericElement.SemanticType),
                        0,
                        ConvertGenericFormat(genericElement.ElementFormat),
                        currentOffset,
                        slot);
                    currentOffset += genericElement.SizeInBytes;
                    element += 1;
                }
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
                if (binding.IsLocalBinding) // Do not dispose shared bindings.
                {
                    binding.ConstantBuffer.Dispose();
                }
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
            public readonly bool IsLocalBinding;
            private readonly bool _isVertexShaderBuffer;
            private readonly bool _isPixelShaderBuffer;

            public int Slot { get; }
            public BufferProviderPair Pair { get; }
            public D3DConstantBuffer ConstantBuffer => (D3DConstantBuffer)Pair.ConstantBuffer;

            public GlobalConstantBufferBinding(int slot, BufferProviderPair pair, bool isLocalBinding, bool isVertexBuffer, bool isPixelShader)
            {
                Slot = slot;
                Pair = pair;
                IsLocalBinding = isLocalBinding;
                _isVertexShaderBuffer = isVertexBuffer;
                _isPixelShaderBuffer = isPixelShader;
            }

            public void UpdateBuffer()
            {
                if (IsLocalBinding)
                {
                    Pair.UpdateData();
                }
            }

            public void BindToShaderSlots(DeviceContext dc)
            {
                if (_isVertexShaderBuffer)
                {
                    dc.VertexShader.SetConstantBuffer(Slot, ConstantBuffer.Buffer);
                }
                if (_isPixelShaderBuffer)
                {
                    dc.PixelShader.SetConstantBuffer(Slot, ConstantBuffer.Buffer);
                }
            }
        }

        private struct PerObjectConstantBufferBinding
        {
            private readonly bool _isVertexShaderBuffer;
            private readonly bool _isPixelShaderBuffer;

            public int Slot { get; }
            public D3DConstantBuffer ConstantBuffer { get; }

            public PerObjectConstantBufferBinding(int slot, D3DConstantBuffer constantBuffer, bool isVertexBuffer, bool isPixelShader)
            {
                Slot = slot;
                ConstantBuffer = constantBuffer;
                _isVertexShaderBuffer = isVertexBuffer;
                _isPixelShaderBuffer = isPixelShader;
            }

            public void BindToShaderSlots(DeviceContext dc)
            {
                if (_isVertexShaderBuffer)
                {
                    dc.VertexShader.SetConstantBuffer(Slot, ConstantBuffer.Buffer);
                }
                if (_isPixelShaderBuffer)
                {
                    dc.PixelShader.SetConstantBuffer(Slot, ConstantBuffer.Buffer);
                }
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