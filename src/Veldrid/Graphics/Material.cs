namespace Veldrid.Graphics
{
    public interface Material : RenderStateModifier
    {
        void ApplyPerObjectInput(ConstantBufferDataProvider dataProvider);
        void ApplyPerObjectInputs(ConstantBufferDataProvider[] dataProviders);
    }
}