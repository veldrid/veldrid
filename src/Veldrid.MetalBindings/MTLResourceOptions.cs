using System.Diagnostics.CodeAnalysis;

namespace Veldrid.MetalBindings
{
    [SuppressMessage("Design", "CA1069:Enums values should not be duplicated", Justification = "Bindings")]
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
