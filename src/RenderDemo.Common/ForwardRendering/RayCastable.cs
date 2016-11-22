using System.Collections.Generic;

namespace Veldrid.RenderDemo.ForwardRendering
{
    public interface RayCastable
    {
        int RayCast(Ray ray, List<float> distances);
    }
}