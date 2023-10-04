#if DEBUG
#define ALLOC_TRACK
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
using static Veldrid.Vulkan.VulkanUtil;
using VulkanBuffer = TerraFX.Interop.Vulkan.VkBuffer;

namespace Veldrid.Vulkan
{
    internal sealed unsafe class VkDeviceMemoryManager : IDisposable
    {
        private readonly VkDevice _device;
        private readonly VkPhysicalDevice _physicalDevice;
        private readonly ulong _bufferImageGranularity;
        private readonly ulong _chunkGranularity;
        private readonly object _allocatorMutex = new();
        private readonly Dictionary<uint, ChunkAllocatorSet> _allocatorsByMemoryTypeUnmapped = new();
        private readonly Dictionary<uint, ChunkAllocatorSet> _allocatorsByMemoryType = new();

        public VkDeviceMemoryManager(
            VkDevice device,
            VkPhysicalDevice physicalDevice,
            ulong bufferImageGranularity,
            ulong chunkGranularity)
        {
            _device = device;
            _physicalDevice = physicalDevice;
            _bufferImageGranularity = bufferImageGranularity;
            _chunkGranularity = chunkGranularity;
        }

        public VkMemoryBlock Allocate(
            VkPhysicalDeviceMemoryProperties memProperties,
            uint memoryTypeBits,
            VkMemoryPropertyFlags flags,
            bool persistentMapped,
            ulong size,
            ulong alignment)
        {
            return Allocate(
                memProperties,
                memoryTypeBits,
                flags,
                persistentMapped,
                size,
                alignment,
                false,
                VkImage.NULL,
                VulkanBuffer.NULL);
        }

        public VkMemoryBlock Allocate(
            VkPhysicalDeviceMemoryProperties memProperties,
            uint memoryTypeBits,
            VkMemoryPropertyFlags flags,
            bool persistentMapped,
            ulong size,
            ulong alignment,
            bool dedicated,
            VkImage dedicatedImage,
            VulkanBuffer dedicatedBuffer)
        {
            if (!TryFindMemoryType(memProperties, memoryTypeBits, flags, out uint memoryTypeIndex))
            {
                throw new VeldridException("No suitable memory type.");
            }

            ulong minDedicatedAllocationSize = persistentMapped
                ? ChunkAllocator.PersistentMappedChunkSize
                : ChunkAllocator.UnmappedChunkSize;

            ulong alignedSize = ((size + _chunkGranularity - 1) / _chunkGranularity) * _chunkGranularity;

            if (dedicated || alignedSize >= minDedicatedAllocationSize)
            {
                ulong dedicatedSize;
                if (dedicatedImage == VkImage.NULL && dedicatedBuffer == VulkanBuffer.NULL)
                {
                    // Round up to the nearest multiple of bufferImageGranularity.
                    dedicatedSize = ((alignedSize + _bufferImageGranularity - 1) / _bufferImageGranularity) * _bufferImageGranularity;
                }
                else
                {
                    // VkMemoryRequirements.size must be met.
                    dedicatedSize = size;
                }

                VkMemoryAllocateInfo allocateInfo = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO,
                    allocationSize = dedicatedSize,
                    memoryTypeIndex = memoryTypeIndex
                };

                VkMemoryDedicatedAllocateInfo dedicatedAI;
                if (dedicated)
                {
                    dedicatedAI = new VkMemoryDedicatedAllocateInfo()
                    {
                        sType = VkStructureType.VK_STRUCTURE_TYPE_MEMORY_DEDICATED_ALLOCATE_INFO,
                        buffer = dedicatedBuffer,
                        image = dedicatedImage
                    };
                    allocateInfo.pNext = &dedicatedAI;
                }

                VkDeviceMemory memory;
                VkResult allocationResult = vkAllocateMemory(_device, &allocateInfo, null, &memory);
                if (allocationResult != VkResult.VK_SUCCESS)
                {
                    throw new VeldridException("Unable to allocate sufficient Vulkan memory.");
                }

                void* mappedPtr = null;
                if (persistentMapped)
                {
                    VkResult mapResult = vkMapMemory(_device, memory, 0, dedicatedSize, 0, &mappedPtr);
                    if (mapResult != VkResult.VK_SUCCESS)
                    {
                        throw new VeldridException("Unable to map newly-allocated Vulkan memory.");
                    }
                }

                return new VkMemoryBlock(memory, 0, dedicatedSize, memoryTypeBits, mappedPtr, true);
            }
            else
            {
                ChunkAllocatorSet allocator = GetAllocator(memoryTypeIndex, persistentMapped);
                bool result = allocator.Allocate((uint)alignedSize, (uint)alignment, out VkMemoryBlock ret);
                if (!result)
                {
                    throw new VeldridException("Unable to allocate sufficient Vulkan memory.");
                }

                return ret;
            }
        }

        public void Free(VkMemoryBlock block)
        {
            if (block.DedicatedAllocation)
            {
                vkFreeMemory(_device, block.DeviceMemory, null);
            }
            else
            {
                GetAllocator(block.MemoryTypeIndex, block.IsPersistentMapped).Free(block);
            }
        }

        private ChunkAllocatorSet GetAllocator(uint memoryTypeIndex, bool persistentMapped)
        {
            lock (_allocatorMutex)
            {
                ChunkAllocatorSet? ret;

                if (persistentMapped)
                {
                    if (!_allocatorsByMemoryType.TryGetValue(memoryTypeIndex, out ret))
                    {
                        ret = new ChunkAllocatorSet(_device, memoryTypeIndex, true);
                        _allocatorsByMemoryType.Add(memoryTypeIndex, ret);
                    }
                }
                else
                {
                    if (!_allocatorsByMemoryTypeUnmapped.TryGetValue(memoryTypeIndex, out ret))
                    {
                        ret = new ChunkAllocatorSet(_device, memoryTypeIndex, false);
                        _allocatorsByMemoryTypeUnmapped.Add(memoryTypeIndex, ret);
                    }
                }

                return ret;
            }
        }

        private sealed class ChunkAllocatorSet : IDisposable
        {
            private readonly VkDevice _device;
            private readonly uint _memoryTypeIndex;
            private readonly bool _persistentMapped;
            private readonly object _mutex = new();
            private readonly List<ChunkAllocator> _allocators = new();

            public ChunkAllocatorSet(VkDevice device, uint memoryTypeIndex, bool persistentMapped)
            {
                _device = device;
                _memoryTypeIndex = memoryTypeIndex;
                _persistentMapped = persistentMapped;
            }

            public bool Allocate(uint size, uint alignment, out VkMemoryBlock block)
            {
                lock (_mutex)
                {
                    List<ChunkAllocator> allocators = _allocators;
                    for (int i = 0; i < allocators.Count; i++)
                    {
                        ChunkAllocator allocator = allocators[i];
                        if (allocator.Allocate(size, alignment, out block))
                        {
                            return true;
                        }
                    }

                    ChunkAllocator newAllocator = new(_device, _memoryTypeIndex, _persistentMapped);
                    allocators.Add(newAllocator);
                    return newAllocator.Allocate(size, alignment, out block);
                }
            }

            public void Free(VkMemoryBlock block)
            {
                lock (_mutex)
                {
                    List<ChunkAllocator> allocators = _allocators;
                    for (int i = 0; i < allocators.Count; i++)
                    {
                        ChunkAllocator allocator = allocators[i];
                        if (allocator.Memory == block.DeviceMemory)
                        {
                            allocator.Free(block);

                            // Free may have merged free blocks.
                            if (allocator.IsFullFreeBlock())
                            {
                                // The allocator is a free contiguous block so dispose it.
                                allocator.Dispose();

                                allocators.RemoveAt(i);
                            }
                            return;
                        }
                    }
                }
            }

            public void Dispose()
            {
                lock (_mutex)
                {
                    foreach (ChunkAllocator allocator in _allocators)
                    {
                        allocator.Dispose();
                    }
                    _allocators.Clear();
                }
            }
        }

        private sealed class ChunkAllocator : IDisposable
        {
            public const ulong PersistentMappedChunkSize = 1024 * 1024;
            public const ulong UnmappedChunkSize = 1024 * 1024 * 8;

            private readonly VkDevice _device;
            private readonly uint _memoryTypeIndex;
            private readonly bool _persistentMapped;
            private readonly List<VkMemoryBlock> _freeBlocks = new();
            private readonly void* _mappedPtr;
            private readonly ulong _totalMemorySize;

#if ALLOC_TRACK
            private SortedList<uint, ulong> _allocatedBlocks = new();
#endif

            public readonly VkDeviceMemory Memory;

            public ChunkAllocator(VkDevice device, uint memoryTypeIndex, bool persistentMapped)
            {
                _device = device;
                _memoryTypeIndex = memoryTypeIndex;
                _persistentMapped = persistentMapped;
                _totalMemorySize = persistentMapped ? PersistentMappedChunkSize : UnmappedChunkSize;

                VkMemoryAllocateInfo memoryAI = new()
                {
                    sType = VkStructureType.VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO,
                    allocationSize = _totalMemorySize,
                    memoryTypeIndex = _memoryTypeIndex
                };
                VkDeviceMemory memory;
                VkResult result = vkAllocateMemory(_device, &memoryAI, null, &memory);
                CheckResult(result);
                Memory = memory;

                void* mappedPtr = null;
                if (persistentMapped)
                {
                    result = vkMapMemory(_device, Memory, 0, _totalMemorySize, 0, &mappedPtr);
                    CheckResult(result);
                }
                _mappedPtr = mappedPtr;

                VkMemoryBlock initialBlock = new(
                    Memory,
                    0,
                    _totalMemorySize,
                    _memoryTypeIndex,
                    _mappedPtr,
                    false);
                _freeBlocks.Add(initialBlock);
            }

            public bool Allocate(uint size, uint alignment, out VkMemoryBlock resultBlock)
            {
                if (_freeBlocks.Count == 0)
                {
                    resultBlock = default;
                    return false;
                }

                Span<VkMemoryBlock> freeBlocks = CollectionsMarshal.AsSpan(_freeBlocks);

                ulong alignedBlockSize = 0;
                uint alignedOffsetRemainder = 0;

                int selectedIndex = -1;
                for (int i = 0; i < freeBlocks.Length; i++)
                {
                    ref VkMemoryBlock block = ref freeBlocks[i];
                    alignedBlockSize = block.Size;
                    alignedOffsetRemainder = block.Offset % alignment;
                    if (alignedOffsetRemainder != 0)
                    {
                        ulong alignmentCorrection = alignment - alignedOffsetRemainder;
                        if (alignedBlockSize <= alignmentCorrection)
                        {
                            continue;
                        }
                        alignedBlockSize -= alignmentCorrection;
                    }

                    if (alignedBlockSize >= size) // Valid match -- split it and return.
                    {
                        selectedIndex = i;
                        break;
                    }
                }

                if (selectedIndex != -1)
                {
                    VkMemoryBlock block = freeBlocks[selectedIndex];
                    if (alignedOffsetRemainder != 0)
                    {
                        uint offset = alignment - alignedOffsetRemainder;

                        VkMemoryBlock splitBlock = new(
                            block.DeviceMemory,
                            block.Offset,
                            offset,
                            _memoryTypeIndex,
                            block.BaseMappedPointer,
                            false);
                        _freeBlocks.Insert(selectedIndex, splitBlock);
                        selectedIndex++;

                        block.Offset += offset;
                    }
                    block.Size = alignedBlockSize;

                    if (alignedBlockSize != size)
                    {
                        VkMemoryBlock splitBlock = new(
                            block.DeviceMemory,
                            block.Offset + size,
                            block.Size - size,
                            _memoryTypeIndex,
                            block.BaseMappedPointer,
                            false);

                        _freeBlocks[selectedIndex] = splitBlock;
                        block.Size = size;
                    }
                    else
                    {
                        _freeBlocks.RemoveAt(selectedIndex);
                    }

#if ALLOC_TRACK
                    CheckAllocatedBlock(block.Offset, block.Size);
#endif
                    resultBlock = block;
                    return true;
                }

#if ALLOC_TRACK
                bool hasMergedBlocks = MergeContiguousBlocks();
                TrackAssert(!hasMergedBlocks, "Free method was not effective at merging blocks.");
#endif

                resultBlock = default;
                return false;
            }

            private static int FindPrecedingBlockIndex(ReadOnlySpan<VkMemoryBlock> list, ulong targetOffset)
            {
                int low = 0;
                int high = list.Length - 1;
                ref VkMemoryBlock b = ref MemoryMarshal.GetReference(list);

                if (list.Length == 0 || Unsafe.Add(ref b, high).Offset < targetOffset)
                    return -1;

                while (low <= high)
                {
                    int mid = low + ((high - low) / 2);

                    if (Unsafe.Add(ref b, mid).Offset >= targetOffset)
                        high = mid - 1;
                    else
                        low = mid + 1;
                }

                return high + 1;
            }

            public void Free(VkMemoryBlock block)
            {
                Span<VkMemoryBlock> freeBlocks = CollectionsMarshal.AsSpan(_freeBlocks);

                // The free block list should always be sorted by offset.
                // List mutations done by this algorithm must preserve order. 

                int precedingBlock = FindPrecedingBlockIndex(freeBlocks, block.Offset);
                if (precedingBlock != -1)
                {
                    if ((uint)precedingBlock < (uint)freeBlocks.Length &&
                        block.End == freeBlocks[precedingBlock].Offset)
                    {
                        // Free block ends at the beginning of found block; merge with found block.
                        freeBlocks[precedingBlock].Size += block.Size;
                        freeBlocks[precedingBlock].Offset = block.Offset;

                        int prevBlock = precedingBlock - 1;
                        if ((uint)prevBlock < (uint)freeBlocks.Length &&
                            freeBlocks[prevBlock].End == freeBlocks[precedingBlock].Offset)
                        {
                            // Merged block begins at the end of the previous block; extend the previous block.
                            freeBlocks[prevBlock].Size += freeBlocks[precedingBlock].Size;
                            _freeBlocks.RemoveAt(precedingBlock);
                        }
                    }
                    else
                    {
                        int prevBlock = precedingBlock - 1;
                        if ((uint)prevBlock < (uint)freeBlocks.Length &&
                            freeBlocks[prevBlock].End == block.Offset)
                        {
                            // Free block begins at the end of found block; extend the previous block.
                            freeBlocks[prevBlock].Size += block.Size;
                        }
                        else
                        {
                            // Free block could not be merged with previous or found block.
                            _freeBlocks.Insert(precedingBlock, block);
                        }
                    }
                }
                else
                {
                    int lastIndex = freeBlocks.Length - 1;
                    if ((uint)lastIndex < (uint)freeBlocks.Length &&
                        freeBlocks[lastIndex].End == block.Offset)
                    {
                        // Free block begins at the end of last block; extend the last block.
                        freeBlocks[lastIndex].Size += block.Size;
                    }
                    else
                    {
                        // Free block is at the end of the list.
                        _freeBlocks.Add(block);
                    }
                }

#if ALLOC_TRACK
                RemoveAllocatedBlock(block.Offset);
#endif
            }


#if ALLOC_TRACK
            private bool MergeContiguousBlocks()
            {
                List<VkMemoryBlock> freeBlocks = _freeBlocks;
                bool hasMerged = false;
                int contiguousLength = 1;

                for (int i = 0; i < freeBlocks.Count - 1; i++)
                {
                    uint blockStart = freeBlocks[i].Offset;
                    while (i + contiguousLength < freeBlocks.Count
                        && freeBlocks[i + contiguousLength - 1].End == freeBlocks[i + contiguousLength].Offset)
                    {
                        contiguousLength += 1;
                    }

                    if (contiguousLength > 1)
                    {
                        ulong blockEnd = freeBlocks[i + contiguousLength - 1].End;
                        freeBlocks.RemoveRange(i, contiguousLength);

                        VkMemoryBlock mergedBlock = new(
                            Memory,
                            blockStart,
                            blockEnd - blockStart,
                            _memoryTypeIndex,
                            _mappedPtr,
                            false);
                        freeBlocks.Insert(i, mergedBlock);
                        hasMerged = true;
                        contiguousLength = 0;
                    }
                }

                return hasMerged;
            }

            private void CheckAllocatedBlock(uint offset, ulong size)
            {
                _allocatedBlocks.Add(offset, size); // Throws on same key added twice.

                int index = _allocatedBlocks.IndexOfKey(offset);

                if (index > 0)
                {
                    uint leftOffset = _allocatedBlocks.Keys[index - 1];
                    ulong leftSize = _allocatedBlocks.Values[index - 1];
                    TrackAssert(!BlocksOverlap(offset, size, leftOffset, leftSize), "Allocated segments have overlapped.");
                }

                if (index < _allocatedBlocks.Count - 1)
                {
                    uint rightOffset = _allocatedBlocks.Keys[index + 1];
                    ulong rightSize = _allocatedBlocks.Values[index + 1];
                    TrackAssert(!BlocksOverlap(offset, size, rightOffset, rightSize), "Allocated segments have overlapped.");
                }
            }

            private static bool BlocksOverlap(uint firstOffset, ulong firstSize, uint secondOffset, ulong secondSize)
            {
                ulong firstStart = firstOffset;
                ulong firstEnd = firstOffset + firstSize;
                ulong secondStart = secondOffset;
                ulong secondEnd = secondOffset + secondSize;

                return (firstStart <= secondStart && firstEnd > secondStart
                    || firstStart >= secondStart && firstEnd <= secondEnd
                    || firstStart < secondEnd && firstEnd >= secondEnd
                    || firstStart <= secondStart && firstEnd >= secondEnd);
            }

            private void RemoveAllocatedBlock(uint offset)
            {
                TrackAssert(_allocatedBlocks.Remove(offset), "Unable to remove a supposedly allocated block.");
            }

            private static void TrackAssert(bool condition, string message)
            {
                if (!condition)
                {
                    ThrowTrackException(message);
                }
            }

            private static void ThrowTrackException(string message)
            {
                throw new InvalidOperationException(message);
            }
#endif

            public bool IsFullFreeBlock()
            {
                if (_freeBlocks.Count == 1)
                {
                    return IsFirstFullFreeBlock();
                }
                return false;
            }

            private bool IsFirstFullFreeBlock()
            {
                ref VkMemoryBlock freeBlock = ref MemoryMarshal.GetReference(CollectionsMarshal.AsSpan(_freeBlocks));
                return freeBlock.Offset == 0
                    && freeBlock.Size == _totalMemorySize;
            }

            public void Dispose()
            {
                vkFreeMemory(_device, Memory, null);
            }
        }

        public void Dispose()
        {
            foreach (KeyValuePair<uint, ChunkAllocatorSet> kvp in _allocatorsByMemoryType)
            {
                kvp.Value.Dispose();
            }

            foreach (KeyValuePair<uint, ChunkAllocatorSet> kvp in _allocatorsByMemoryTypeUnmapped)
            {
                kvp.Value.Dispose();
            }
        }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal unsafe struct VkMemoryBlock : IEquatable<VkMemoryBlock>
    {
        private readonly uint MemoryType;
        public readonly VkDeviceMemory DeviceMemory;
        public readonly void* BaseMappedPointer;

        public uint Offset;
        public ulong Size;

        public readonly void* BlockMappedPointer => ((byte*)BaseMappedPointer) + Offset;
        public readonly bool IsPersistentMapped => BaseMappedPointer != null;
        public readonly ulong End => Offset + Size;
        public readonly uint MemoryTypeIndex => MemoryType & 31;
        public readonly bool DedicatedAllocation => ((MemoryType >> 5) & 1) != 0;

        public VkMemoryBlock(
            VkDeviceMemory memory,
            uint offset,
            ulong size,
            uint memoryTypeIndex,
            void* mappedPtr,
            bool dedicatedAllocation)
        {
            DeviceMemory = memory;
            Offset = offset;
            Size = size;
            MemoryType = memoryTypeIndex;
            BaseMappedPointer = mappedPtr;

            if (dedicatedAllocation)
                MemoryType |= 1u << 5;
        }

        public readonly bool Equals(VkMemoryBlock other)
        {
            return DeviceMemory.Equals(other.DeviceMemory)
                && Offset.Equals(other.Offset)
                && Size.Equals(other.Size);
        }

        private string GetDebuggerDisplay()
        {
            return $"[Mem:{DeviceMemory.Value:x}] Off:{Offset}, Size:{Size}, End:{Offset + Size}";
        }
    }
}
