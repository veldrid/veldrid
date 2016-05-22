using System;

namespace Veldrid.Graphics
{
    public interface Material : IDisposable
    {
        void ApplyPerObjectInput(ConstantBufferDataProvider dataProvider);
        void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders);
        void UseTexture(int slot, ShaderTextureBinding binding);
    }
}