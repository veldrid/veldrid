namespace Veldrid
{
    /// <summary>
    /// Identifies a particular type of TextureView.
    /// </summary>
    public enum TextureViewType
    {
        /// <summary>
        /// Undefined, gets detected from Texture.
        /// </summary>
        Undefined,
        /// <summary>
        /// A one-dimensional TextureView.
        /// </summary>
        View1D,
        /// <summary>
        /// A two-dimensional TextureView.
        /// </summary>
        View2D,
        /// <summary>
        /// A three-dimensional TextureView.
        /// </summary>
        View3D,
        /// <summary>
        /// A cubemap TextureView.
        /// </summary>
        ViewCube,
        /// <summary>
        /// A one-dimensional array TextureView.
        /// </summary>
        View1DArray,
        /// <summary>
        /// A two-dimensional array TextureView.
        /// </summary>
        View2DArray,
        /// <summary>
        /// A cubemap arraya TextureView.
        /// </summary>
        ViewCubeArray,
    }
}
