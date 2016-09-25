using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;

namespace Veldrid.Graphics.Direct3D
{
    public class D3DShaderConstantBindings : ShaderConstantBindings
    {
        private readonly Device _device;
        private readonly GlobalConstantBufferBinding[] _constantBufferBindings;
        private readonly PerObjectConstantBufferBinding[] _perObjectBufferBindings;

        public void Apply()
        {
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

        public D3DShaderConstantBindings(
            RenderContext rc,
            Device device,
            ShaderSet shaderSet,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs)
        {
            _device = device;

            ShaderReflection vsReflection = new ShaderReflection(((D3DVertexShader)shaderSet.VertexShader).Bytecode.Data);
            ShaderReflection psReflection = new ShaderReflection(((D3DFragmentShader)shaderSet.FragmentShader).Bytecode.Data);

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

        public void Dispose()
        {
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
    }
}