namespace Veldrid.MetalBindings
{
    public enum MTLResourceOptions : uint
    {
        CPUCacheModeDefaultCache = MTLCPUCacheMode.DefaultCache,
        CPUCacheModeWriteCombined = MTLCPUCacheMode.WriteCombined,

        StorageModeShared = MTLStorageMode.Shared << 4,
        StorageModeManaged = MTLStorageMode.Managed << 4,
        StorageModePrivate = MTLStorageMode.Private << 4,
        StorageModeMemoryless = MTLStorageMode.Memoryless << 4,

        HazardTrackingModeUntracked = (uint)(0x1UL << 8),
    }
}