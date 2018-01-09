using System;
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

        public unsafe MTLShader(ref ShaderDescription description, MTLGraphicsDevice gd)
            : base(description.Stage)
        {
            _device = gd;
            DispatchQueue queue = Dispatch.dispatch_get_global_queue(QualityOfServiceLevel.QOS_CLASS_USER_INTERACTIVE, 0);
            fixed (byte* shaderBytesPtr = description.ShaderBytes)
            {
                DispatchData dispatchData = Dispatch.dispatch_data_create(
                    shaderBytesPtr,
                    (UIntPtr)description.ShaderBytes.Length,
                    queue,
                    IntPtr.Zero);

                Library = gd.Device.newLibraryWithData(dispatchData);
                Function = Library.newFunctionWithName(description.EntryPoint);
                Dispatch.dispatch_release(dispatchData.NativePtr);
            }
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