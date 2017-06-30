using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLShaderSet : ShaderSet
    {
        private readonly Dictionary<int, OpenGLConstantBuffer> _boundConstantBuffers = new Dictionary<int, OpenGLConstantBuffer>();

        public OpenGLVertexInputLayout InputLayout { get; }

        public Shader VertexShader { get; }

        public Shader GeometryShader { get; }

        public Shader FragmentShader { get; }

        public int ProgramID { get; }

        public OpenGLShaderSet(OpenGLVertexInputLayout inputLayout, OpenGLShader vertexShader, OpenGLShader geometryShader, OpenGLShader fragmentShader)
        {
            InputLayout = inputLayout;
            VertexShader = vertexShader;
            GeometryShader = geometryShader;
            FragmentShader = fragmentShader;

            ProgramID = GL.CreateProgram();
            GL.AttachShader(ProgramID, vertexShader.ShaderID);
            if (geometryShader != null)
            {
                GL.AttachShader(ProgramID, geometryShader.ShaderID);
            }
            GL.AttachShader(ProgramID, fragmentShader.ShaderID);

            int slot = 0;
            foreach (var input in inputLayout.InputDescriptions)
            {
                for (int i = 0; i < input.Elements.Length; i++)
                {
                    GL.BindAttribLocation(ProgramID, slot, input.Elements[i].Name);
                    slot += 1;
                }
            }

            GL.LinkProgram(ProgramID);

            int linkStatus;
            GL.GetProgram(ProgramID, GetProgramParameterName.LinkStatus, out linkStatus);
            if (linkStatus != 1)
            {
                string log = GL.GetProgramInfoLog(ProgramID);
                throw new InvalidOperationException($"Error linking GL program: {log}");
            }
        }

        public bool BindConstantBuffer(int slot, int blockLocation, OpenGLConstantBuffer cb)
        {
            // NOTE: slot == uniformBlockIndex

            if (_boundConstantBuffers.TryGetValue(slot, out OpenGLConstantBuffer boundCB) && boundCB == cb)
            {
                return false;
            }

            GL.UniformBlockBinding(ProgramID, blockLocation, slot);
            _boundConstantBuffers[slot] = cb;
            return true;
        }

        VertexInputLayout ShaderSet.InputLayout => InputLayout;

        public void Dispose()
        {
            InputLayout.Dispose();
            VertexShader.Dispose();
            GeometryShader?.Dispose();
            FragmentShader.Dispose();
            GL.DeleteProgram(ProgramID);
        }
    }
}
