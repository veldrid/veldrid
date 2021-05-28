#if !EXCLUDE_METAL_BACKEND
using Veldrid.MTL;

namespace Veldrid
{
    /// <summary>
    /// Exposes Metal-specific functionality,
    /// useful for interoperating with native components which interface directly with Metal.
    /// Can only be used on <see cref="GraphicsBackend.Metal"/>.
    /// </summary>
    public class BackendInfoMetal
    {
        private readonly MTLGraphicsDevice _gd;

        internal BackendInfoMetal(MTLGraphicsDevice gd)
        {
            _gd = gd;
        }

        public MTLFeatureSupport FeatureSupport => _gd.MetalFeatures;
    }
}
#endif
