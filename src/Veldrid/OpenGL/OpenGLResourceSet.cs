using System;

namespace Veldrid.OpenGL
{
    internal class OpenGLResourceSet : ResourceSet
    {
        public new OpenGLResourceLayout Layout { get; }
        public new BindableResource[] Resources { get; }
        public override string Name { get; set; }
        public ResourceRefCount[] RefCounts { get; }

        public OpenGLResourceSet(ref ResourceSetDescription description) : base(ref description)
        {
            Layout = Util.AssertSubtype<ResourceLayout, OpenGLResourceLayout>(description.Layout);
            Resources = Util.ShallowClone(description.BoundResources);
            RefCounts = new ResourceRefCount[Resources.Length];
            for (int i = 0; i < RefCounts.Length; i++)
            {
                RefCounts[i] = GetRefCount(Resources[i]);
            }
        }

        private ResourceRefCount GetRefCount(BindableResource br)
        {
            if (br is OpenGLTexture tex)
            {
                return tex.RefCount;
            }
            else if (br is OpenGLTextureView texView)
            {
                return texView.RefCount;
            }
            else if (br is OpenGLBuffer buff)
            {
                return buff.RefCount;
            }
            else if (br is DeviceBufferRange buffRange)
            {
                return ((OpenGLBuffer)buffRange.Buffer).RefCount;
            }
            else if (br is OpenGLSampler sampler)
            {
                return sampler.RefCount;
            }
            else
            {
                throw new VeldridException($"Unexpected resource in OpenGL ResourceSet: {br.GetType().Name}");
            }
        }

        public override void Dispose()
        {
        }
    }
}
