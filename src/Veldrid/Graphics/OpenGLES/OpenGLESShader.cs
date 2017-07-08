using OpenTK.Graphics.ES30;
using System;
using System.IO;

namespace Veldrid.Graphics.OpenGLES
{
    public class OpenGLESShader : Shader
    {
        public int ShaderID { get; private set; }

        public ShaderType Type { get; }

        public OpenGLESShader(Stream dataStream, OpenTK.Graphics.ES30.ShaderType type)
        {
            Type = OpenGLESFormats.GLToVeldridShaderType(type);
            string source;
            using (var sr = new StreamReader(dataStream))
            {
                source = sr.ReadToEnd();
            }

            LoadShader(source, type);
        }

        public OpenGLESShader(string source, OpenTK.Graphics.ES30.ShaderType type)
        {
            LoadShader(source, type);
        }

        private void LoadShader(string source, OpenTK.Graphics.ES30.ShaderType type)
        {
            ShaderID = GL.CreateShader(type);
            Utilities.CheckLastGLES3Error();
            GL.ShaderSource(ShaderID, source);
            Utilities.CheckLastGLES3Error();
            GL.CompileShader(ShaderID);
            Utilities.CheckLastGLES3Error();
            GL.GetShader(ShaderID, ShaderParameter.CompileStatus, out int compileStatus);
            Utilities.CheckLastGLES3Error();
            if (compileStatus != 1)
            {
                string shaderLog = GL.GetShaderInfoLog(ShaderID);
                Utilities.CheckLastGLES3Error();
                throw new VeldridException($"Error compiling {type} shader. {shaderLog}");
            }
        }

        public void Dispose()
        {
            GL.DeleteShader(ShaderID);
            Utilities.CheckLastGLES3Error();
        }
    }
}
