namespace Veldrid.Graphics
{
    public class MaterialInputs<T> where T : MaterialInputElement
    {
        public T[] Elements { get; private set; }

        public MaterialInputs(params T[] elements)
        {
            Elements = elements;
        }

        public static MaterialInputs<T> Empty { get; } = new MaterialInputs<T>(System.Array.Empty<T>());
    }

    public abstract class MaterialInputElement
    {
        public string Name { get; set; }
        public MaterialInputType Type { get; set; }

        public MaterialInputElement(string name, MaterialInputType type)
        {
            Name = name;
            Type = type;
        }
    }

    public class MaterialGlobalInputElement : MaterialInputElement
    {
        public ConstantBufferDataProvider DataProvider { get; private set; }

        public MaterialGlobalInputElement(string name, MaterialInputType type, ConstantBufferDataProvider dataProvider)
            : base(name, type)
        {
            DataProvider = dataProvider;
        }
    }

    public class MaterialPerObjectInputElement : MaterialInputElement
    {
        public int BufferSizeInBytes { get; private set; }

        public MaterialPerObjectInputElement(string name, MaterialInputType type, int bufferSizeInBytes)
            : base(name, type)
        {
            BufferSizeInBytes = bufferSizeInBytes;
        }

        public MaterialPerObjectInputElement()
            : this("<NONAME>", MaterialInputType.Matrix4x4, 16)
        {
        }
    }
}
