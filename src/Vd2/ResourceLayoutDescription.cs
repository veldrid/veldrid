namespace Vd2
{
    public struct ResourceLayoutDescription
    {
        public ResourceLayoutElementDescription[] Elements;

        public ResourceLayoutDescription(params ResourceLayoutElementDescription[] elements)
        {
            Elements = elements;
        }
    }
}
