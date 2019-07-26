namespace Veldrid
{
    public struct RenderPassDescription
    {
        public Framebuffer Framebuffer;

        public LoadAction DepthLoadAction;
        public float ClearDepth;
        public StoreAction DepthStoreAction;

        public LoadAction StencilLoadAction;
        public byte ClearStencil;
        public StoreAction StencilStoreAction;

        private SmallFixedOrDynamicArray<LoadAction> LoadActions;
        private SmallFixedOrDynamicArray<RgbaFloat> ClearColors;
        private SmallFixedOrDynamicArray<StoreAction> StoreActions;

        public RenderPassDescription(
            Framebuffer framebuffer,
            LoadAction loadAction,
            RgbaFloat clearColor,
            LoadAction depthLoadAction,
            float clearDepth,
            StoreAction storeAction,
            StoreAction depthStoreAction)
        {
            Framebuffer = framebuffer;
            uint colorCount = (uint)framebuffer.ColorTargets.Count;
            LoadActions = new SmallFixedOrDynamicArray<LoadAction>(colorCount);
            ClearColors = new SmallFixedOrDynamicArray<RgbaFloat>(colorCount);
            StoreActions = new SmallFixedOrDynamicArray<StoreAction>(colorCount);
            for (uint i = 0; i < colorCount; i++)
            {
                LoadActions.Set(i, loadAction);
                StoreActions.Set(i, storeAction);
                ClearColors.Set(i, clearColor);
            }

            DepthLoadAction = depthLoadAction;
            ClearDepth = clearDepth;
            DepthStoreAction = depthStoreAction;

            StencilLoadAction = default;
            ClearStencil = default;
            StencilStoreAction = default;
        }

        public RenderPassDescription(
            Framebuffer framebuffer,
            LoadAction[] loadActions,
            RgbaFloat[] clearColors,
            LoadAction depthLoadAction,
            float clearDepth,
            LoadAction stencilLoadAction,
            byte clearStencil,
            StoreAction[] storeActions,
            StoreAction depthStoreAction,
            StoreAction stencilStoreAction,
            Texture[] resolveTextures)
        {
            Framebuffer = framebuffer;
            LoadActions = new SmallFixedOrDynamicArray<LoadAction>(loadActions);
            ClearColors = new SmallFixedOrDynamicArray<RgbaFloat>(clearColors);
            DepthLoadAction = depthLoadAction;
            ClearDepth = clearDepth;
            StencilLoadAction = stencilLoadAction;
            ClearStencil = clearStencil;
            StencilStoreAction = stencilStoreAction;
            StoreActions = new SmallFixedOrDynamicArray<StoreAction>(storeActions);
            DepthStoreAction = depthStoreAction;
        }

        public static RenderPassDescription Create(Framebuffer framebuffer)
        {
            RenderPassDescription ret = new RenderPassDescription();
            ret.Framebuffer = framebuffer;
            uint colorTargetCount = (uint)framebuffer.ColorTargets.Count;
            ret.LoadActions = new SmallFixedOrDynamicArray<LoadAction>(colorTargetCount);
            ret.StoreActions = new SmallFixedOrDynamicArray<StoreAction>(colorTargetCount);
            ret.ClearColors = new SmallFixedOrDynamicArray<RgbaFloat>(colorTargetCount);
            return ret;
        }

        public void GetColorAttachment(
            uint index,
            out LoadAction load,
            out StoreAction store,
            out RgbaFloat clearColor)
        {
            load = LoadActions.Get(index);
            store = StoreActions.Get(index);
            clearColor = ClearColors.Get(index);
        }

        public void SetColorAttachment(uint index, LoadAction load, StoreAction store)
            => SetColorAttachment(index, load, store, default);
        public void SetColorAttachment(uint index, LoadAction load, StoreAction store, RgbaFloat clearColor)
        {
            LoadActions.Set(index, load);
            StoreActions.Set(index, store);
            ClearColors.Set(index, clearColor);
        }

        public void SetDepthAttachment(LoadAction load, StoreAction store) => SetDepthAttachment(load, store, default);
        public void SetDepthAttachment(LoadAction load, StoreAction store, float clearDepth)
        {
            DepthLoadAction = load;
            DepthStoreAction = store;
            ClearDepth = clearDepth;
        }

        public void SetStencilAttachment(LoadAction load, StoreAction store)
            => SetStencilAttachment(load, store, default);
        public void SetStencilAttachment(LoadAction load, StoreAction store, byte clearStencil)
        {
            StencilLoadAction = load;
            StencilStoreAction = store;
            ClearStencil = clearStencil;
        }
    }
}
