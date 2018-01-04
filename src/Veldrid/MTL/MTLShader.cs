using System.Text;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLShader : Shader
    {
        private readonly MTLGraphicsDevice _device;
        private bool _disposed;

        public MTLLibrary Library { get; private set; }
        public MTLFunction Function { get; private set; }
        public override string Name { get; set; }

        public MTLShader(ref ShaderDescription description, MTLGraphicsDevice gd)
            : base(description.Stage)
        {
            _device = gd;
            string shaderString = Encoding.UTF8.GetString(description.ShaderBytes);
            Library = gd.Device.newLibraryWithSource(shaderString, MTLCompileOptions.New());
            Function = Library.newFunctionWithName(description.EntryPoint);
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                ObjectiveCRuntime.release(Function.NativePtr);
                ObjectiveCRuntime.release(Library.NativePtr);
            }
        }
    }
}