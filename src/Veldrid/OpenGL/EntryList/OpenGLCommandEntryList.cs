using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Veldrid.OpenGL.NoAllocEntryList
{
    internal unsafe class OpenGLNoAllocCommandEntryList : OpenGLCommandEntryList, IDisposable
    {
        private readonly StagingMemoryPool _memoryPool;
        private readonly List<EntryStorageBlock> _blocks = new List<EntryStorageBlock>();
        private EntryStorageBlock _currentBlock;
        private uint _totalEntries;
        private readonly List<object> _resourceList = new List<object>();
        private readonly List<StagingBlock> _stagingBlocks = new List<StagingBlock>();

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

        private const byte SetResourceSetEntryID = 9;
        private static readonly uint SetResourceSetEntrySize = Util.USizeOf<NoAllocSetResourceSetEntry>();

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

        private const byte DrawIndirectEntryID = 20;
        private static readonly uint DrawIndirectEntrySize = Util.USizeOf<NoAllocDrawIndirectEntry>();

        private const byte DrawIndexedIndirectEntryID = 21;
        private static readonly uint DrawIndexedIndirectEntrySize = Util.USizeOf<NoAllocDrawIndexedIndirectEntry>();

        private const byte DispatchIndirectEntryID = 22;
        private static readonly uint DispatchIndirectEntrySize = Util.USizeOf<NoAllocDispatchIndirectEntry>();

        private const byte GenerateMipmapsEntryID = 23;
        private static readonly uint GenerateMipmapsEntrySize = Util.USizeOf<NoAllocGenerateMipmapsEntry>();

        private const byte PushDebugGroupEntryID = 24;
        private static readonly uint PushDebugGroupEntrySize = Util.USizeOf<NoAllocPushDebugGroupEntry>();

        private const byte PopDebugGroupEntryID = 25;
        private static readonly uint PopDebugGroupEntrySize = Util.USizeOf<NoAllocPopDebugGroupEntry>();

        private const byte InsertDebugMarkerEntryID = 26;
        private static readonly uint InsertDebugMarkerEntrySize = Util.USizeOf<NoAllocInsertDebugMarkerEntry>();

        public OpenGLCommandList Parent { get; }

        public OpenGLNoAllocCommandEntryList(OpenGLCommandList cl)
        {
            Parent = cl;
            _memoryPool = cl.Device.StagingMemoryPool;
            _currentBlock = EntryStorageBlock.New();
            _blocks.Add(_currentBlock);
        }

        public void Reset()
        {
            FlushStagingBlocks();
            _resourceList.Clear();
            _totalEntries = 0;
            _currentBlock = _blocks[0];
            foreach (EntryStorageBlock block in _blocks)
            {
                block.Clear();
            }
        }

        public void Dispose()
        {
            FlushStagingBlocks();
            _resourceList.Clear();
            _totalEntries = 0;
            _currentBlock = _blocks[0];
            foreach (EntryStorageBlock block in _blocks)
            {
                block.Clear();
                block.Free();
            }
        }

        private void FlushStagingBlocks()
        {
            StagingMemoryPool pool = _memoryPool;
            foreach (StagingBlock block in _stagingBlocks)
            {
                pool.Free(block);
            }

            _stagingBlocks.Clear();
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
                        NoAllocClearColorTargetEntry ccte = Unsafe.ReadUnaligned<NoAllocClearColorTargetEntry>(entryBasePtr);
                        executor.ClearColorTarget(ccte.Index, ccte.ClearColor);
                        currentOffset += ClearColorTargetEntrySize;
                        break;
                    case ClearDepthTargetID:
                        NoAllocClearDepthTargetEntry cdte = Unsafe.ReadUnaligned<NoAllocClearDepthTargetEntry>(entryBasePtr);
                        executor.ClearDepthStencil(cdte.Depth, cdte.Stencil);
                        currentOffset += ClearDepthTargetEntrySize;
                        break;
                    case DrawEntryID:
                        NoAllocDrawEntry de = Unsafe.ReadUnaligned<NoAllocDrawEntry>(entryBasePtr);
                        executor.Draw(de.VertexCount, de.InstanceCount, de.VertexStart, de.InstanceStart);
                        currentOffset += DrawEntrySize;
                        break;
                    case DrawIndexedEntryID:
                        NoAllocDrawIndexedEntry die = Unsafe.ReadUnaligned<NoAllocDrawIndexedEntry>(entryBasePtr);
                        executor.DrawIndexed(die.IndexCount, die.InstanceCount, die.IndexStart, die.VertexOffset, die.InstanceStart);
                        currentOffset += DrawIndexedEntrySize;
                        break;
                    case DrawIndirectEntryID:
                        NoAllocDrawIndirectEntry drawIndirectEntry = Unsafe.ReadUnaligned<NoAllocDrawIndirectEntry>(entryBasePtr);
                        executor.DrawIndirect(
                            drawIndirectEntry.IndirectBuffer.Get(_resourceList),
                            drawIndirectEntry.Offset,
                            drawIndirectEntry.DrawCount,
                            drawIndirectEntry.Stride);
                        currentOffset += DrawIndirectEntrySize;
                        break;
                    case DrawIndexedIndirectEntryID:
                        NoAllocDrawIndexedIndirectEntry diie = Unsafe.ReadUnaligned<NoAllocDrawIndexedIndirectEntry>(entryBasePtr);
                        executor.DrawIndexedIndirect(diie.IndirectBuffer.Get(_resourceList), diie.Offset, diie.DrawCount, diie.Stride);
                        currentOffset += DrawIndexedIndirectEntrySize;
                        break;
                    case DispatchEntryID:
                        NoAllocDispatchEntry dispatchEntry = Unsafe.ReadUnaligned<NoAllocDispatchEntry>(entryBasePtr);
                        executor.Dispatch(dispatchEntry.GroupCountX, dispatchEntry.GroupCountY, dispatchEntry.GroupCountZ);
                        currentOffset += DispatchEntrySize;
                        break;
                    case DispatchIndirectEntryID:
                        NoAllocDispatchIndirectEntry dispatchIndir = Unsafe.ReadUnaligned<NoAllocDispatchIndirectEntry>(entryBasePtr);
                        executor.DispatchIndirect(dispatchIndir.IndirectBuffer.Get(_resourceList), dispatchIndir.Offset);
                        currentOffset += DispatchIndirectEntrySize;
                        break;
                    case EndEntryID:
                        executor.End();
                        currentOffset += EndEntrySize;
                        break;
                    case SetFramebufferEntryID:
                        NoAllocSetFramebufferEntry sfbe = Unsafe.ReadUnaligned<NoAllocSetFramebufferEntry>(entryBasePtr);
                        executor.SetFramebuffer(sfbe.Framebuffer.Get(_resourceList));
                        currentOffset += SetFramebufferEntrySize;
                        break;
                    case SetIndexBufferEntryID:
                        NoAllocSetIndexBufferEntry sibe = Unsafe.ReadUnaligned<NoAllocSetIndexBufferEntry>(entryBasePtr);
                        executor.SetIndexBuffer(sibe.Buffer.Get(_resourceList), sibe.Format, sibe.Offset);
                        currentOffset += SetIndexBufferEntrySize;
                        break;
                    case SetPipelineEntryID:
                        NoAllocSetPipelineEntry spe = Unsafe.ReadUnaligned<NoAllocSetPipelineEntry>(entryBasePtr);
                        executor.SetPipeline(spe.Pipeline.Get(_resourceList));
                        currentOffset += SetPipelineEntrySize;
                        break;
                    case SetResourceSetEntryID:
                        NoAllocSetResourceSetEntry srse = Unsafe.ReadUnaligned<NoAllocSetResourceSetEntry>(entryBasePtr);
                        ResourceSet rs = srse.ResourceSet.Get(_resourceList);
                        uint* dynamicOffsetsPtr = srse.DynamicOffsetCount > NoAllocSetResourceSetEntry.MaxInlineDynamicOffsets
                            ? (uint*)srse.DynamicOffsets_Block.Data
                            : srse.DynamicOffsets_Inline;
                        if (srse.IsGraphics)
                        {
                            executor.SetGraphicsResourceSet(
                                srse.Slot,
                                rs,
                                srse.DynamicOffsetCount,
                                ref Unsafe.AsRef<uint>(dynamicOffsetsPtr));
                        }
                        else
                        {
                            executor.SetComputeResourceSet(
                                srse.Slot,
                                rs,
                                srse.DynamicOffsetCount,
                                ref Unsafe.AsRef<uint>(dynamicOffsetsPtr));
                        }
                        currentOffset += SetResourceSetEntrySize;
                        break;
                    case SetScissorRectEntryID:
                        NoAllocSetScissorRectEntry ssre = Unsafe.ReadUnaligned<NoAllocSetScissorRectEntry>(entryBasePtr);
                        executor.SetScissorRect(ssre.Index, ssre.X, ssre.Y, ssre.Width, ssre.Height);
                        currentOffset += SetScissorRectEntrySize;
                        break;
                    case SetVertexBufferEntryID:
                        NoAllocSetVertexBufferEntry svbe = Unsafe.ReadUnaligned<NoAllocSetVertexBufferEntry>(entryBasePtr);
                        executor.SetVertexBuffer(svbe.Index, svbe.Buffer.Get(_resourceList), svbe.Offset);
                        currentOffset += SetVertexBufferEntrySize;
                        break;
                    case SetViewportEntryID:
                        NoAllocSetViewportEntry svpe = Unsafe.ReadUnaligned<NoAllocSetViewportEntry>(entryBasePtr);
                        executor.SetViewport(svpe.Index, ref svpe.Viewport);
                        currentOffset += SetViewportEntrySize;
                        break;
                    case UpdateBufferEntryID:
                        NoAllocUpdateBufferEntry ube = Unsafe.ReadUnaligned<NoAllocUpdateBufferEntry>(entryBasePtr);
                        byte* dataPtr = (byte*)ube.StagingBlock.Data;
                        executor.UpdateBuffer(
                            ube.Buffer.Get(_resourceList),
                            ube.BufferOffsetInBytes,
                            (IntPtr)dataPtr, ube.StagingBlockSize);
                        currentOffset += UpdateBufferEntrySize;
                        break;
                    case CopyBufferEntryID:
                        NoAllocCopyBufferEntry cbe = Unsafe.ReadUnaligned<NoAllocCopyBufferEntry>(entryBasePtr);
                        executor.CopyBuffer(
                            cbe.Source.Get(_resourceList),
                            cbe.SourceOffset,
                            cbe.Destination.Get(_resourceList),
                            cbe.DestinationOffset,
                            cbe.SizeInBytes);
                        currentOffset += CopyBufferEntrySize;
                        break;
                    case CopyTextureEntryID:
                        NoAllocCopyTextureEntry cte = Unsafe.ReadUnaligned<NoAllocCopyTextureEntry>(entryBasePtr);
                        executor.CopyTexture(
                            cte.Source.Get(_resourceList),
                            cte.SrcX, cte.SrcY, cte.SrcZ,
                            cte.SrcMipLevel,
                            cte.SrcBaseArrayLayer,
                            cte.Destination.Get(_resourceList),
                            cte.DstX, cte.DstY, cte.DstZ,
                            cte.DstMipLevel,
                            cte.DstBaseArrayLayer,
                            cte.Width, cte.Height, cte.Depth,
                            cte.LayerCount);
                        currentOffset += CopyTextureEntrySize;
                        break;
                    case ResolveTextureEntryID:
                        NoAllocResolveTextureEntry rte = Unsafe.ReadUnaligned<NoAllocResolveTextureEntry>(entryBasePtr);
                        executor.ResolveTexture(rte.Source.Get(_resourceList), rte.Destination.Get(_resourceList));
                        currentOffset += ResolveTextureEntrySize;
                        break;
                    case GenerateMipmapsEntryID:
                        NoAllocGenerateMipmapsEntry gme = Unsafe.ReadUnaligned<NoAllocGenerateMipmapsEntry>(entryBasePtr);
                        executor.GenerateMipmaps(gme.Texture.Get(_resourceList));
                        currentOffset += GenerateMipmapsEntrySize;
                        break;
                    case PushDebugGroupEntryID:
                        NoAllocPushDebugGroupEntry pdge = Unsafe.ReadUnaligned<NoAllocPushDebugGroupEntry>(entryBasePtr);
                        executor.PushDebugGroup(pdge.Name.Get(_resourceList));
                        currentOffset += PushDebugGroupEntrySize;
                        break;
                    case PopDebugGroupEntryID:
                        executor.PopDebugGroup();
                        currentOffset += PopDebugGroupEntrySize;
                        break;
                    case InsertDebugMarkerEntryID:
                        NoAllocInsertDebugMarkerEntry idme = Unsafe.ReadUnaligned<NoAllocInsertDebugMarkerEntry>(entryBasePtr);
                        executor.InsertDebugMarker(idme.Name.Get(_resourceList));
                        currentOffset += InsertDebugMarkerEntrySize;
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

        public void ClearDepthTarget(float depth, byte stencil)
        {
            NoAllocClearDepthTargetEntry entry = new NoAllocClearDepthTargetEntry(depth, stencil);
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

        public void DrawIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            NoAllocDrawIndirectEntry entry = new NoAllocDrawIndirectEntry(Track(indirectBuffer), offset, drawCount, stride);
            AddEntry(DrawIndirectEntryID, ref entry);
        }

        public void DrawIndexedIndirect(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            NoAllocDrawIndexedIndirectEntry entry = new NoAllocDrawIndexedIndirectEntry(Track(indirectBuffer), offset, drawCount, stride);
            AddEntry(DrawIndexedIndirectEntryID, ref entry);
        }

        public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            NoAllocDispatchEntry entry = new NoAllocDispatchEntry(groupCountX, groupCountY, groupCountZ);
            AddEntry(DispatchEntryID, ref entry);
        }

        public void DispatchIndirect(DeviceBuffer indirectBuffer, uint offset)
        {
            NoAllocDispatchIndirectEntry entry = new NoAllocDispatchIndirectEntry(Track(indirectBuffer), offset);
            AddEntry(DispatchIndirectEntryID, ref entry);
        }

        public void End()
        {
            NoAllocEndEntry entry = new NoAllocEndEntry();
            AddEntry(EndEntryID, ref entry);
        }

        public void SetFramebuffer(Framebuffer fb)
        {
            NoAllocSetFramebufferEntry entry = new NoAllocSetFramebufferEntry(Track(fb));
            AddEntry(SetFramebufferEntryID, ref entry);
        }

        public void SetIndexBuffer(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            NoAllocSetIndexBufferEntry entry = new NoAllocSetIndexBufferEntry(Track(buffer), format, offset);
            AddEntry(SetIndexBufferEntryID, ref entry);
        }

        public void SetPipeline(Pipeline pipeline)
        {
            NoAllocSetPipelineEntry entry = new NoAllocSetPipelineEntry(Track(pipeline));
            AddEntry(SetPipelineEntryID, ref entry);
        }

        public void SetGraphicsResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets)
        {
            SetResourceSet(slot, rs, dynamicOffsetCount, ref dynamicOffsets, isGraphics: true);
        }

        private void SetResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets, bool isGraphics)
        {
            NoAllocSetResourceSetEntry entry;

            if (dynamicOffsetCount > NoAllocSetResourceSetEntry.MaxInlineDynamicOffsets)
            {
                StagingBlock block = _memoryPool.GetStagingBlock(dynamicOffsetCount * sizeof(uint));
                _stagingBlocks.Add(block);
                for (uint i = 0; i < dynamicOffsetCount; i++)
                {
                    *((uint*)block.Data + i) = Unsafe.Add(ref dynamicOffsets, (int)i);
                }

                entry = new NoAllocSetResourceSetEntry(slot, Track(rs), isGraphics, block);
            }
            else
            {
                entry = new NoAllocSetResourceSetEntry(slot, Track(rs), isGraphics, dynamicOffsetCount, ref dynamicOffsets);
            }

            AddEntry(SetResourceSetEntryID, ref entry);
        }

        public void SetComputeResourceSet(uint slot, ResourceSet rs, uint dynamicOffsetCount, ref uint dynamicOffsets)
        {
            SetResourceSet(slot, rs, dynamicOffsetCount, ref dynamicOffsets, isGraphics: false);
        }

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            NoAllocSetScissorRectEntry entry = new NoAllocSetScissorRectEntry(index, x, y, width, height);
            AddEntry(SetScissorRectEntryID, ref entry);
        }

        public void SetVertexBuffer(uint index, DeviceBuffer buffer, uint offset)
        {
            NoAllocSetVertexBufferEntry entry = new NoAllocSetVertexBufferEntry(index, Track(buffer), offset);
            AddEntry(SetVertexBufferEntryID, ref entry);
        }

        public void SetViewport(uint index, ref Viewport viewport)
        {
            NoAllocSetViewportEntry entry = new NoAllocSetViewportEntry(index, ref viewport);
            AddEntry(SetViewportEntryID, ref entry);
        }

        public void ResolveTexture(Texture source, Texture destination)
        {
            NoAllocResolveTextureEntry entry = new NoAllocResolveTextureEntry(Track(source), Track(destination));
            AddEntry(ResolveTextureEntryID, ref entry);
        }

        public void UpdateBuffer(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            _stagingBlocks.Add(stagingBlock);
            NoAllocUpdateBufferEntry entry = new NoAllocUpdateBufferEntry(Track(buffer), bufferOffsetInBytes, stagingBlock, sizeInBytes);
            AddEntry(UpdateBufferEntryID, ref entry);
        }

        public void CopyBuffer(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            NoAllocCopyBufferEntry entry = new NoAllocCopyBufferEntry(
                Track(source),
                sourceOffset,
                Track(destination),
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
                Track(source),
                srcX, srcY, srcZ,
                srcMipLevel,
                srcBaseArrayLayer,
                Track(destination),
                dstX, dstY, dstZ,
                dstMipLevel,
                dstBaseArrayLayer,
                width, height, depth,
                layerCount);
            AddEntry(CopyTextureEntryID, ref entry);
        }

        public void GenerateMipmaps(Texture texture)
        {
            NoAllocGenerateMipmapsEntry entry = new NoAllocGenerateMipmapsEntry(Track(texture));
            AddEntry(GenerateMipmapsEntryID, ref entry);
        }

        public void PushDebugGroup(string name)
        {
            NoAllocPushDebugGroupEntry entry = new NoAllocPushDebugGroupEntry(Track(name));
            AddEntry(PushDebugGroupEntryID, ref entry);
        }

        public void PopDebugGroup()
        {
            NoAllocPopDebugGroupEntry entry = new NoAllocPopDebugGroupEntry();
            AddEntry(PopDebugGroupEntryID, ref entry);
        }

        public void InsertDebugMarker(string name)
        {
            NoAllocInsertDebugMarkerEntry entry = new NoAllocInsertDebugMarkerEntry(Track(name));
            AddEntry(InsertDebugMarkerEntryID, ref entry);
        }

        private Tracked<T> Track<T>(T item) where T : class
        {
            return new Tracked<T>(_resourceList, item);
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

    /// <summary>
    /// A handle for an object stored in some List.
    /// </summary>
    /// <typeparam name="T">The type of object to track.</typeparam>
    internal struct Tracked<T> where T : class
    {
        private readonly int _index;

        public T Get(List<object> list) => (T)list[_index];

        public Tracked(List<object> list, T item)
        {
            _index = list.Count;
            list.Add(item);
        }
    }
}
