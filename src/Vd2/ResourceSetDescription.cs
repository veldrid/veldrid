namespace Vd2
{
    public struct ResourceSetDescription
    {
        public ResourceLayout Layout;
        public BindableResource[] BoundResources;

        public ResourceSetDescription(ResourceLayout layout, params BindableResource[] boundResources)
        {
            Layout = layout;
            BoundResources = boundResources;
        }
    }
}