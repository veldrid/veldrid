using System;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using System.IO;

namespace Veldrid.Graphics.Direct3D
{
    internal class D3DMaterial : Material
    {
        private readonly Device _device;
        private readonly VertexShader _vertexShader;
        private readonly PixelShader _pixelShader;
        private readonly InputLayout _inputLayout;
        private readonly ConstantBufferBinding[] _constantBufferBindings;
        private readonly ConstantBufferBinding[] _perObjectBufferBindings;
        private readonly ResourceViewBinding[] _resourceViewBindings;

        private const ShaderFlags defaultShaderFlags
#if DEBUG
            = ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#else
            = ShaderFlags.None;
#endif

        public D3DMaterial(
            Device device,
            D3DResourceFactory resourceFactory,
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
                ? new ConstantBufferBinding[numGlobalElements]
                : Array.Empty<ConstantBufferBinding>();
            for (int i = 0; i < numGlobalElements; i++)
            {
                var genericElement = globalInputs.Elements[i];
                D3DConstantBuffer constantBuffer = new D3DConstantBuffer(device, genericElement.DataProvider.DataSizeInBytes);
                _constantBufferBindings[i] = new ConstantBufferBinding(i, constantBuffer, genericElement.DataProvider);
            }

            int numPerObjectInputs = perObjectInputs.Elements.Length;
            _perObjectBufferBindings =
                (numPerObjectInputs > 0)
                ? new ConstantBufferBinding[numPerObjectInputs]
                : Array.Empty<ConstantBufferBinding>();
            for (int i = 0; i < numPerObjectInputs; i++)
            {
                var genericElement = perObjectInputs.Elements[i];
                D3DConstantBuffer constantBuffer = new D3DConstantBuffer(device, genericElement.BufferSizeInBytes);
                _perObjectBufferBindings[i] = new ConstantBufferBinding(i + numGlobalElements, constantBuffer, null); // TODO: Fix this, shouldn't pass null.
            }

            int numTextures = textureInputs.Elements.Length;
            _resourceViewBindings = new ResourceViewBinding[numTextures];
            for (int i = 0; i < numTextures; i++)
            {
                var genericElement = textureInputs.Elements[i];
                D3DTexture texture = (D3DTexture)genericElement.TextureData.CreateDeviceTexture(resourceFactory);
                ShaderResourceView resourceView = new ShaderResourceView(device, texture.DeviceTexture);
                _resourceViewBindings[i] = new ResourceViewBinding(i, resourceView);
            }
        }


        public void Apply()
        {
            _device.ImmediateContext.InputAssembler.InputLayout = _inputLayout;
            _device.ImmediateContext.VertexShader.Set(_vertexShader);
            _device.ImmediateContext.PixelShader.Set(_pixelShader);

            foreach (ConstantBufferBinding cbBinding in _constantBufferBindings)
            {
                cbBinding.DataProvider.SetData(cbBinding.ConstantBuffer);
                _device.ImmediateContext.VertexShader.SetConstantBuffer(cbBinding.Slot, cbBinding.ConstantBuffer.Buffer);
                _device.ImmediateContext.PixelShader.SetConstantBuffer(cbBinding.Slot, cbBinding.ConstantBuffer.Buffer);
            }

            foreach (ResourceViewBinding rvBinding in _resourceViewBindings)
            {
                _device.ImmediateContext.PixelShader.SetShaderResource(rvBinding.Slot, rvBinding.ResourceView);
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

            ConstantBufferBinding cbBinding = _perObjectBufferBindings[0];
            dataProvider.SetData(cbBinding.ConstantBuffer);
            _device.ImmediateContext.VertexShader.SetConstantBuffer(cbBinding.Slot, cbBinding.ConstantBuffer.Buffer);
            _device.ImmediateContext.PixelShader.SetConstantBuffer(cbBinding.Slot, cbBinding.ConstantBuffer.Buffer);
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
                ConstantBufferBinding cbBinding = _perObjectBufferBindings[i];
                ConstantBufferDataProvider provider = dataProviders[i];

                provider.SetData(cbBinding.ConstantBuffer);
                _device.ImmediateContext.VertexShader.SetConstantBuffer(cbBinding.Slot, cbBinding.ConstantBuffer.Buffer);
                _device.ImmediateContext.PixelShader.SetConstantBuffer(cbBinding.Slot, cbBinding.ConstantBuffer.Buffer);
            }
        }

        private struct ConstantBufferBinding
        {
            public int Slot { get; }
            public D3DConstantBuffer ConstantBuffer { get; }
            public ConstantBufferDataProvider DataProvider { get; }

            public ConstantBufferBinding(int slot, D3DConstantBuffer buffer, ConstantBufferDataProvider dataProvider)
            {
                Slot = slot;
                ConstantBuffer = buffer;
                DataProvider = dataProvider;
            }
        }

        private struct ResourceViewBinding
        {
            public int Slot { get; }
            public ShaderResourceView ResourceView { get; }

            public ResourceViewBinding(int slot, ShaderResourceView resourceView)
            {
                Slot = slot;
                ResourceView = resourceView;
            }
        }
    }
}