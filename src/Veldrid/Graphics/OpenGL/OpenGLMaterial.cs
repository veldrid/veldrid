using OpenTK.Graphics.OpenGL;
using System;
using System.Diagnostics;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLMaterial : Material, IDisposable
    {
        private readonly OpenGLShader _vertexShader;
        private readonly OpenGLShader _fragmentShader;
        private readonly int _programID;
        private readonly OpenGLMaterialVertexInput _inputs;
        private readonly GlobalBindingPair[] _globalUniformBindings;
        private readonly UniformBinding[] _perObjectBindings;
        private readonly OpenGLProgramTextureBinding[] _textureBindings;

        private static int s_vertexAttribSlotsBound = 0;

        public OpenGLMaterial(
            OpenGLRenderContext rc,
            OpenGLShader vertexShader,
            OpenGLShader fragmentShader,
            MaterialVertexInput vertexInputs,
            MaterialInputs<MaterialGlobalInputElement> globalInputs,
            MaterialInputs<MaterialPerObjectInputElement> perObjectInputs,
            MaterialTextureInputs textureInputs)
        {
            _vertexShader = vertexShader;
            _fragmentShader = fragmentShader;
            _inputs = new OpenGLMaterialVertexInput(vertexInputs);

            _programID = GL.CreateProgram();
            GL.AttachShader(_programID, _vertexShader.ShaderID);
            GL.AttachShader(_programID, _fragmentShader.ShaderID);

            for (int slot = 0; slot < vertexInputs.Elements.Length; slot++)
            {
                GL.BindAttribLocation(_programID, slot, vertexInputs.Elements[slot].Name);
            }

            GL.LinkProgram(_programID);

            int linkStatus;
            GL.GetProgram(_programID, GetProgramParameterName.LinkStatus, out linkStatus);
            if (linkStatus != 1)
            {
                string log = GL.GetProgramInfoLog(_programID);
                throw new InvalidOperationException($"Error linking GL program: {log}");
            }

            int globalInputsCount = globalInputs.Elements.Length;
            int bindingIndex = 0;
            _globalUniformBindings = new GlobalBindingPair[globalInputsCount];
            for (int i = 0; i < globalInputsCount; i++)
            {
                var element = globalInputs.Elements[i];

                int blockIndex = GL.GetUniformBlockIndex(_programID, element.Name);
                if (blockIndex != -1)
                {
                    _globalUniformBindings[i] = new GlobalBindingPair(
                        new UniformBlockBinding(
                            _programID,
                            blockIndex,
                            bindingIndex,
                            new OpenGLConstantBuffer(element.DataProvider)),
                        element.DataProvider);
                    bindingIndex += 1;
                }
                else
                {
                    int uniformLocation = GL.GetUniformLocation(_programID, element.Name);
                    if (uniformLocation == -1)
                    {
                        throw new InvalidOperationException($"No uniform or uniform block with name {element.Name} was found.");
                    }

                    _globalUniformBindings[i] = new GlobalBindingPair(
                        new UniformLocationBinding(
                            _programID,
                            uniformLocation),
                        element.DataProvider);
                }
            }

            int perObjectInputsCount = perObjectInputs.Elements.Length;
            _perObjectBindings = new UniformBinding[perObjectInputsCount];
            for (int i = 0; i < perObjectInputsCount; i++)
            {
                var element = perObjectInputs.Elements[i];

                int blockIndex = GL.GetUniformBlockIndex(_programID, element.Name);
                if (blockIndex != -1)
                {
                    _perObjectBindings[i] = new UniformBlockBinding(
                            _programID,
                            blockIndex,
                            bindingIndex,
                            new OpenGLConstantBuffer());
                    bindingIndex += 1;
                }
                else
                {
                    int uniformLocation = GL.GetUniformLocation(_programID, element.Name);
                    if (uniformLocation == -1)
                    {
                        throw new InvalidOperationException($"No uniform or uniform block with name {element.Name} was found.");
                    }

                    _perObjectBindings[i] = new UniformLocationBinding(
                        _programID,
                        uniformLocation);
                }
            }

            _textureBindings = new OpenGLProgramTextureBinding[textureInputs.Elements.Length];
            for (int i = 0; i < textureInputs.Elements.Length; i++)
            {
                var element = textureInputs.Elements[i];
                int location = GL.GetUniformLocation(_programID, element.Name);
                if (location == -1)
                {
                    throw new InvalidOperationException($"No sampler was found with the name {element.Name}");
                }
                OpenGLTexture deviceTexture = (OpenGLTexture)element.GetDeviceTexture(rc);
                _textureBindings[i] = new OpenGLProgramTextureBinding(location, deviceTexture);
            }
        }

        public void Apply()
        {
            for (int slot = 0; slot < _inputs.Elements.Length; slot++)
            {
                var element = _inputs.Elements[slot];

                GL.EnableVertexAttribArray(slot);
                GL.VertexAttribPointer(slot, element.ElementCount, element.Type, element.Normalized, _inputs.VertexSizeInBytes, element.Offset);
            }

            for (int extraSlot = _inputs.Elements.Length; extraSlot < s_vertexAttribSlotsBound; extraSlot++)
            {
                GL.DisableVertexAttribArray(extraSlot);
            }

            s_vertexAttribSlotsBound = _inputs.Elements.Length;

            GL.UseProgram(_programID);

            foreach (var globalBinding in _globalUniformBindings)
            {
                globalBinding.Bind();
            }

            for (int i = 0; i < _textureBindings.Length; i++)
            {
                var binding = _textureBindings[i];
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                binding.DeviceTexture.Apply();
                GL.Uniform1(binding.UniformLocation, i);
            }
        }

        public void ApplyPerObjectInput(ConstantBufferDataProvider dataProvider)
        {
            _perObjectBindings[0].Bind(dataProvider);
        }

        public void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders)
        {
            for (int i = 0; i < dataProviders.Length; i++)
            {
                _perObjectBindings[i].Bind(dataProviders[i]);
            }
        }

        public void UseTexture(int slot, ShaderTextureBinding binding)
        {
            if (!(binding is OpenGLTextureBinding))
            {
                throw new InvalidOperationException("Illegal binding type.");
            }

            BindTexture(slot, (OpenGLTexture)binding.BoundTexture);
        }

        private void BindTexture(int slot, OpenGLTexture texture)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            texture.Apply();
            GL.Uniform1(GetTextureUniformLocation(slot), slot);
        }

        private int GetTextureUniformLocation(int slot)
        {
            if (_textureBindings.Length <= slot)
            {
                throw new InvalidOperationException("Illegal slot value. There are only  " + _textureBindings.Length + " texture bindings.");
            }

            return _textureBindings[slot].UniformLocation;
        }

        public void Dispose()
        {
            GL.DeleteProgram(_programID);
            _vertexShader.Dispose();
            _fragmentShader.Dispose();

            foreach (var textureBinding in _textureBindings)
            {
                textureBinding.DeviceTexture.Dispose();
            }

            foreach (var globalBinding in _globalUniformBindings)
            {
                globalBinding.Binding.Dispose();
            }

            foreach (var perObjectbinding in _perObjectBindings)
            {
                perObjectbinding.Dispose();
            }
        }

        private abstract class UniformBinding : IDisposable
        {
            public int ProgramID { get; }

            public UniformBinding(int programID)
            {
                ProgramID = programID;
            }

            public abstract void Bind(ConstantBufferDataProvider dataProvider);
            public abstract void Dispose();
        }

        [DebuggerDisplay("Prog:{ProgramID} BlockInd:{BlockIndex} BindingInd:{BindingIndex}")]
        private class UniformBlockBinding : UniformBinding
        {
            public int BlockIndex { get; }
            public int BindingIndex { get; }
            public OpenGLConstantBuffer ConstantBuffer { get; }

            public UniformBlockBinding(
                int programID,
                int blockIndex,
                int bindingIndex,
                OpenGLConstantBuffer constantBuffer)
                : base(programID)
            {
                BlockIndex = blockIndex;
                BindingIndex = bindingIndex;
                ConstantBuffer = constantBuffer;
            }

            public override void Bind(ConstantBufferDataProvider dataProvider)
            {
                dataProvider.SetData(ConstantBuffer);
                ConstantBuffer.BindToBlock(ProgramID, BlockIndex, dataProvider.DataSizeInBytes, BindingIndex);
            }

            public override void Dispose()
            {
                ConstantBuffer.Dispose();
            }
        }

        private class UniformLocationBinding : UniformBinding
        {
            public OpenGLUniformStorageAdapter StorageAdapter { get; }

            public UniformLocationBinding(
                int programID,
                int uniformLocation) : base(programID)
            {
                StorageAdapter = new OpenGLUniformStorageAdapter(ProgramID, uniformLocation);
            }

            public override void Bind(ConstantBufferDataProvider dataProvider)
            {
                dataProvider.SetData(StorageAdapter);
            }

            public override void Dispose()
            {
            }
        }

        private struct GlobalBindingPair
        {
            public UniformBinding Binding { get; }
            public ConstantBufferDataProvider DataProvider { get; }

            public GlobalBindingPair(UniformBinding binding, ConstantBufferDataProvider dataProvider)
            {
                Binding = binding;
                DataProvider = dataProvider;
            }

            public void Bind()
            {
                Binding.Bind(DataProvider);
            }
        }

        private class OpenGLMaterialVertexInput
        {
            public int VertexSizeInBytes { get; }
            public OpenGLMaterialVertexInputElement[] Elements { get; }

            public OpenGLMaterialVertexInput(int vertexSizeInBytes, OpenGLMaterialVertexInputElement[] elements)
            {
                VertexSizeInBytes = vertexSizeInBytes;
                Elements = elements;
            }

            public OpenGLMaterialVertexInput(MaterialVertexInput genericInput)
            {
                VertexSizeInBytes = genericInput.VertexSizeInBytes;
                Elements = new OpenGLMaterialVertexInputElement[genericInput.Elements.Length];
                int offset = 0;
                for (int i = 0; i < Elements.Length; i++)
                {
                    var genericElement = genericInput.Elements[i];
                    Elements[i] = new OpenGLMaterialVertexInputElement(genericElement, offset);
                    offset += genericElement.SizeInBytes;
                }
            }
        }

        private struct OpenGLMaterialVertexInputElement
        {
            public byte SizeInBytes { get; }
            public byte ElementCount { get; }
            public VertexAttribPointerType Type { get; }
            public int Offset { get; }
            public bool Normalized { get; }

            public OpenGLMaterialVertexInputElement(byte sizeInBytes, byte elementCount, VertexAttribPointerType type, int offset, bool normalized)
            {
                SizeInBytes = sizeInBytes;
                ElementCount = elementCount;
                Type = type;
                Offset = offset;
                Normalized = normalized;
            }

            public OpenGLMaterialVertexInputElement(MaterialVertexInputElement genericElement, int offset)
            {
                SizeInBytes = genericElement.SizeInBytes;
                ElementCount = VertexFormatHelpers.GetElementCount(genericElement.ElementFormat);
                Type = GetGenericFormatType(genericElement.ElementFormat);
                Offset = offset;
                Normalized = genericElement.SemanticType == VertexSemanticType.Color && genericElement.ElementFormat == VertexElementFormat.Byte4;
            }

            private static VertexAttribPointerType GetGenericFormatType(VertexElementFormat format)
            {
                switch (format)
                {
                    case VertexElementFormat.Float1:
                    case VertexElementFormat.Float2:
                    case VertexElementFormat.Float3:
                    case VertexElementFormat.Float4:
                        return VertexAttribPointerType.Float;
                    case VertexElementFormat.Byte1:
                    case VertexElementFormat.Byte4:
                        return VertexAttribPointerType.UnsignedByte;
                    default:
                        throw Illegal.Value<VertexElementFormat>();
                }
            }
        }

        private struct OpenGLProgramTextureBinding
        {
            public readonly int UniformLocation;
            public readonly OpenGLTexture DeviceTexture;

            public OpenGLProgramTextureBinding(int location, OpenGLTexture deviceTexture)
            {
                UniformLocation = location;
                DeviceTexture = deviceTexture;
            }
        }
    }
}
