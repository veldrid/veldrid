namespace Veldrid.Graphics
{
    public class ManualTextureInput : MaterialTextureInputElement
    {
        public ManualTextureInput(string name) : base(name)
        {
        }

        public override DeviceTexture GetDeviceTexture(RenderContext rc)
        {
            return null;
        }
    }
}
