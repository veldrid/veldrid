using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Vd2.OpenGL.NoAllocEntryList
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

        private const byte DrawEntryID = 4;
        private static readonly uint DrawEntrySize = Util.USizeOf<NoAllocDrawEntry>();

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

        private const byte UpdateTexture2DEntryID = 14;
        private static readonly uint UpdateTexture2DEntrySize = Util.USizeOf<NoAllocUpdateTexture2DEntry>();

        private const byte UpdateTextureCubeEntryID = 15;
        private static readonly uint UpdateTextureCubeEntrySize = Util.USizeOf<NoAllocUpdateTextureCubeEntry>();

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
                        executor.Draw(de.IndexCount, de.InstanceCount, de.IndexStart, de.VertexOffset, de.InstanceCount);
                        currentOffset += DrawEntrySize;
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
                        executor.SetIndexBuffer(sibe.IndexBuffer.Item);
                        currentOffset += SetIndexBufferEntrySize;
                        break;
                    case SetPipelineEntryID:
                        ref NoAllocSetPipelineEntry spe = ref Unsafe.AsRef<NoAllocSetPipelineEntry>(entryBasePtr);
                        executor.SetPipeline(spe.Pipeline);
                        currentOffset += SetPipelineEntrySize;
                        break;
                    case SetResourceSetEntryID:
                        ref NoAllocSetResourceSetEntry srse = ref Unsafe.AsRef<NoAllocSetResourceSetEntry>(entryBasePtr);
                        executor.SetResourceSet(srse.ResourceSet);
                        currentOffset += SetResourceSetEntrySize;
                        break;
                    case SetScissorRectEntryID:
                        ref NoAllocSetScissorRectEntry ssre = ref Unsafe.AsRef<NoAllocSetScissorRectEntry>(entryBasePtr);
                        executor.SetScissorRect(ssre.Index, ssre.X, ssre.Y, ssre.Width, ssre.Height);
                        currentOffset += SetScissorRectEntrySize;
                        break;
                    case SetVertexBufferEntryID:
                        ref NoAllocSetVertexBufferEntry svbe = ref Unsafe.AsRef<NoAllocSetVertexBufferEntry>(entryBasePtr);
                        executor.SetVertexBuffer(svbe.Index, svbe.VertexBuffer.Item);
                        currentOffset += SetVertexBufferEntrySize;
                        break;
                    case SetViewportEntryID:
                        ref NoAllocSetViewportEntry svpe = ref Unsafe.AsRef<NoAllocSetViewportEntry>(entryBasePtr);
                        executor.SetViewport(svpe.Index, ref svpe.Viewport);
                        currentOffset += SetViewportEntrySize;
                        break;
                    case UpdateBufferEntryID:
                        ref NoAllocUpdateBufferEntry ube = ref Unsafe.AsRef<NoAllocUpdateBufferEntry>(entryBasePtr);
                        executor.UpdateBuffer(ube.Buffer.Item, ube.BufferOffsetInBytes, ube.StagingBlock);
                        currentOffset += UpdateBufferEntrySize;
                        break;
                    case UpdateTexture2DEntryID:
                        ref NoAllocUpdateTexture2DEntry ut2de = ref Unsafe.AsRef<NoAllocUpdateTexture2DEntry>(entryBasePtr);
                        executor.UpdateTexture2D(
                            ut2de.Texture2D,
                            ut2de.StagingBlock,
                            ut2de.X,
                            ut2de.Y,
                            ut2de.Width,
                            ut2de.Height,
                            ut2de.MipLevel,
                            ut2de.ArrayLayer);
                        currentOffset += UpdateTexture2DEntrySize;
                        break;
                    case UpdateTextureCubeEntryID:
                        ref NoAllocUpdateTextureCubeEntry utce = ref Unsafe.AsRef<NoAllocUpdateTextureCubeEntry>(entryBasePtr);
                        executor.UpdateTextureCube(
                            utce.TextureCube,
                            utce.StagingBlock,
                            utce.Face,
                            utce.X,
                            utce.Y,
                            utce.Width,
                            utce.Height,
                            utce.MipLevel,
                            utce.ArrayLayer);
                        currentOffset += UpdateTextureCubeEntrySize;
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
                        sibe.IndexBuffer.Free();
                        currentOffset += SetIndexBufferEntrySize;
                        break;
                    case SetPipelineEntryID:
                        ref NoAllocSetPipelineEntry spe = ref Unsafe.AsRef<NoAllocSetPipelineEntry>(entryBasePtr);
                        spe.Pipeline.Free();
                        currentOffset += SetPipelineEntrySize;
                        break;
                    case SetResourceSetEntryID:
                        ref NoAllocSetResourceSetEntry srse = ref Unsafe.AsRef<NoAllocSetResourceSetEntry>(entryBasePtr);
                        srse.ResourceSet.Free();
                        currentOffset += SetResourceSetEntrySize;
                        break;
                    case SetScissorRectEntryID:
                        currentOffset += SetScissorRectEntrySize;
                        break;
                    case SetVertexBufferEntryID:
                        ref NoAllocSetVertexBufferEntry svbe = ref Unsafe.AsRef<NoAllocSetVertexBufferEntry>(entryBasePtr);
                        svbe.VertexBuffer.Free();
                        currentOffset += SetVertexBufferEntrySize;
                        break;
                    case SetViewportEntryID:
                        currentOffset += SetViewportEntrySize;
                        break;
                    case UpdateBufferEntryID:
                        ref NoAllocUpdateBufferEntry ube = ref Unsafe.AsRef<NoAllocUpdateBufferEntry>(entryBasePtr);
                        ube.Buffer.Free();
                        currentOffset += UpdateBufferEntrySize;
                        break;
                    case UpdateTexture2DEntryID:
                        ref NoAllocUpdateTexture2DEntry ut2de = ref Unsafe.AsRef<NoAllocUpdateTexture2DEntry>(entryBasePtr);
                        ut2de.Texture2D.Free();
                        currentOffset += UpdateTexture2DEntrySize;
                        break;
                    case UpdateTextureCubeEntryID:
                        ref NoAllocUpdateTextureCubeEntry utce = ref Unsafe.AsRef<NoAllocUpdateTextureCubeEntry>(entryBasePtr);
                        utce.TextureCube.Free();
                        currentOffset += UpdateTextureCubeEntrySize;
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

        public void Draw(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            NoAllocDrawEntry entry = new NoAllocDrawEntry(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
            AddEntry(DrawEntryID, ref entry);
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

        public void SetIndexBuffer(IndexBuffer ib)
        {
            NoAllocSetIndexBufferEntry entry = new NoAllocSetIndexBufferEntry(ib);
            AddEntry(SetIndexBufferEntryID, ref entry);
        }

        public void SetPipeline(Pipeline pipeline)
        {
            NoAllocSetPipelineEntry entry = new NoAllocSetPipelineEntry(pipeline);
            AddEntry(SetPipelineEntryID, ref entry);
        }

        public void SetResourceSet(ResourceSet rs)
        {
            NoAllocSetResourceSetEntry entry = new NoAllocSetResourceSetEntry(rs);
            AddEntry(SetResourceSetEntryID, ref entry);
        }

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            NoAllocSetScissorRectEntry entry = new NoAllocSetScissorRectEntry(index, x, y, width, height);
            AddEntry(SetScissorRectEntryID, ref entry);
        }

        public void SetVertexBuffer(uint index, VertexBuffer vb)
        {
            NoAllocSetVertexBufferEntry entry = new NoAllocSetVertexBufferEntry(index, vb);
            AddEntry(SetVertexBufferEntryID, ref entry);
        }

        public void SetViewport(uint index, ref Viewport viewport)
        {
            NoAllocSetViewportEntry entry = new NoAllocSetViewportEntry(index, ref viewport);
            AddEntry(SetViewportEntryID, ref entry);
        }

        public void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            NoAllocUpdateBufferEntry entry = new NoAllocUpdateBufferEntry(buffer, bufferOffsetInBytes, stagingBlock);
            AddEntry(UpdateBufferEntryID, ref entry);
        }

        public void UpdateTexture2D(
            Texture2D texture2D,
            IntPtr source,
            uint sizeInBytes,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            NoAllocUpdateTexture2DEntry entry = new NoAllocUpdateTexture2DEntry(
                texture2D,
                stagingBlock,
                x,
                y,
                width,
                height,
                mipLevel,
                arrayLayer);
            AddEntry(UpdateTexture2DEntryID, ref entry);
        }

        public void UpdateTextureCube(
            TextureCube textureCube,
            IntPtr source,
            uint sizeInBytes,
            CubeFace face,
            uint x,
            uint y,
            uint width,
            uint height,
            uint mipLevel,
            uint arrayLayer)
        {
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            NoAllocUpdateTextureCubeEntry entry = new NoAllocUpdateTextureCubeEntry(
                textureCube,
                stagingBlock,
                face,
                x,
                y,
                width,
                height,
                mipLevel,
                arrayLayer);
            AddEntry(UpdateTextureCubeEntryID, ref entry);
        }

        public void Dispose()
        {
            foreach (EntryStorageBlock block in _blocks)
            {
                block.Free();
            }
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
