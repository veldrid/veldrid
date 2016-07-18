using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A device object describing renderable material information.
    /// Controls which shaders are used, and which shaders and textures are used.
    /// </summary>
    public interface Material : IDisposable
    {
        /// <summary>
        /// Applies per-object shader data to the material.
        /// If this method is used, the material must have only one per-object input parameter.
        /// </summary>
        /// <param name="dataProvider">The data provider to use.</param>
        void ApplyPerObjectInput(ConstantBufferDataProvider dataProvider);
        /// <summary>
        /// Applies per-object shader data to the material.
        /// </summary>
        /// <param name="dataProviders">The data providers to use. The number of elements in this
        /// array must match the number of per-object parameters accepted by the material.</param>
        void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders);
        void UseDefaultTextures();
        /// <summary>
        /// Applies a texture override for the given texture slot.
        /// </summary>
        /// <param name="slot">The texture slot to override.</param>
        /// <param name="binding">A shader binding for the texture to use.</param>
        void UseTexture(int slot, ShaderTextureBinding binding);
    }
}