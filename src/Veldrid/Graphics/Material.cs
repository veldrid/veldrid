using System;

namespace Veldrid.Graphics
{
    public interface Material : RenderStateModifier, IDisposable
    {
        void ApplyPerObjectInput(ConstantBufferDataProvider dataProvider);
        void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders);
    }
}