using System;

namespace Veldrid.Graphics
{
    public interface ShaderConstantBindings : IDisposable
    {
        void Apply();
        void ApplyPerObjectInput(ConstantBufferDataProvider dataProvider);
        void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders);
    }
}
