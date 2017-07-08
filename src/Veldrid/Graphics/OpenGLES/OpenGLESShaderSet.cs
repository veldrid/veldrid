using OpenTK.Graphics.ES30;
using System;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESShaderSet : ShaderSet
    {
        public OpenGLESVertexInputLayout InputLayout { get; }

        public Shader VertexShader { get; }

        public Shader GeometryShader => null;

        public Shader FragmentShader { get; }

        public int ProgramID { get; }

        public OpenGLESShaderSet(OpenGLESVertexInputLayout inputLayout, OpenGLESShader vertexShader, OpenGLESShader fragmentShader)
        {
            InputLayout = inputLayout;
            VertexShader = vertexShader;
            FragmentShader = fragmentShader;

            ProgramID = GL.CreateProgram();
            Utilities.CheckLastGLES3Error();
            GL.AttachShader(ProgramID, vertexShader.ShaderID);
            Utilities.CheckLastGLES3Error();
            GL.AttachShader(ProgramID, fragmentShader.ShaderID);
            Utilities.CheckLastGLES3Error();

            int slot = 0;
            foreach (var input in inputLayout.InputDescriptions)
            {
                for (int i = 0; i < input.Elements.Length; i++)
                {
                    GL.BindAttribLocation(ProgramID, slot, input.Elements[i].Name);
                    Utilities.CheckLastGLES3Error();
                    slot += 1;
                }
            }

            GL.LinkProgram(ProgramID);
            Utilities.CheckLastGLES3Error();

            int linkStatus;
            GL.GetProgram(ProgramID, GetProgramParameterName.LinkStatus, out linkStatus);
            Utilities.CheckLastGLES3Error();
            if (linkStatus != 1)
            {
                string log = GL.GetProgramInfoLog(ProgramID);
                Utilities.CheckLastGLES3Error();
                throw new VeldridException($"Error linking GL program: {log}");
            }
        }

        VertexInputLayout ShaderSet.InputLayout => InputLayout;

        public void Dispose()
        {
            InputLayout.Dispose();
            VertexShader.Dispose();
            FragmentShader.Dispose();
            GL.DeleteProgram(ProgramID);
            Utilities.CheckLastGLES3Error();
        }
    }
}
