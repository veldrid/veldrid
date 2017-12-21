// using System;
// using System.IO;
// using System.Runtime.CompilerServices;
// using Veldrid.MetalBindings;
// using Veldrid.Sdl2;

// namespace Veldrid.Metal
// {
//     class Program
//     {
//         private static CAMetalLayer _metalLayer;
//         private static MTLRenderPipelineState _pipelineState;
//         private static MTLCommandQueue _commandQueue;
//         private static MTLBuffer _vertexBuffer;

//         static unsafe void Main(string[] args)
//         {
//             MTLDevice device = MTLDevice.MTLCreateSystemDefaultDevice();
//             var maxThing = device.maxThreadsPerThreadgroup;
//             string name = device.name;

//             Sdl2Window window = new Sdl2Window("Metal test",
//                 50, 50, 960, 540,
//                 SDL_WindowFlags.Shown | SDL_WindowFlags.Resizable, false);

//             SDL_SysWMinfo info;
//             Sdl2Native.SDL_GetWMWindowInfo(window.SdlWindowHandle, &info);

//             ref CocoaWindowInfo cocoaInfo = ref Unsafe.AsRef<CocoaWindowInfo>(&info.info);
//             NSWindow nswindow = new NSWindow(cocoaInfo.Window);

//             var contentView = nswindow.contentView;
//             contentView.WantsLayer = true;

//             _metalLayer = CAMetalLayer.New();
//             contentView.Layer = _metalLayer.NativePtr;
//             _metalLayer.device = device;
//             _metalLayer.pixelFormat = MTLPixelFormat.BGRA8Unorm;
//             _metalLayer.framebufferOnly = true;

//             float[] vertexData =
//             {
//                 0, 1, 0,
//                 -1f, -1f, 0,
//                 1f, -1f, 0
//             };

//             fixed (float* vertexDataPtr = vertexData)
//             {
//                 _vertexBuffer = device.newBuffer(vertexDataPtr, (UIntPtr)(sizeof(float) * vertexData.Length), 0);
//             }

//             string shaderText = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Shaders.metal"));
//             var library = device.newLibraryWithSource(shaderText, MTLCompileOptions.New());
//             var vertexFunction = library.newFunctionWithName("basic_vertex");
//             var fragmentFunction = library.newFunctionWithName("basic_fragment");

//             var descriptor = MTLRenderPipelineDescriptor.New();
//             descriptor.vertexFunction = vertexFunction;
//             descriptor.fragmentFunction = fragmentFunction;
//             var attachment = descriptor.colorAttachments[0];
//             attachment.pixelFormat = MTLPixelFormat.BGRA8Unorm;

//             _pipelineState = device.newRenderPipelineStateWithDescriptor(descriptor);

//             _commandQueue = device.newCommandQueue();

//             while (window.Exists)
//             {
//                 window.PumpEvents();
//                 Draw();
//             }
//         }

//         private static void Draw()
//         {
//             CAMetalDrawable drawable = _metalLayer.nextDrawable();
//             ObjectiveCRuntime.objc_msgSend(drawable.NativePtr, "retain");
            
//             if (drawable.NativePtr == IntPtr.Zero)
//             {
//                 return;
//             }
            
//             MTLRenderPassDescriptor rpDesc = MTLUtil.AllocInit<MTLRenderPassDescriptor>();
//             var attachment = rpDesc.colorAttachments[0];
//             attachment.texture = drawable.texture;
//             attachment.loadAction = MTLLoadAction.Clear;

//             attachment.clearColor = new MTLClearColor(
//                 (Environment.TickCount / 1000f) % 1.0,
//                 (Environment.TickCount / 2000f) % 1.0,
//                 (Environment.TickCount / 3000f) % 1.0,
//                 1);

//             var commandBuffer = _commandQueue.commandBuffer();
//             var encoder = commandBuffer.renderCommandEncoderWithDescriptor(rpDesc);
//             encoder.setRenderPipelineState(_pipelineState);
//             encoder.setVertexBuffer(_vertexBuffer, UIntPtr.Zero, UIntPtr.Zero);
//             encoder.drawPrimitives(MTLPrimitiveType.Triangle, (UIntPtr)0, (UIntPtr)3);
//             encoder.endEncoding();
//             commandBuffer.presentDrawable(drawable.NativePtr);
//             ObjectiveCRuntime.objc_msgSend(drawable.NativePtr, "release");
//             commandBuffer.commit();
//         }
//     }
// }
