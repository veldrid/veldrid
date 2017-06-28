using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLTextureBindingSlots : ShaderTextureBindingSlots
    {
        public MaterialTextureInputs TextureInputs { get; }

        private readonly OpenGLProgramTextureBinding[] _textureBindings;

        public OpenGLTextureBindingSlots(ShaderSet shaderSet, MaterialTextureInputs textureInputs)
        {
            TextureInputs = textureInputs;

            _textureBindings = new OpenGLProgramTextureBinding[textureInputs.Elements.Length];
            for (int i = 0; i < textureInputs.Elements.Length; i++)
            {
                MaterialTextureInputElement element = textureInputs.Elements[i];
                int location = GL.GetUniformLocation(((OpenGLShaderSet)shaderSet).ProgramID, element.Name);
                if (location == -1)
                {
                    throw new InvalidOperationException($"No sampler was found with the name {element.Name}");
                }

                _textureBindings[i] = new OpenGLProgramTextureBinding(location);
            }
        }

        public int GetUniformLocation(int slot)
        {
            if (slot < 0 || slot >= _textureBindings.Length)
            {
                throw new ArgumentOutOfRangeException($"Invalid slot:{slot}. Valid range:{0}-{_textureBindings.Length - 1}.");
            }

            return _textureBindings[slot].UniformLocation;
        }

        private struct OpenGLProgramTextureBinding
        {
            public readonly int UniformLocation;

            public OpenGLProgramTextureBinding(int location)
            {
                UniformLocation = location;
            }
        }
    }
}