namespace Veldrid.Graphics
{
    public class MaterialGlobalInputs
    {
        public MaterialGlobalInputElement[] Elements { get; }

        public MaterialGlobalInputs(MaterialGlobalInputElement[] elements)
        {
            Elements = elements;
        }
    }

    public class MaterialGlobalInputElement
    {
        public string Name { get; }
        public MaterialGlobalInputType Type { get; }
        public ConstantBufferDataProvider DataProvider { get; }

        public MaterialGlobalInputElement(string name, MaterialGlobalInputType type, ConstantBufferDataProvider dataProvider)
        {
            Name = name;
            Type = type;
            DataProvider = dataProvider;
        }
    }
}
