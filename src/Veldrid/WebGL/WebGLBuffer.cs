using System;
using static Veldrid.WebGL.WebGLConstants;

namespace Veldrid.WebGL
{
    internal class WebGLBuffer : DeviceBuffer
    {
        private readonly WebGLGraphicsDevice _gd;

        public WebGLDotNET.WebGLBuffer WglBuffer { get; }

        public override uint SizeInBytes { get; }

        public override BufferUsage Usage { get; }

        public override string Name { get; set; }

        public WebGLBuffer(WebGLGraphicsDevice gd, ref BufferDescription description)
            : base(gd)
        {
            _gd = gd;
            SizeInBytes = description.SizeInBytes;
            Usage = description.Usage;
            WglBuffer = _gd.Ctx.CreateBuffer();
            uint target = (Usage & BufferUsage.IndexBuffer) != 0 ? ELEMENT_ARRAY_BUFFER : TRANSFORM_FEEDBACK_BUFFER;
            _gd.Ctx.BindBuffer(target, WglBuffer);

            uint usage = ((Usage & BufferUsage.Dynamic) != 0) ? DYNAMIC_DRAW : STATIC_DRAW;
            // TODO: This doesn't work because mono seems to call the wrong BufferData overload.
            //_gd.Ctx.BufferData(target, SizeInBytes, usage);

            var array = new WebAssembly.Core.Uint8Array((int)SizeInBytes);
            _gd.Ctx.BufferData(
                target,
                array,
                usage,
                0,
                SizeInBytes);
            array.Dispose();

            // int bufferSize = (int)gd.Ctx.GetBufferParameter(target, BUFFER_SIZE);
            // if (SizeInBytes != bufferSize)
            // {
            //     Console.WriteLine($"Buffer failed to initialize to the correct size.");
            // }
            // else
            // {
            //     Console.WriteLine($"Successfully set buffer size.");
            // }
        }

        public override void Dispose()
        {
            _gd.Ctx.DeleteBuffer(WglBuffer);
        }
    }
}
