namespace Veldrid
{
    /// <summary>
    /// Indicates how the rasterizer will fill polygons.
    /// </summary>
    public enum PolygonFillMode : byte
    {
        /// <summary>
        /// Polygons are filled completely.
        /// </summary>
        Solid,
        /// <summary>
        /// Polygons are outlined in a "wireframe" style.
        /// </summary>
        Wireframe,
    }
}
