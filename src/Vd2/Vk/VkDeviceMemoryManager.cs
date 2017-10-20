using Vulkan;
using static Vulkan.VulkanNative;
using static Vd2.Vk.VulkanUtil;
using System.Collections.Generic;
using System.Diagnostics;

namespace Vd2.Vk
{
    public unsafe class VkDeviceMemoryManager
    {
        private readonly VkDevice _device;
        private readonly VkPhysicalDevice _physicalDevice;

        private readonly Dictionary<uint, ChunkAllocatorSet> _allocatorsByMemoryType = new Dictionary<uint, ChunkAllocatorSet>();

        public VkDeviceMemoryManager(VkDevice device, VkPhysicalDevice physicalDevice)
        {
            _device = device;
            _physicalDevice = physicalDevice;
        }

        public VkMemoryBlock Allocate(uint memoryTypeIndex, ulong size, ulong alignment)
        {
            ChunkAllocatorSet allocator = GetAllocator(memoryTypeIndex);
            bool result = allocator.Allocate(size, alignment, out VkMemoryBlock ret);
            if (!result)
            {
                throw new VdException("Unable to allocate memory.");
            }

            return ret;
        }

        public void Free(VkMemoryBlock block)
        {
            GetAllocator(block.MemoryTypeIndex).Free(block);
        }

        private ChunkAllocatorSet GetAllocator(uint memoryTypeIndex)
        {
            if (!_allocatorsByMemoryType.TryGetValue(memoryTypeIndex, out ChunkAllocatorSet ret))
            {
                ret = new ChunkAllocatorSet(_device, memoryTypeIndex);
                _allocatorsByMemoryType.Add(memoryTypeIndex, ret);
            }

            return ret;
        }

        private class ChunkAllocatorSet
        {
            private readonly VkDevice _device;
            private readonly uint _memoryTypeIndex;
            private readonly List<ChunkAllocator> _allocators = new List<ChunkAllocator>();

            public ChunkAllocatorSet(VkDevice device, uint memoryTypeIndex)
            {
                _device = device;
                _memoryTypeIndex = memoryTypeIndex;
            }

            public bool Allocate(ulong size, ulong alignment, out VkMemoryBlock block)
            {
                foreach (ChunkAllocator allocator in _allocators)
                {
                    if (allocator.Allocate(size, alignment, out block))
                    {
                        return true;
                    }
                }

                ChunkAllocator newAllocator = new ChunkAllocator(_device, _memoryTypeIndex);
                _allocators.Add(newAllocator);
                return newAllocator.Allocate(size, alignment, out block);
            }

            public void Free(VkMemoryBlock block)
            {
                foreach (ChunkAllocator chunk in _allocators)
                {
                    if (chunk.Memory == block.DeviceMemory)
                    {
                        chunk.Free(block);
                    }
                }
            }
        }

        private class ChunkAllocator
        {
            private readonly VkDevice _device;
            private readonly uint _memoryTypeIndex;
            private readonly List<VkMemoryBlock> _freeBlocks = new List<VkMemoryBlock>();
            private readonly VkDeviceMemory _memory;

            private ulong _totalMemorySize = 1024 * 1024 * 256; // 256 MB
            private ulong _totalAllocatedBytes = 0;

            public VkDeviceMemory Memory => _memory;

            public ChunkAllocator(VkDevice device, uint memoryTypeIndex)
            {
                _device = device;
                _memoryTypeIndex = memoryTypeIndex;

                VkMemoryAllocateInfo memoryAI = VkMemoryAllocateInfo.New();
                memoryAI.allocationSize = _totalMemorySize;
                memoryAI.memoryTypeIndex = _memoryTypeIndex;
                VkResult result = vkAllocateMemory(_device, ref memoryAI, null, out _memory);
                CheckResult(result);

                VkMemoryBlock initialBlock = new VkMemoryBlock(_memory, 0, _totalMemorySize, _memoryTypeIndex);
                _freeBlocks.Add(initialBlock);
            }

            public bool Allocate(ulong size, ulong alignment, out VkMemoryBlock block)
            {
                checked
                {
                    for (int i = 0; i < _freeBlocks.Count; i++)
                    {
                        VkMemoryBlock freeBlock = _freeBlocks[i];
                        ulong alignedBlockSize = freeBlock.Size;
                        if (freeBlock.Offset % alignment != 0)
                        {
                            ulong alignmentCorrection = (alignment - freeBlock.Offset % alignment);
                            if (alignedBlockSize <= alignmentCorrection)
                            {
                                continue;
                            }
                            alignedBlockSize -= alignmentCorrection;
                        }

                        if (alignedBlockSize >= size) // Valid match -- split it and return.
                        {
                            _freeBlocks.Remove(freeBlock);

                            freeBlock.Size = alignedBlockSize;
                            if ((freeBlock.Offset % alignment) != 0)
                            {
                                freeBlock.Offset += alignment - (freeBlock.Offset % alignment);
                            }

                            block = freeBlock;

                            if (alignedBlockSize != size)
                            {
                                VkMemoryBlock splitBlock = new VkMemoryBlock(
                                    freeBlock.DeviceMemory,
                                    freeBlock.Offset + size,
                                    freeBlock.Size - size,
                                    _memoryTypeIndex);
                                _freeBlocks.Add(splitBlock);
                                block = freeBlock;
                                block.Size = size;
                            }

#if DEBUG
                            CheckAllocatedBlock(block);
#endif
                            _totalAllocatedBytes += alignedBlockSize;
                            return true;
                        }
                    }

                    block = null;
                    return false;
                }
            }

            public void Free(VkMemoryBlock block)
            {
                _freeBlocks.Add(block);
#if DEBUG
                RemoveAllocatedBlock(block);
#endif
            }

#if DEBUG
            private List<VkMemoryBlock> _allocatedBlocks = new List<VkMemoryBlock>();

            private void CheckAllocatedBlock(VkMemoryBlock block)
            {
                foreach (VkMemoryBlock oldBlock in _allocatedBlocks)
                {
                    Debug.Assert(!BlocksOverlap(block, oldBlock), "Allocated blocks have overlapped.");
                }

                _allocatedBlocks.Add(block);
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
        }
    }

    [DebuggerDisplay("[Mem:{DeviceMemory.Handle}] Off:{Offset}, Size:{Size}")]
    public class VkMemoryBlock
    {
        public readonly uint MemoryTypeIndex;
        public readonly VkDeviceMemory DeviceMemory;

        public ulong Offset;
        public ulong Size;

        public VkMemoryBlock(VkDeviceMemory memory, ulong offset, ulong size, uint memoryTypeIndex)
        {
            DeviceMemory = memory;
            Offset = offset;
            Size = size;
            MemoryTypeIndex = memoryTypeIndex;
        }
    }
}
