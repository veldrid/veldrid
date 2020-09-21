using System.Text;
using static Veldrid.WebGL.WebGLConstants;

namespace Veldrid.WebGL
{
    internal class WebGLShader : Shader
    {
        private readonly WebGLGraphicsDevice _gd;
        private bool _disposed;

        public WebGLDotNET.WebGLShader WglShader { get; }
        public override string Name { get; set; }

        public override bool IsDisposed => _disposed;

        public WebGLShader(WebGLGraphicsDevice gd, ref ShaderDescription description)
            : base(description.Stage, description.EntryPoint)
        {
            _gd = gd;
            WglShader = _gd.Ctx.CreateShader(WebGLUtil.GetShaderStage(description.Stage));
            _gd.CheckError();

            string shaderSource = Encoding.UTF8.GetString(description.ShaderBytes);

            _gd.Ctx.ShaderSource(WglShader, shaderSource);
            _gd.CheckError();
            _gd.Ctx.CompileShader(WglShader);
            _gd.CheckError();

            bool compileSucceeded = (bool)_gd.Ctx.GetShaderParameter(WglShader, COMPILE_STATUS);

            if (!compileSucceeded)
            {
                string message = _gd.Ctx.GetShaderInfoLog(WglShader);
                throw new VeldridException($"Unable to compile shader code for shader [{Name}] of type {description.Stage}: {message}");
            }
        }

        public override void Dispose()
        {
            WglShader.Dispose();
            _disposed = true;
        }
    }
}
