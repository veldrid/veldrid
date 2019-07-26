namespace Veldrid.CommandRecording
{
    internal struct NoAllocBeginRenderPassEntry
    {
        public Tracked<Framebuffer> Framebuffer;
        /// <summary>
        /// The starting index into the owner's attachment entry list.
        /// There is one entry per color attachment in <see cref="Framebuffer"/>.
        /// </summary>
        public uint AttachmentInfoStartIndex;

        public LoadAction DepthLoad;
        public float ClearDepth;
        public StoreAction DepthStore;

        public LoadAction StencilLoad;
        public byte ClearStencil;
        public StoreAction StencilStore;

        public NoAllocBeginRenderPassEntry(
            Tracked<Framebuffer> framebuffer,
            uint attachmentInfoStartIndex,
            LoadAction depthLoad,
            float clearDepth,
            StoreAction depthStore,
            LoadAction stencilLoad,
            byte clearStencil,
            StoreAction stencilStore)
        {
            Framebuffer = framebuffer;
            AttachmentInfoStartIndex = attachmentInfoStartIndex;
            DepthLoad = depthLoad;
            ClearDepth = clearDepth;
            DepthStore = depthStore;
            StencilLoad = stencilLoad;
            ClearStencil = clearStencil;
            StencilStore = stencilStore;
        }
    }

    internal struct RecordedAttachmentInfo
    {
        public StoreAction Store;
        public LoadAction Load;
        public RgbaFloat ClearColor;

        public RecordedAttachmentInfo(StoreAction store, LoadAction load, RgbaFloat clearColor)
        {
            Store = store;
            Load = load;
            ClearColor = clearColor;
        }
    }
}
