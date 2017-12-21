using System.Text;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLShader : Shader
    {
        private readonly MTLGraphicsDevice _device;

        public MTLLibrary Library { get; private set; }
        public MTLFunction Function { get; private set; }
        public override string Name
        {
            get => throw new System.NotImplementedException();
            set => throw new System.NotImplementedException();
        }

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
            ObjectiveCRuntime.release(Function.NativePtr);
            ObjectiveCRuntime.release(Library.NativePtr);
        }
    }
}