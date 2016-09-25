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
        private readonly ShaderSet _shaderSet;
        private readonly VertexInputLayout _inputLayout;
        private readonly ShaderConstantBindings _constantBindings;

        private readonly ResourceViewBinding?[] _resourceViewBindings;
        private readonly ShaderTextureBinding[] _currentTextureBindings;

        public D3DMaterial(
            Device device,
            D3DRenderContext rc,
            VertexInputLayout inputLayout,
            ShaderSet shaderSet,
            ShaderConstantBindings constantBindings,
            MaterialTextureInputs textureInputs)
        {
            _device = device;
            _inputLayout = inputLayout;
            _shaderSet = shaderSet;
            _constantBindings = constantBindings;

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
            _device.ImmediateContext.InputAssembler.InputLayout = ((D3DVertexInputLayout)_inputLayout).DeviceLayout;
            _device.ImmediateContext.VertexShader.Set(((D3DVertexShader)_shaderSet.VertexShader).DeviceShader);
            _device.ImmediateContext.PixelShader.Set(((D3DFragmentShader)_shaderSet.FragmentShader).DeviceShader);
            _constantBindings.Apply();
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
            SemanticIndices indicesTracker = new SemanticIndices();
            for (int vbSlot = 0; vbSlot < vertexInputs.Length; vbSlot++)
            {
                MaterialVertexInput bufferInput = vertexInputs[vbSlot];
                int numElements = bufferInput.Elements.Length;
                int currentOffset = 0;
                for (int i = 0; i < numElements; i++)
                {
                    var genericElement = bufferInput.Elements[i];
                    elements[element] = new InputElement(
                        GetSemanticName(genericElement.SemanticType),
                        indicesTracker.GetAndIncrement(genericElement.SemanticType),
                        ConvertGenericFormat(genericElement.ElementFormat),
                        currentOffset,
                        vbSlot,
                        D3DFormats.ConvertInputClass(genericElement.StorageClassifier),
                        genericElement.InstanceStepRate);
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
            ((D3DShaderConstantBindings)_constantBindings).ApplyPerObjectInput(dataProvider);
        }

        public void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders)
        {
            ((D3DShaderConstantBindings)_constantBindings).ApplyPerObjectInputs(dataProviders);
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
            _shaderSet.Dispose();
            _inputLayout.Dispose();
            _constantBindings.Dispose();
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

        private class SemanticIndices
        {
            private int _position;
            private int _texCoord;
            private int _normal;
            private int _color;

            public int GetAndIncrement(VertexSemanticType type)
            {
                switch (type)
                {
                    case VertexSemanticType.Position:
                        return _position++;
                    case VertexSemanticType.TextureCoordinate:
                        return _texCoord++;
                    case VertexSemanticType.Normal:
                        return _normal++;
                    case VertexSemanticType.Color:
                        return _color++;
                    default:
                        throw Illegal.Value<VertexSemanticType>();
                }
            }
        }
    }
}