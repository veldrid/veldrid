using Vulkan;
using static Vulkan.VulkanNative;
using static Veldrid.Vk.VulkanUtil;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Veldrid.Vk
{
    internal unsafe class VkDeviceMemoryManager : IDisposable
    {
        private readonly VkDevice _device;
        private readonly VkPhysicalDevice _physicalDevice;
        private readonly ulong _bufferImageGranularity;
        private readonly ulong _chunkGranularity;
        private readonly object _allocatorMutex = new object();
        private ulong _totalAllocatedBytes;
        private readonly Dictionary<uint, ChunkAllocatorSet> _allocatorsByMemoryTypeUnmapped = new Dictionary<uint, ChunkAllocatorSet>();
        private readonly Dictionary<uint, ChunkAllocatorSet> _allocatorsByMemoryType = new Dictionary<uint, ChunkAllocatorSet>();

        private readonly vkGetBufferMemoryRequirements2_t _getBufferMemoryRequirements2;
        private readonly vkGetImageMemoryRequirements2_t _getImageMemoryRequirements2;

        public VkDeviceMemoryManager(
            VkDevice device,
            VkPhysicalDevice physicalDevice,
            ulong bufferImageGranularity,
            ulong chunkGranularity,
            vkGetBufferMemoryRequirements2_t getBufferMemoryRequirements2,
            vkGetImageMemoryRequirements2_t getImageMemoryRequirements2)
        {
            _device = device;
            _physicalDevice = physicalDevice;
            _bufferImageGranularity = bufferImageGranularity;
            _chunkGranularity = chunkGranularity;
            _getBufferMemoryRequirements2 = getBufferMemoryRequirements2;
            _getImageMemoryRequirements2 = getImageMemoryRequirements2;
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
                VkImage.Null,
                Vulkan.VkBuffer.Null);
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
            Vulkan.VkBuffer dedicatedBuffer)
        {
            if (!TryFindMemoryType(memProperties, memoryTypeBits, flags, out uint memoryTypeIndex))
            {
                throw new VeldridException("No suitable memory type.");
            }

            ulong minDedicatedAllocationSize = persistentMapped
                ? ChunkAllocator.PersistentMappedChunkSize
                : ChunkAllocator.UnmappedChunkSize;

            if (dedicated || size >= minDedicatedAllocationSize)
            {
                if (dedicatedImage == VkImage.Null && dedicatedBuffer == Vulkan.VkBuffer.Null)
                {
                    // Round up to the nearest multiple of bufferImageGranularity.
                    size = ((size + _bufferImageGranularity - 1) / _bufferImageGranularity) * _bufferImageGranularity;
                }
                Interlocked.Add(ref _totalAllocatedBytes, size);

                VkMemoryAllocateInfo allocateInfo = VkMemoryAllocateInfo.New();
                allocateInfo.allocationSize = size;
                allocateInfo.memoryTypeIndex = memoryTypeIndex;

                VkMemoryDedicatedAllocateInfoKHR dedicatedAI;
                if (dedicated)
                {
                    dedicatedAI = VkMemoryDedicatedAllocateInfoKHR.New();
                    dedicatedAI.buffer = dedicatedBuffer;
                    dedicatedAI.image = dedicatedImage;
                    allocateInfo.pNext = &dedicatedAI;
                }

                VkResult allocationResult = vkAllocateMemory(_device, ref allocateInfo, null, out VkDeviceMemory memory);
                if (allocationResult != VkResult.Success)
                {
                    throw new VeldridException("Unable to allocate sufficient Vulkan memory.");
                }

                void* mappedPtr = null;
                if (persistentMapped)
                {
                    VkResult mapResult = vkMapMemory(_device, memory, 0, size, 0, &mappedPtr);
                    if (mapResult != VkResult.Success)
                    {
                        throw new VeldridException("Unable to map newly-allocated Vulkan memory.");
                    }
                }

                return new VkMemoryBlock(memory, 0, size, memoryTypeBits, mappedPtr, true);
            }
            else
            {
                size = ((size / _chunkGranularity) + 1) * _chunkGranularity;
                Interlocked.Add(ref _totalAllocatedBytes, size);

                ChunkAllocatorSet allocator = GetAllocator(memoryTypeIndex, persistentMapped);
                bool result = allocator.Allocate((uint)size, (uint)alignment, out VkMemoryBlock ret);
                if (!result)
                {
                    throw new VeldridException("Unable to allocate sufficient Vulkan memory.");
                }

                return ret;
            }
        }

        public void Free(VkMemoryBlock block)
        {
            Interlocked.Add(ref _totalAllocatedBytes, (ulong)-(long)block.Size);

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
                ChunkAllocatorSet ret;

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

        private class ChunkAllocatorSet : IDisposable
        {
            private readonly VkDevice _device;
            private readonly uint _memoryTypeIndex;
            private readonly bool _persistentMapped;
            private readonly object _mutex = new object();
            private readonly List<ChunkAllocator> _allocators = new List<ChunkAllocator>();

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

                        // Allocate may have merged free blocks.
                        if (allocator.IsFullFreeBlock())
                        {
                            // The allocator is a free contiguous block so dispose it.
                            allocator.Dispose();

                            allocators.RemoveAt(i);
                            i--;
                        }
                    }

                    ChunkAllocator newAllocator = new ChunkAllocator(_device, _memoryTypeIndex, _persistentMapped);
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

        private class ChunkAllocator : IDisposable
        {
            public const ulong PersistentMappedChunkSize = 1024 * 1024;
            public const ulong UnmappedChunkSize = 1024 * 1024 * 8;

            private readonly VkDevice _device;
            private readonly uint _memoryTypeIndex;
            private readonly bool _persistentMapped;
            private readonly List<VkMemoryBlock> _freeBlocks = new List<VkMemoryBlock>();
            private readonly void* _mappedPtr;
            private readonly ulong _totalMemorySize;

            public readonly VkDeviceMemory Memory;

            public ChunkAllocator(VkDevice device, uint memoryTypeIndex, bool persistentMapped)
            {
                _device = device;
                _memoryTypeIndex = memoryTypeIndex;
                _persistentMapped = persistentMapped;
                _totalMemorySize = persistentMapped ? PersistentMappedChunkSize : UnmappedChunkSize;

                VkMemoryAllocateInfo memoryAI = VkMemoryAllocateInfo.New();
                memoryAI.allocationSize = _totalMemorySize;
                memoryAI.memoryTypeIndex = _memoryTypeIndex;
                VkResult result = vkAllocateMemory(_device, ref memoryAI, null, out Memory);
                CheckResult(result);

                void* mappedPtr = null;
                if (persistentMapped)
                {
                    result = vkMapMemory(_device, Memory, 0, _totalMemorySize, 0, &mappedPtr);
                    CheckResult(result);
                }
                _mappedPtr = mappedPtr;

                VkMemoryBlock initialBlock = new VkMemoryBlock(
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

                checked
                {
                    bool hasMergedBlocks = false;
                    do
                    {
                        Span<VkMemoryBlock> freeBlocks = CollectionsMarshal.AsSpan(_freeBlocks);

                        ulong alignedBlockSize = 0;
                        uint alignedOffsetRemainder = 0;

                        int i = 0;
                        int selectedIndex = -1;
                        for (; i < freeBlocks.Length; i++)
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
                            block.Size = alignedBlockSize;
                            if (alignedOffsetRemainder != 0)
                            {
                                block.Offset += alignment - alignedOffsetRemainder;
                            }

                            if (alignedBlockSize != size)
                            {
                                VkMemoryBlock splitBlock = new VkMemoryBlock(
                                    block.DeviceMemory,
                                    block.Offset + size,
                                    block.Size - size,
                                    _memoryTypeIndex,
                                    block.BaseMappedPointer,
                                    false);

                                freeBlocks[selectedIndex] = splitBlock;
                                block.Size = size;
                            }
                            else
                            {
                                _freeBlocks.RemoveAt(i);
                            }

#if DEBUG
                            CheckAllocatedBlock(block);
#endif
                            resultBlock = block;
                            return true;
                        }

                        if (hasMergedBlocks)
                        {
                            break;
                        }
                        hasMergedBlocks = MergeContiguousBlocks();
                    }
                    while (hasMergedBlocks);

                    resultBlock = default;
                    return false;
                }
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
                // Assume that _freeBlocks is always sorted.
                int precedingBlock = FindPrecedingBlockIndex(CollectionsMarshal.AsSpan(_freeBlocks), block.Offset);
                if (precedingBlock != -1)
                {
                    _freeBlocks.Insert(precedingBlock, block);
                }
                else
                {
                    _freeBlocks.Add(block);
                }

#if DEBUG
                RemoveAllocatedBlock(block);
#endif
            }

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

                        VkMemoryBlock mergedBlock = new VkMemoryBlock(
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

#if DEBUG
            private HashSet<VkMemoryBlock> _allocatedBlocks = new HashSet<VkMemoryBlock>();

            private void CheckAllocatedBlock(VkMemoryBlock block)
            {
                foreach (VkMemoryBlock oldBlock in _allocatedBlocks)
                {
                    Debug.Assert(!BlocksOverlap(block, oldBlock), "Allocated blocks have overlapped.");
                }

                Debug.Assert(_allocatedBlocks.Add(block), "Same block added twice.");
            }

            private bool BlocksOverlap(VkMemoryBlock first, VkMemoryBlock second)
            {
                ulong firstStart = first.Offset;
                ulong firstEnd = first.Offset + first.Size;
                ulong secondStart = second.Offset;
                ulong secondEnd = second.Offset + second.Size;

                return (firstStart <= secondStart && firstEnd > secondStart
                    || firstStart >= secondStart && firstEnd <= secondEnd
                    || firstStart < secondEnd && firstEnd >= secondEnd
                    || firstStart <= secondStart && firstEnd >= secondEnd);
            }

            private void RemoveAllocatedBlock(VkMemoryBlock block)
            {
                Debug.Assert(_allocatedBlocks.Remove(block), "Unable to remove a supposedly allocated block.");
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

        internal IntPtr Map(in VkMemoryBlock memoryBlock)
        {
            void* ret;
            VkResult result = vkMapMemory(_device, memoryBlock.DeviceMemory, memoryBlock.Offset, memoryBlock.Size, 0, &ret);
            CheckResult(result);
            return (IntPtr)ret;
        }
    }

    [DebuggerDisplay("[Mem:{DeviceMemory.Handle}] Off:{Offset}, Size:{Size} End:{Offset+Size}")]
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
    }
}
