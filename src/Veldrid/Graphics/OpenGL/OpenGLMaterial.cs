//using OpenTK.Graphics.OpenGL;
//using System;
//using System.Diagnostics;
//using System.Linq;

//namespace Veldrid.Graphics.OpenGL
//{
//    public class OpenGLMaterial : Material, IDisposable
//    {
//        private readonly ShaderSet _shaderSet;
//        private readonly OpenGLVertexInputLayout _inputLayout;
//        private readonly ShaderConstantBindings _constantBindings;
//        private readonly OpenGLProgramTextureBinding[] _textureBindings;

//        private static int s_vertexAttribSlotsBound = 0;

//        public OpenGLMaterial(
//            OpenGLRenderContext rc,
//            ShaderSet shaderSet,
//            VertexInputLayout inputLayout,
//            ShaderConstantBindings constantBindings,
//            MaterialTextureInputs textureInputs)
//        {
//            _shaderSet = shaderSet;
//            _inputLayout = (OpenGLVertexInputLayout)inputLayout;
//            _constantBindings = constantBindings;

//            _textureBindings = new OpenGLProgramTextureBinding[textureInputs.Elements.Length];
//            for (int i = 0; i < textureInputs.Elements.Length; i++)
//            {
//                var element = textureInputs.Elements[i];
//                int location = GL.GetUniformLocation(((OpenGLShaderSet)shaderSet).ProgramID, element.Name);
//                if (location == -1)
//                {
//                    throw new InvalidOperationException($"No sampler was found with the name {element.Name}");
//                }
//                OpenGLTexture2D deviceTexture = (OpenGLTexture2D)element.GetDeviceTexture(rc);
//                _textureBindings[i] = new OpenGLProgramTextureBinding(location, deviceTexture);
//            }
//        }

//        private OpenGLMaterialVertexInput[] GetInputsBySlot(MaterialVertexInput[] vertexInputs)
//        {
//            return vertexInputs.Select(mvi => new OpenGLMaterialVertexInput(mvi)).ToArray();
//        }

        

//        public void Apply(VertexBuffer[] vertexBuffers)
//        {
//            int totalSlotsBound = 0;
//            for (int i = 0; i < _inputLayout.VBLayoutsBySlot.Length; i++)
//            {
//                OpenGLMaterialVertexInput input = _inputLayout.VBLayoutsBySlot[i];
//                ((OpenGLVertexBuffer)vertexBuffers[i]).Apply();
//                for (int slot = 0; slot < input.Elements.Length; slot++)
//                {
//                    OpenGLMaterialVertexInputElement element = input.Elements[slot];
//                    int actualSlot = totalSlotsBound + slot;
//                    GL.EnableVertexAttribArray(actualSlot);
//                    GL.VertexAttribPointer(actualSlot, element.ElementCount, element.Type, element.Normalized, input.VertexSizeInBytes, element.Offset);
//                    GL.VertexAttribDivisor(actualSlot, element.InstanceStepRate);
//                }

//                totalSlotsBound += input.Elements.Length;
//            }

//            for (int extraSlot = totalSlotsBound; extraSlot < s_vertexAttribSlotsBound; extraSlot++)
//            {
//                GL.DisableVertexAttribArray(extraSlot);
//            }

//            s_vertexAttribSlotsBound = totalSlotsBound;

//            GL.UseProgram(((OpenGLShaderSet)_shaderSet).ProgramID);

//            _constantBindings.Apply();

//            ApplyDefaultTextureBindings();
//        }

//        private void ApplyDefaultTextureBindings()
//        {
//            for (int i = 0; i < _textureBindings.Length; i++)
//            {
//                var binding = _textureBindings[i];
//                if (binding.DeviceTexture != null)
//                {
//                    GL.ActiveTexture(TextureUnit.Texture0 + i);
//                    binding.DeviceTexture.Bind();
//                    GL.Uniform1(binding.UniformLocation, i);
//                }
//            }
//        }

//        public void ApplyPerObjectInput(ConstantBufferDataProvider dataProvider)
//        {
//            _constantBindings.ApplyPerObjectInput(dataProvider);
//        }

//        public void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders)
//        {
//            _constantBindings.ApplyPerObjectInputs(dataProviders);
//        }

//        public void UseDefaultTextures()
//        {
//            ApplyDefaultTextureBindings();
//        }

//        public void UseTexture(int slot, ShaderTextureBinding binding)
//        {
//            if (!(binding is OpenGLTextureBinding))
//            {
//                throw new InvalidOperationException("Illegal binding type.");
//            }

//            if (binding.BoundTexture != null)
//            {
//                BindTexture(slot, (OpenGLTexture)binding.BoundTexture);
//            }
//        }

//        public void SetVertexAttributes(int vertexBufferSlot, OpenGLVertexBuffer vb)
//        {
//            // TODO: Related to OpenGLRenderContext.PlatformSetVertexBuffer()
//            // These attributes should be lazily set on a draw call or something.
//            if (vertexBufferSlot <= _inputLayout.VBLayoutsBySlot.Length)
//            {
//                return;
//            }

//            int baseSlot = GetSlotBaseIndex(vertexBufferSlot);
//            OpenGLMaterialVertexInput input = _inputLayout.VBLayoutsBySlot[vertexBufferSlot];
//            vb.Apply();
//            for (int i = 0; i < input.Elements.Length; i++)
//            {
//                OpenGLMaterialVertexInputElement element = input.Elements[i];
//                int slot = baseSlot + i;
//                GL.EnableVertexAttribArray(slot);
//                GL.VertexAttribPointer(slot, element.ElementCount, element.Type, element.Normalized, input.VertexSizeInBytes, element.Offset);
//            }
//        }

//        private int GetSlotBaseIndex(int vertexBufferSlot)
//        {
//            int index = 0;
//            for (int i = 0; i < vertexBufferSlot; i++)
//            {
//                index += _inputLayout.VBLayoutsBySlot[i].Elements.Length;
//            }

//            return index;
//        }

//        private void BindTexture(int slot, OpenGLTexture texture)
//        {
//            GL.ActiveTexture(TextureUnit.Texture0 + slot);
//            texture.Bind();
//            GL.Uniform1(GetTextureUniformLocation(slot), slot);
//        }

//        private int GetTextureUniformLocation(int slot)
//        {
//            if (_textureBindings.Length <= slot)
//            {
//                throw new InvalidOperationException("Illegal slot value. There are only  " + _textureBindings.Length + " texture bindings.");
//            }

//            return _textureBindings[slot].UniformLocation;
//        }

//        public void Dispose()
//        {
//            _shaderSet.Dispose();
//            _inputLayout.Dispose();
//            _constantBindings.Dispose();

//            foreach (var textureBinding in _textureBindings)
//            {
//                textureBinding.DeviceTexture?.Dispose();
//            }
//        }

//        private struct OpenGLProgramTextureBinding
//        {
//            public readonly int UniformLocation;
//            public readonly OpenGLTexture DeviceTexture;

//            public OpenGLProgramTextureBinding(int location, OpenGLTexture deviceTexture)
//            {
//                UniformLocation = location;
//                DeviceTexture = deviceTexture;
//            }
//        }
//    }
//}
