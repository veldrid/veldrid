namespace Veldrid.Graphics
{
    public class ShaderTextureInput
    {
        public int Slot { get; }
        public string Name { get; }

        public ShaderTextureInput(int slot, string name)
        {
            Slot = slot;
            Name = name;
        }
    }
}
