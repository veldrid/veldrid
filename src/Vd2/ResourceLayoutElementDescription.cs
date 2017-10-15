namespace Vd2
{
    public struct ResourceLayoutElementDescription
    {
        public string Name;
        public ResourceKind Kind;
        public ShaderStages Stages;

        public ResourceLayoutElementDescription(string name, ResourceKind kind, ShaderStages stages)
        {
            Name = name;
            Kind = kind;
            Stages = stages;
        }
    }
}
