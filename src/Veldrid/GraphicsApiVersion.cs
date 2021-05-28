
namespace Veldrid
{
    public readonly struct GraphicsApiVersion
    {
        public static GraphicsApiVersion Unknown => default;

        public int Major { get; }
        public int Minor { get; }
        public int Subminor { get; }
        public int Patch { get; }

        public bool IsKnown => Major != 0 && Minor != 0 && Subminor != 0 && Patch != 0;

        public GraphicsApiVersion(int major, int minor, int subminor, int patch)
        {
            Major = major;
            Minor = minor;
            Subminor = subminor;
            Patch = patch;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Subminor}.{Patch}";
        }
    }
}
