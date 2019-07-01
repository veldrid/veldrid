namespace Veldrid.SampleGallery
{
    public class GalleryConfig
    {
        public static GalleryConfig Global { get; set; } = new GalleryConfig();

        public OutputDescription MainFBOutput { get; set; }
        public ResourceLayout BlitterLayout { get; internal set; }

        /// <summary>
        /// An array of ResourceSets, one for each buffered frame.
        /// Contents of each set:
        ///   0: UniformBuffer w/ <see cref="CameraInfo"/> struct.
        /// </summary>
        public ResourceSet[] CameraInfoSets { get; set; }
        /// <summary>
        /// The layout for <see cref="CameraInfoSet"/>.
        /// </summary>
        public ResourceLayout CameraInfoLayout { get; set; }
        /// <summary>
        /// Contents:
        ///   0: UniformBuffer w/ <see cref="FBInfo"/> struct.
        /// </summary>
        public ResourceSet MainFBInfoSet { get; set; }
        /// <summary>
        /// The layout for <see cref="MainFBInfoSet"/>.
        /// </summary>
        public ResourceLayout MainFBInfoLayout { get; set; }
        public float ViewWidth { get; set; }
        public float ViewHeight { get; set; }
    }

    public struct FBInfo
    {
        public uint Width;
        public uint Height;
        private uint _padding0;
        private uint _padding1;
    }
}
