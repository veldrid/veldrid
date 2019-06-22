namespace Veldrid.CommandRecording
{
    internal struct NoAllocBeginRenderPassEntry
    {
        public Tracked<Framebuffer> Framebuffer;
        public StoreAction StoreAction;
        public LoadAction LoadAction;
        public RgbaFloat ClearColor;
        public float ClearDepth;

        public NoAllocBeginRenderPassEntry(
            Tracked<Framebuffer> tracked,
            StoreAction storeAction,
            LoadAction loadAction,
            RgbaFloat clearColor,
            float clearDepth)
        {
            Framebuffer = tracked;
            StoreAction = storeAction;
            LoadAction = loadAction;
            ClearColor = clearColor;
            ClearDepth = clearDepth;
        }
    }
}
