using OpenTK.Graphics.OpenGL;
using System;

namespace Veldrid.Graphics.OpenGL
{
    public class OpenGLShader : IDisposable
    {
        public int ShaderID { get; }

        public OpenGLShader(string source, ShaderType type)
        {
            ShaderID = GL.CreateShader(type);
            GL.ShaderSource(ShaderID, source);
            GL.CompileShader(ShaderID);
            int compileStatus;
            GL.GetShader(ShaderID, ShaderParameter.CompileStatus, out compileStatus);
            if (compileStatus != 1)
            {
                string shaderLog = GL.GetShaderInfoLog(ShaderID);
                throw new InvalidOperationException($"Error compiling {type} shader. {shaderLog}");
            }
        }

        public void Dispose()
        {
            GL.DeleteShader(ShaderID);
        }
    }
}
