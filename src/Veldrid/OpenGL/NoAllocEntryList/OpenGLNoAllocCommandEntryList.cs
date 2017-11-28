using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal unsafe class OpenGLNoAllocCommandEntryList : OpenGLCommandEntryList, IDisposable
    {
        private readonly StagingMemoryPool _memoryPool = new StagingMemoryPool();
        private readonly List<EntryStorageBlock> _blocks = new List<EntryStorageBlock>();
        private EntryStorageBlock _currentBlock;
        private uint _totalEntries;

        // Entry IDs
        private const byte BeginEntryID = 1;
        private static readonly uint BeginEntrySize = Util.USizeOf<NoAllocBeginEntry>();

        private const byte ClearColorTargetID = 2;
        private static readonly uint ClearColorTargetEntrySize = Util.USizeOf<NoAllocClearColorTargetEntry>();

        private const byte ClearDepthTargetID = 3;
        private static readonly uint ClearDepthTargetEntrySize = Util.USizeOf<NoAllocClearDepthTargetEntry>();

        private const byte DrawIndexedEntryID = 4;
        private static readonly uint DrawIndexedEntrySize = Util.USizeOf<NoAllocDrawIndexedEntry>();

        private const byte EndEntryID = 5;
        private static readonly uint EndEntrySize = Util.USizeOf<NoAllocEndEntry>();

        private const byte SetFramebufferEntryID = 6;
        private static readonly uint SetFramebufferEntrySize = Util.USizeOf<NoAllocSetFramebufferEntry>();

        private const byte SetIndexBufferEntryID = 7;
        private static readonly uint SetIndexBufferEntrySize = Util.USizeOf<NoAllocSetIndexBufferEntry>();

        private const byte SetPipelineEntryID = 8;
        private static readonly uint SetPipelineEntrySize = Util.USizeOf<NoAllocSetPipelineEntry>();

        private const byte SetGraphicsResourceSetEntryID = 9;
        private static readonly uint SetGraphicsResourceSetEntrySize = Util.USizeOf<NoAllocSetGraphicsResourceSetEntry>();

        private const byte SetScissorRectEntryID = 10;
        private static readonly uint SetScissorRectEntrySize = Util.USizeOf<NoAllocSetScissorRectEntry>();

        private const byte SetVertexBufferEntryID = 11;
        private static readonly uint SetVertexBufferEntrySize = Util.USizeOf<NoAllocSetVertexBufferEntry>();

        private const byte SetViewportEntryID = 12;
        private static readonly uint SetViewportEntrySize = Util.USizeOf<NoAllocSetViewportEntry>();

        private const byte UpdateBufferEntryID = 13;
        private static readonly uint UpdateBufferEntrySize = Util.USizeOf<NoAllocUpdateBufferEntry>();

        private const byte CopyBufferEntryID = 14;
        private static readonly uint CopyBufferEntrySize = Util.USizeOf<NoAllocCopyBufferEntry>();

        private const byte CopyTextureEntryID = 15;
        private static readonly uint CopyTextureEntrySize = Util.USizeOf<NoAllocCopyTextureEntry>();

        private const byte ResolveTextureEntryID = 16;
        private static readonly uint ResolveTextureEntrySize = Util.USizeOf<NoAllocResolveTextureEntry>();

        private const byte DrawEntryID = 17;
        private static readonly uint DrawEntrySize = Util.USizeOf<NoAllocDrawEntry>();

        private const byte DispatchEntryID = 18;
        private static readonly uint DispatchEntrySize = Util.USizeOf<NoAllocDispatchEntry>();

        private const byte SetComputeResourceSetEntryID = 19;
        private static readonly uint SetComputeResourceSetEntrySize = Util.USizeOf<NoAllocSetComputeResourceSetEntry>();

        private const byte DrawIndirectEntryID = 20;
        private static readonly uint DrawIndirectEntrySize = Util.USizeOf<NoAllocDrawIndirectEntry>();

        private const byte DrawIndexedIndirectEntryID = 21;
        private static readonly uint DrawIndexedIndirectEntrySize = Util.USizeOf<NoAllocDrawIndexedIndirectEntry>();

        private const byte DispatchIndirectEntryID = 22;
        private static readonly uint DispatchIndirectEntrySize = Util.USizeOf<NoAllocDispatchIndirectEntry>();

        public OpenGLNoAllocCommandEntryList()
        {
            _currentBlock = EntryStorageBlock.New();
            _blocks.Add(_currentBlock);
        }

        public void Reset()
        {
            FreeAllHandles();
            _totalEntries = 0;
            _currentBlock = _blocks[0];
            foreach (EntryStorageBlock block in _blocks)
            {
                block.Clear();
            }
        }

        public void Dispose()
        {
            FreeAllHandles();
            _totalEntries = 0;
            _currentBlock = _blocks[0];
            foreach (EntryStorageBlock block in _blocks)
            {
                block.Clear();
                block.Free();
            }
        }

        public void* GetStorageChunk(uint size, out byte* terminatorWritePtr)
        {
            terminatorWritePtr = null;
            if (!_currentBlock.Alloc(size, out void* ptr))
            {
                int currentBlockIndex = _blocks.IndexOf(_currentBlock);
                bool anyWorked = false;
                for (int i = currentBlockIndex + 1; i < _blocks.Count; i++)
                {
                    EntryStorageBlock nextBlock = _blocks[i];
                    if (nextBlock.Alloc(size, out ptr))
                    {
                        _currentBlock = nextBlock;
                        anyWorked = true;
                        break;
                    }
                }

                if (!anyWorked)
                {
                    _currentBlock = EntryStorageBlock.New();
                    _blocks.Add(_currentBlock);
                    bool result = _currentBlock.Alloc(size, out ptr);
                    Debug.Assert(result);
                }
            }
            if (_currentBlock.RemainingSize > size)
            {
                terminatorWritePtr = (byte*)ptr + size;
            }

            return ptr;
        }

        public void AddEntry<T>(byte id, ref T entry) where T : struct
        {
            uint size = Util.USizeOf<T>();
            AddEntry(id, size, ref entry);
        }

        public void AddEntry<T>(byte id, uint sizeOfT, ref T entry) where T : struct
        {
            Debug.Assert(sizeOfT == Unsafe.SizeOf<T>());
            uint storageSize = sizeOfT + 1; // Include ID
            void* storagePtr = GetStorageChunk(storageSize, out byte* terminatorWritePtr);
            Unsafe.Write(storagePtr, id);
            Unsafe.Write((byte*)storagePtr + 1, entry);
            if (terminatorWritePtr != null)
            {
                *terminatorWritePtr = 0;
            }
            _totalEntries += 1;
        }

        public void ExecuteAll(OpenGLCommandExecutor executor)
        {
            int currentBlockIndex = 0;
            EntryStorageBlock block = _blocks[currentBlockIndex];
            uint currentOffset = 0;
            for (uint i = 0; i < _totalEntries; i++)
            {
                if (currentOffset == block.TotalSize)
                {
                    currentBlockIndex += 1;
                    block = _blocks[currentBlockIndex];
                    currentOffset = 0;
                }

                uint id = Unsafe.Read<byte>(block.BasePtr + currentOffset);
                if (id == 0)
                {
                    currentBlockIndex += 1;
                    block = _blocks[currentBlockIndex];
                    currentOffset = 0;
                    id = Unsafe.Read<byte>(block.BasePtr + currentOffset);
                }

                Debug.Assert(id != 0);
                currentOffset += 1;
                byte* entryBasePtr = block.BasePtr + currentOffset;
                switch (id)
                {
                    case BeginEntryID:
                        executor.Begin();
                        currentOffset += BeginEntrySize;
                        break;
                    case ClearColorTargetID:
                        ref NoAllocClearColorTargetEntry ccte = ref Unsafe.AsRef<NoAllocClearColorTargetEntry>(entryBasePtr);
                        executor.ClearColorTarget(ccte.Index, ccte.ClearColor);
                        currentOffset += ClearColorTargetEntrySize;
                        break;
                    case ClearDepthTargetID:
                        ref NoAllocClearDepthTargetEntry cdte = ref Unsafe.AsRef<NoAllocClearDepthTargetEntry>(entryBasePtr);
                        executor.ClearDepthTarget(cdte.Depth);
                        currentOffset += ClearDepthTargetEntrySize;
                        break;
                    case DrawEntryID:
                        ref NoAllocDrawEntry de = ref Unsafe.AsRef<NoAllocDrawEntry>(entryBasePtr);
                        executor.Draw(de.VertexCount, de.InstanceCount, de.VertexStart, de.InstanceStart);
                        currentOffset += DrawEntrySize;
                        break;
                    case DrawIndexedEntryID:
                        ref NoAllocDrawIndexedEntry die = ref Unsafe.AsRef<NoAllocDrawIndexedEntry>(entryBasePtr);
                        executor.DrawIndexed(die.IndexCount, die.InstanceCount, die.IndexStart, die.VertexOffset, die.InstanceCount);
                        currentOffset += DrawIndexedEntrySize;
                        break;
                    case DrawIndirectEntryID:
                        ref NoAllocDrawIndirectEntry drawIndirectEntry = ref Unsafe.AsRef<NoAllocDrawIndirectEntry>(entryBasePtr);
                        executor.DrawIndirect(
                            drawIndirectEntry.IndirectBuffer,
                            drawIndirectEntry.Offset,
                            drawIndirectEntry.DrawCount,
                            drawIndirectEntry.Stride);
                        currentOffset += DrawIndirectEntrySize;
                        break;
                    case DrawIndexedIndirectEntryID:
                        ref NoAllocDrawIndexedIndirectEntry diie = ref Unsafe.AsRef<NoAllocDrawIndexedIndirectEntry>(entryBasePtr);
                        executor.DrawIndexedIndirect(diie.IndirectBuffer, diie.Offset, diie.DrawCount, diie.Stride);
                        currentOffset += DrawIndexedIndirectEntrySize;
                        break;
                    case DispatchEntryID:
                        ref NoAllocDispatchEntry dispatchEntry = ref Unsafe.AsRef<NoAllocDispatchEntry>(entryBasePtr);
                        executor.Dispatch(dispatchEntry.GroupCountX, dispatchEntry.GroupCountY, dispatchEntry.GroupCountZ);
                        currentOffset += DispatchEntrySize;
                        break;
                    case DispatchIndirectEntryID:
                        ref NoAllocDispatchIndirectEntry dispatchIndir = ref Unsafe.AsRef<NoAllocDispatchIndirectEntry>(entryBasePtr);
                        executor.DispatchIndirect(dispatchIndir.IndirectBuffer, dispatchIndir.Offset);
                        currentOffset += DispatchIndirectEntrySize;
                        break;
                    case EndEntryID:
                        executor.End();
                        currentOffset += EndEntrySize;
                        break;
                    case SetFramebufferEntryID:
                        ref NoAllocSetFramebufferEntry sfbe = ref Unsafe.AsRef<NoAllocSetFramebufferEntry>(entryBasePtr);
                        executor.SetFramebuffer(sfbe.Framebuffer);
                        currentOffset += SetFramebufferEntrySize;
                        break;
                    case SetIndexBufferEntryID:
                        ref NoAllocSetIndexBufferEntry sibe = ref Unsafe.AsRef<NoAllocSetIndexBufferEntry>(entryBasePtr);
                        executor.SetIndexBuffer(sibe.Buffer.Item, sibe.Format);
                        currentOffset += SetIndexBufferEntrySize;
                        break;
                    case SetPipelineEntryID:
                        ref NoAllocSetPipelineEntry spe = ref Unsafe.AsRef<NoAllocSetPipelineEntry>(entryBasePtr);
                        executor.SetPipeline(spe.Pipeline);
                        currentOffset += SetPipelineEntrySize;
                        break;
                    case SetGraphicsResourceSetEntryID:
                        ref NoAllocSetGraphicsResourceSetEntry sgrse = ref Unsafe.AsRef<NoAllocSetGraphicsResourceSetEntry>(entryBasePtr);
                        executor.SetGraphicsResourceSet(sgrse.Slot, sgrse.ResourceSet);
                        currentOffset += SetGraphicsResourceSetEntrySize;
                        break;
                    case SetComputeResourceSetEntryID:
                        ref NoAllocSetComputeResourceSetEntry scrse = ref Unsafe.AsRef<NoAllocSetComputeResourceSetEntry>(entryBasePtr);
                        executor.SetComputeResourceSet(scrse.Slot, scrse.ResourceSet);
                        currentOffset += SetComputeResourceSetEntrySize;
                        break;
                    case SetScissorRectEntryID:
                        ref NoAllocSetScissorRectEntry ssre = ref Unsafe.AsRef<NoAllocSetScissorRectEntry>(entryBasePtr);
                        executor.SetScissorRect(ssre.Index, ssre.X, ssre.Y, ssre.Width, ssre.Height);
                        currentOffset += SetScissorRectEntrySize;
                        break;
                    case SetVertexBufferEntryID:
                        ref NoAllocSetVertexBufferEntry svbe = ref Unsafe.AsRef<NoAllocSetVertexBufferEntry>(entryBasePtr);
                        executor.SetVertexBuffer(svbe.Index, svbe.Buffer.Item);
                        currentOffset += SetVertexBufferEntrySize;
                        break;
                    case SetViewportEntryID:
                        ref NoAllocSetViewportEntry svpe = ref Unsafe.AsRef<NoAllocSetViewportEntry>(entryBasePtr);
                        executor.SetViewport(svpe.Index, ref svpe.Viewport);
                        currentOffset += SetViewportEntrySize;
                        break;
                    case UpdateBufferEntryID:
                        ref NoAllocUpdateBufferEntry ube = ref Unsafe.AsRef<NoAllocUpdateBufferEntry>(entryBasePtr);
                        fixed (byte* dataPtr = &ube.StagingBlock.Array[0])
                        {
                            executor.UpdateBuffer(
                                ube.Buffer.Item,
                                ube.BufferOffsetInBytes,
                                (IntPtr)dataPtr, ube.StagingBlock.SizeInBytes);
                        }
                        currentOffset += UpdateBufferEntrySize;
                        break;
                    case CopyBufferEntryID:
                        ref NoAllocCopyBufferEntry cbe = ref Unsafe.AsRef<NoAllocCopyBufferEntry>(entryBasePtr);
                        executor.CopyBuffer(
                            cbe.Source,
                            cbe.SourceOffset,
                            cbe.Destination,
                            cbe.DestinationOffset,
                            cbe.SizeInBytes);
                        currentOffset += CopyBufferEntrySize;
                        break;
                    case CopyTextureEntryID:
                        ref NoAllocCopyTextureEntry cte = ref Unsafe.AsRef<NoAllocCopyTextureEntry>(entryBasePtr);
                        executor.CopyTexture(
                            cte.Source,
                            cte.SrcX, cte.SrcY, cte.SrcZ,
                            cte.SrcMipLevel,
                            cte.SrcBaseArrayLayer,
                            cte.Destination,
                            cte.DstX, cte.DstY, cte.DstZ,
                            cte.DstMipLevel,
                            cte.DstBaseArrayLayer,
                            cte.Width, cte.Height, cte.Depth,
                            cte.LayerCount);
                        currentOffset += CopyTextureEntrySize;
                        break;
                    case ResolveTextureEntryID:
                        ref NoAllocResolveTextureEntry rte = ref Unsafe.AsRef<NoAllocResolveTextureEntry>(entryBasePtr);
                        executor.ResolveTexture(rte.Source.Item, rte.Destination.Item);
                        currentOffset += ResolveTextureEntrySize;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid entry ID: " + id);
                }
            }
        }

        private void FreeAllHandles()
        {
            int currentBlockIndex = 0;
            EntryStorageBlock block = _blocks[currentBlockIndex];
            uint currentOffset = 0;
            for (uint i = 0; i < _totalEntries; i++)
            {
                if (currentOffset == block.TotalSize)
                {
                    currentBlockIndex += 1;
                    block = _blocks[currentBlockIndex];
                    currentOffset = 0;
                }

                uint id = Unsafe.Read<byte>(block.BasePtr + currentOffset);
                if (id == 0)
                {
                    currentBlockIndex += 1;
                    block = _blocks[currentBlockIndex];
                    currentOffset = 0;
                    id = Unsafe.Read<byte>(block.BasePtr + currentOffset);
                }

                Debug.Assert(id != 0);
                currentOffset += 1;
                byte* entryBasePtr = block.BasePtr + currentOffset;
                switch (id)
                {
                    case BeginEntryID:
                        currentOffset += BeginEntrySize;
                        break;
                    case ClearColorTargetID:
                        currentOffset += ClearColorTargetEntrySize;
                        break;
                    case ClearDepthTargetID:
                        currentOffset += ClearDepthTargetEntrySize;
                        break;
                    case DrawEntryID:
                        currentOffset += DrawEntrySize;
                        break;
                    case DrawIndexedEntryID:
                        currentOffset += DrawIndexedEntrySize;
                        break;
                    case DrawIndirectEntryID:
                        ref NoAllocDrawIndirectEntry drawIndirectEntry = ref Unsafe.AsRef<NoAllocDrawIndirectEntry>(entryBasePtr);
                        drawIndirectEntry.IndirectBuffer.Free();
                        currentOffset += DrawIndirectEntrySize;
                        break;
                    case DrawIndexedIndirectEntryID:
                        ref NoAllocDrawIndexedIndirectEntry diie = ref Unsafe.AsRef<NoAllocDrawIndexedIndirectEntry>(entryBasePtr);
                        diie.IndirectBuffer.Free();
                        currentOffset += DrawIndexedIndirectEntrySize;
                        break;
                    case DispatchEntryID:
                        currentOffset += DispatchEntrySize;
                        break;
                    case DispatchIndirectEntryID:
                        ref NoAllocDispatchIndirectEntry dispatchIndirect = ref Unsafe.AsRef<NoAllocDispatchIndirectEntry>(entryBasePtr);
                        dispatchIndirect.IndirectBuffer.Free();
                        currentOffset += DispatchIndirectEntrySize;
                        break;
                    case EndEntryID:
                        currentOffset += EndEntrySize;
                        break;
                    case SetFramebufferEntryID:
                        ref NoAllocSetFramebufferEntry sfbe = ref Unsafe.AsRef<NoAllocSetFramebufferEntry>(entryBasePtr);
                        sfbe.Framebuffer.Free();
                        currentOffset += SetFramebufferEntrySize;
                        break;
                    case SetIndexBufferEntryID:
                        ref NoAllocSetIndexBufferEntry sibe = ref Unsafe.AsRef<NoAllocSetIndexBufferEntry>(entryBasePtr);
                        sibe.Buffer.Free();
                        currentOffset += SetIndexBufferEntrySize;
                        break;
                    case SetPipelineEntryID:
                        ref NoAllocSetPipelineEntry spe = ref Unsafe.AsRef<NoAllocSetPipelineEntry>(entryBasePtr);
                        spe.Pipeline.Free();
                        currentOffset += SetPipelineEntrySize;
                        break;
                    case SetGraphicsResourceSetEntryID:
                        ref NoAllocSetGraphicsResourceSetEntry sgrse = ref Unsafe.AsRef<NoAllocSetGraphicsResourceSetEntry>(entryBasePtr);
                        sgrse.ResourceSet.Free();
                        currentOffset += SetGraphicsResourceSetEntrySize;
                        break;
                    case SetComputeResourceSetEntryID:
                        ref NoAllocSetComputeResourceSetEntry scrse = ref Unsafe.AsRef<NoAllocSetComputeResourceSetEntry>(entryBasePtr);
                        scrse.ResourceSet.Free();
                        currentOffset += SetComputeResourceSetEntrySize;
                        break;
                    case SetScissorRectEntryID:
                        currentOffset += SetScissorRectEntrySize;
                        break;
                    case SetVertexBufferEntryID:
                        ref NoAllocSetVertexBufferEntry svbe = ref Unsafe.AsRef<NoAllocSetVertexBufferEntry>(entryBasePtr);
                        svbe.Buffer.Free();
                        currentOffset += SetVertexBufferEntrySize;
                        break;
                    case SetViewportEntryID:
                        currentOffset += SetViewportEntrySize;
                        break;
                    case UpdateBufferEntryID:
                        ref NoAllocUpdateBufferEntry ube = ref Unsafe.AsRef<NoAllocUpdateBufferEntry>(entryBasePtr);
                        ube.Buffer.Free();
                        ube.StagingBlock.GCHandle.Free();
                        currentOffset += UpdateBufferEntrySize;
                        break;
                    case CopyBufferEntryID:
                        ref NoAllocCopyBufferEntry cbe = ref Unsafe.AsRef<NoAllocCopyBufferEntry>(entryBasePtr);
                        cbe.Source.Free();
                        cbe.Destination.Free();
                        currentOffset += CopyBufferEntrySize;
                        break;
                    case CopyTextureEntryID:
                        ref NoAllocCopyTextureEntry cte = ref Unsafe.AsRef<NoAllocCopyTextureEntry>(entryBasePtr);
                        cte.Source.Free();
                        cte.Destination.Free();
                        currentOffset += CopyTextureEntrySize;
                        break;
                    case ResolveTextureEntryID:
                        ref NoAllocResolveTextureEntry rte = ref Unsafe.AsRef<NoAllocResolveTextureEntry>(entryBasePtr);
                        rte.Source.Free();
                        rte.Destination.Free();
                        currentOffset += ResolveTextureEntrySize;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid entry ID: " + id);
                }
            }
        }

        public void Begin()
        {
            NoAllocBeginEntry entry = new NoAllocBeginEntry();
            AddEntry(BeginEntryID, ref entry);
        }

        public void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            NoAllocClearColorTargetEntry entry = new NoAllocClearColorTargetEntry(index, clearColor);
            AddEntry(ClearColorTargetID, ref entry);
        }

        public void ClearDepthTarget(float depth)
        {
            NoAllocClearDepthTargetEntry entry = new NoAllocClearDepthTargetEntry(depth);
            AddEntry(ClearDepthTargetID, ref entry);
        }

        public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            NoAllocDrawEntry entry = new NoAllocDrawEntry(vertexCount, instanceCount, vertexStart, instanceStart);
            AddEntry(DrawEntryID, ref entry);
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            NoAllocDrawIndexedEntry entry = new NoAllocDrawIndexedEntry(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
            AddEntry(DrawIndexedEntryID, ref entry);
        }

        public void DrawIndirect(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            NoAllocDrawIndirectEntry entry = new NoAllocDrawIndirectEntry(indirectBuffer, offset, drawCount, stride);
            AddEntry(DrawIndirectEntryID, ref entry);
        }

        public void DrawIndexedIndirect(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            NoAllocDrawIndexedIndirectEntry entry = new NoAllocDrawIndexedIndirectEntry(indirectBuffer, offset, drawCount, stride);
        }

        public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            NoAllocDispatchEntry entry = new NoAllocDispatchEntry(groupCountX, groupCountY, groupCountZ);
            AddEntry(DispatchEntryID, ref entry);
        }

        public void DispatchIndirect(Buffer indirectBuffer, uint offset)
        {
            NoAllocDispatchIndirectEntry entry = new NoAllocDispatchIndirectEntry(indirectBuffer, offset);
            AddEntry(DispatchIndirectEntryID, ref entry);
        }

        public void End()
        {
            NoAllocEndEntry entry = new NoAllocEndEntry();
            AddEntry(EndEntryID, ref entry);
        }

        public void SetFramebuffer(Framebuffer fb)
        {
            NoAllocSetFramebufferEntry entry = new NoAllocSetFramebufferEntry(fb);
            AddEntry(SetFramebufferEntryID, ref entry);
        }

        public void SetIndexBuffer(Buffer buffer, IndexFormat format)
        {
            NoAllocSetIndexBufferEntry entry = new NoAllocSetIndexBufferEntry(buffer, format);
            AddEntry(SetIndexBufferEntryID, ref entry);
        }

        public void SetPipeline(Pipeline pipeline)
        {
            NoAllocSetPipelineEntry entry = new NoAllocSetPipelineEntry(pipeline);
            AddEntry(SetPipelineEntryID, ref entry);
        }

        public void SetGraphicsResourceSet(uint slot, ResourceSet rs)
        {
            NoAllocSetGraphicsResourceSetEntry entry = new NoAllocSetGraphicsResourceSetEntry(slot, rs);
            AddEntry(SetGraphicsResourceSetEntryID, ref entry);
        }

        public void SetComputeResourceSet(uint slot, ResourceSet rs)
        {
            NoAllocSetComputeResourceSetEntry entry = new NoAllocSetComputeResourceSetEntry(slot, rs);
            AddEntry(SetComputeResourceSetEntryID, ref entry);
        }

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            NoAllocSetScissorRectEntry entry = new NoAllocSetScissorRectEntry(index, x, y, width, height);
            AddEntry(SetScissorRectEntryID, ref entry);
        }

        public void SetVertexBuffer(uint index, Buffer buffer)
        {
            NoAllocSetVertexBufferEntry entry = new NoAllocSetVertexBufferEntry(index, buffer);
            AddEntry(SetVertexBufferEntryID, ref entry);
        }

        public void SetViewport(uint index, ref Viewport viewport)
        {
            NoAllocSetViewportEntry entry = new NoAllocSetViewportEntry(index, ref viewport);
            AddEntry(SetViewportEntryID, ref entry);
        }

        public void ResolveTexture(Texture source, Texture destination)
        {
            NoAllocResolveTextureEntry entry = new NoAllocResolveTextureEntry(source, destination);
            AddEntry(ResolveTextureEntryID, ref entry);
        }

        public void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            NoAllocUpdateBufferEntry entry = new NoAllocUpdateBufferEntry(buffer, bufferOffsetInBytes, stagingBlock);
            AddEntry(UpdateBufferEntryID, ref entry);
        }

        public void CopyBuffer(Buffer source, uint sourceOffset, Buffer destination, uint destinationOffset, uint sizeInBytes)
        {
            NoAllocCopyBufferEntry entry = new NoAllocCopyBufferEntry(
                source,
                sourceOffset,
                destination,
                destinationOffset,
                sizeInBytes);
            AddEntry(CopyBufferEntryID, ref entry);
        }

        public void CopyTexture(
            Texture source,
            uint srcX, uint srcY, uint srcZ,
            uint srcMipLevel,
            uint srcBaseArrayLayer,
            Texture destination,
            uint dstX, uint dstY, uint dstZ,
            uint dstMipLevel,
            uint dstBaseArrayLayer,
            uint width, uint height, uint depth,
            uint layerCount)
        {
            NoAllocCopyTextureEntry entry = new NoAllocCopyTextureEntry(
                source,
                srcX, srcY, srcZ,
                srcMipLevel,
                srcBaseArrayLayer,
                destination,
                dstX, dstY, dstZ,
                dstMipLevel,
                dstBaseArrayLayer,
                width, height, depth,
                layerCount);
            AddEntry(CopyTextureEntryID, ref entry);
        }

        private struct EntryStorageBlock : IEquatable<EntryStorageBlock>
        {
            private const int DefaultStorageBlockSize = 40000;
            private readonly byte[] _bytes;
            private readonly GCHandle _gcHandle;
            public readonly byte* BasePtr;

            private uint _unusedStart;
            public uint RemainingSize => (uint)_bytes.Length - _unusedStart;

            public uint TotalSize => (uint)_bytes.Length;

            public bool Alloc(uint size, out void* ptr)
            {
                if (RemainingSize < size)
                {
                    ptr = null;
                    return false;
                }
                else
                {
                    ptr = (BasePtr + _unusedStart);
                    _unusedStart += size;
                    return true;
                }
            }

            private EntryStorageBlock(int storageBlockSize)
            {
                _bytes = new byte[storageBlockSize];
                _gcHandle = GCHandle.Alloc(_bytes, GCHandleType.Pinned);
                BasePtr = (byte*)_gcHandle.AddrOfPinnedObject().ToPointer();
                _unusedStart = 0;
            }

            public static EntryStorageBlock New()
            {
                return new EntryStorageBlock(DefaultStorageBlockSize);
            }

            public void Free()
            {
                _gcHandle.Free();
            }

            internal void Clear()
            {
                _unusedStart = 0;
                Util.ClearArray(_bytes);
            }

            public bool Equals(EntryStorageBlock other)
            {
                return _bytes == other._bytes;
            }
        }
    }

    internal struct HandleTracked<T> where T : class
    {
        public T Item => (T)_gcHandle.Target;
        private readonly GCHandle _gcHandle;

        public HandleTracked(T item)
        {
            _gcHandle = GCHandle.Alloc(item);
        }

        public void Free()
        {
            _gcHandle.Free();
        }

        public static implicit operator HandleTracked<T>(T item) => new HandleTracked<T>(item);
        public static implicit operator T(HandleTracked<T> tracked) => tracked.Item;
    }
}
