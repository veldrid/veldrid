namespace Veldrid.Graphics
{
    /// <summary>
    /// A <see cref="MaterialTextureInputElement"/> which represents a manually-assigned texture.
    /// This can be thought of as a placeholder element for textures which are manually attached at render time.
    /// </summary>
    public class ManualTextureInput : MaterialTextureInputElement
    {
        /// <summary>
        /// Constructs a new <see cref="ManualTextureInput"/> with the given name.
        /// </summary>
        /// <param name="name"></param>
        public ManualTextureInput(string name) : base(name)
        {
        }

        public override DeviceTexture GetDeviceTexture(RenderContext rc)
        {
            return null;
        }
    }
}
