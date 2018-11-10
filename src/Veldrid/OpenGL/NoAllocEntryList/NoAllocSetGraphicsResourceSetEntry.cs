using System.Runtime.CompilerServices;

namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal unsafe struct NoAllocSetResourceSetEntry
    {
        public const int MaxInlineDynamicOffsets = 10;

        public readonly uint Slot;
        public readonly Tracked<ResourceSet> ResourceSet;
        public readonly bool IsGraphics;
        public readonly uint DynamicOffsetCount;
        public fixed uint DynamicOffsets_Inline[MaxInlineDynamicOffsets];
        public readonly StagingBlock DynamicOffsets_Block;

        public NoAllocSetResourceSetEntry(
            uint slot,
            Tracked<ResourceSet> rs,
            bool isGraphics,
            uint dynamicOffsetCount,
            ref uint dynamicOffsets)
        {
            Slot = slot;
            ResourceSet = rs;
            IsGraphics = isGraphics;
            DynamicOffsetCount = dynamicOffsetCount;
            for (int i = 0; i < dynamicOffsetCount; i++)
            {
                DynamicOffsets_Inline[i] = Unsafe.Add(ref dynamicOffsets, i);
            }

            DynamicOffsets_Block = default;
        }

        public NoAllocSetResourceSetEntry(
            uint slot,
            Tracked<ResourceSet> rs,
            bool isGraphics,
            StagingBlock dynamicOffsets)
        {
            Slot = slot;
            ResourceSet = rs;
            IsGraphics = isGraphics;
            DynamicOffsetCount = (uint)dynamicOffsets.SizeInBytes / sizeof(uint);
            DynamicOffsets_Block = dynamicOffsets;
        }
    }
}
