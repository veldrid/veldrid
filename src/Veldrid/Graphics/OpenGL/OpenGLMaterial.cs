using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLMaterial : Material, IDisposable
    {
        private readonly OpenGLShader _vertexShader;
        private readonly OpenGLShader _fragmentShader;
        private readonly int _programID;
        private readonly OpenGLMaterialVertexInput _inputs;
        private readonly MaterialGlobalInputs _globalInputs;
        private readonly int[] _uniformBlocks;
        private readonly OpenGLConstantBuffer[] _constantBuffers;
        private readonly OpenGLProgramTextureBinding[] _textureBindings;

        public OpenGLMaterial(
            OpenGLShader vertexShader,
            OpenGLShader fragmentShader,
            MaterialVertexInput vertexInputs,
            MaterialGlobalInputs globalInputs,
            MaterialTextureInputs textureInputs)
        {
            _vertexShader = vertexShader;
            _fragmentShader = fragmentShader;
            _inputs = new OpenGLMaterialVertexInput(vertexInputs);
            _globalInputs = globalInputs;

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
            _uniformBlocks = new int[globalInputsCount];
            _constantBuffers = new OpenGLConstantBuffer[globalInputsCount];
            for (int i = 0; i < globalInputsCount; i++)
            {
                var element = globalInputs.Elements[i];
                _uniformBlocks[i] = GL.GetUniformBlockIndex(_programID, element.Name);
                _constantBuffers[i] = new OpenGLConstantBuffer(element.DataProvider);
            }

            _textureBindings = new OpenGLProgramTextureBinding[textureInputs.Elements.Length];
            for (int i = 0; i < textureInputs.Elements.Length; i++)
            {
                var element = textureInputs.Elements[i];
                int location = GL.GetUniformLocation(_programID, element.Name);
                OpenGLTextureBuffer textureBuffer = new OpenGLTextureBuffer(element.Texture);
                _textureBindings[i] = new OpenGLProgramTextureBinding(location, textureBuffer);
            }
        }

        public void Apply()
        {
            for (int slot = 0; slot < _inputs.Elements.Length; slot++)
            {
                var element = _inputs.Elements[slot];

                GL.EnableVertexAttribArray(slot);
                GL.VertexAttribPointer(slot, element.ElementCount, element.Type, false, _inputs.VertexSizeInBytes, element.Offset);
            }

            GL.UseProgram(_programID);

            for (int i = 0; i < _globalInputs.Elements.Length; i++)
            {
                _globalInputs.Elements[i].DataProvider.SetData(_constantBuffers[i]);
                _constantBuffers[i].BindToBlock(_programID, _uniformBlocks[i], _globalInputs.Elements[i].DataProvider.DataSizeInBytes, i);
            }

            for (int i = 0; i < _textureBindings.Length; i++)
            {
                var binding = _textureBindings[i];
                GL.ActiveTexture(TextureUnit.Texture0 + i);
                GL.BindTexture(TextureTarget.Texture2D, binding.TextureBuffer.TextureID);
                GL.Uniform1(binding.UniformLocation, i);
            }
        }

        public void Dispose()
        {
            GL.DeleteProgram(_programID);
            _vertexShader.Dispose();
            _fragmentShader.Dispose();

            // TODO: Other things need to be disposed.
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

            public OpenGLMaterialVertexInputElement(byte sizeInBytes, byte elementCount, VertexAttribPointerType type, int offset)
            {
                SizeInBytes = sizeInBytes;
                ElementCount = elementCount;
                Type = type;
                Offset = offset;
            }

            public OpenGLMaterialVertexInputElement(MaterialVertexInputElement genericElement, int offset)
            {
                SizeInBytes = genericElement.SizeInBytes;
                ElementCount = VertexFormatHelpers.GetElementCount(genericElement.ElementFormat);
                Type = GetGenericFormatType(genericElement.ElementFormat);
                Offset = offset;
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
                    default:
                        throw new InvalidOperationException("Invalid format : " + format);
                }
            }
        }

        private struct OpenGLProgramTextureBinding
        {
            public readonly int UniformLocation;
            public readonly OpenGLTextureBuffer TextureBuffer;

            public OpenGLProgramTextureBinding(int location, OpenGLTextureBuffer textureBuffer)
            {
                UniformLocation = location;
                TextureBuffer = textureBuffer;
            }
        }
    }
}
