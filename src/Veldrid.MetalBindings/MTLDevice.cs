using System;
using System.Runtime.InteropServices;
using static Veldrid.MetalBindings.ObjectiveCRuntime;

namespace Veldrid.MetalBindings
{
    public unsafe struct MTLDevice
    {
        public readonly IntPtr NativePtr;
        public static implicit operator IntPtr(MTLDevice device) => device.NativePtr;
        public MTLDevice(IntPtr nativePtr) => NativePtr = nativePtr;
        public string name => string_objc_msgSend(NativePtr, "name");
        public MTLSize maxThreadsPerThreadgroup => objc_msgSend_stret<MTLSize>(this, new Selector("maxThreadsPerThreadgroup"));
        
        // TODO: This should have an "out NSError" parameter.
        public MTLLibrary newLibraryWithSource(string source, MTLCompileOptions options)
        {
            NSString sourceNSS = NSString.New(source);

            IntPtr library = IntPtr_objc_msgSend(NativePtr, "newLibraryWithSource:options:error:",
                sourceNSS,
                options,
                out NSError error);

            if (error.NativePtr != IntPtr.Zero)
            {
                throw new Exception("Shader compilation failed: " + error.localizedDescription);
            }

            return new MTLLibrary(library);
        }

        public MTLRenderPipelineState newRenderPipelineStateWithDescriptor(MTLRenderPipelineDescriptor desc)
        {
            IntPtr ret = IntPtr_objc_msgSend(NativePtr, "newRenderPipelineStateWithDescriptor:error:",
                desc.NativePtr,
                out NSError error);

            if (error.NativePtr != IntPtr.Zero)
            {
                throw new Exception("Failed to create new MTLRenderPipelineState: " + error.localizedDescription);
            }

            return new MTLRenderPipelineState(ret);
        }

        public MTLCommandQueue newCommandQueue() => objc_msgSend<MTLCommandQueue>(NativePtr, "newCommandQueue");

        public MTLBuffer newBuffer(void* pointer, UIntPtr length, MTLResourceOptions options)
        {
            IntPtr buffer = IntPtr_objc_msgSend(NativePtr, "newBufferWithBytes:length:options:",
                pointer,
                length,
                options);
            return new MTLBuffer(buffer);
        }

        public MTLBuffer newBufferWithLengthOptions(UIntPtr length, MTLResourceOptions options)
        {
            IntPtr buffer = IntPtr_objc_msgSend(NativePtr, "newBufferWithLength:options:", length, options);
            return new MTLBuffer(buffer);
        }

        public MTLTexture newTextureWithDescriptor(MTLTextureDescriptor descriptor)
            => objc_msgSend<MTLTexture>(NativePtr, "newTextureWithDescriptor:", descriptor.NativePtr);

        public MTLSamplerState newSamplerStateWithDescriptor(MTLSamplerDescriptor descriptor)
            => objc_msgSend<MTLSamplerState>(NativePtr, "newSamplerStateWithDescriptor:", descriptor.NativePtr);

        public Bool8 supportsTextureSampleCount(UIntPtr sampleCount)
            => bool8_objc_msgSend(NativePtr, "supportsTextureSampleCount:", sampleCount);

        private const string MetalFramework = "/System/Library/Frameworks/Metal.framework/Metal";
        [DllImport(MetalFramework)]
        public static extern MTLDevice MTLCreateSystemDefaultDevice();
    }
}