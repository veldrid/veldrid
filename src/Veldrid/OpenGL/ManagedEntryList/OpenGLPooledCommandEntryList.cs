#if OPENGL_MANAGED_COMMAND_ENTRY_LIST
using System;
using System.Collections.Generic;

namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class OpenGLPooledCommandEntryList : OpenGLCommandEntryList
    {
        private readonly List<OpenGLCommandEntry> _commands = new List<OpenGLCommandEntry>();
        private readonly StagingMemoryPool _memoryPool = new StagingMemoryPool();

        private readonly EntryPool<BeginEntry> _beginEntryPool = new EntryPool<BeginEntry>();
        private readonly EntryPool<ClearColorTargetEntry> _clearColorTargetEntryPool = new EntryPool<ClearColorTargetEntry>();
        private readonly EntryPool<ClearDepthTargetEntry> _clearDepthTargetEntryPool = new EntryPool<ClearDepthTargetEntry>();
        private readonly EntryPool<DrawEntry> _drawEntryPool = new EntryPool<DrawEntry>();
        private readonly EntryPool<DrawIndexedEntry> _drawIndexedEntryPool = new EntryPool<DrawIndexedEntry>();
        private readonly EntryPool<DispatchEntry> _dispatchEntryPool = new EntryPool<DispatchEntry>();
        private readonly EntryPool<EndEntry> _endEntryPool = new EntryPool<EndEntry>();
        private readonly EntryPool<SetFramebufferEntry> _setFramebufferEntryPool = new EntryPool<SetFramebufferEntry>();
        private readonly EntryPool<SetIndexBufferEntry> _setIndexBufferEntryPool = new EntryPool<SetIndexBufferEntry>();
        private readonly EntryPool<SetPipelineEntry> _setPipelineEntryPool = new EntryPool<SetPipelineEntry>();
        private readonly EntryPool<SetGraphicsResourceSetEntry> _setGraphicsResourceSetEntryPool = new EntryPool<SetGraphicsResourceSetEntry>();
        private readonly EntryPool<SetComputeResourceSetEntry> _setComputeResourceSetEntryPool = new EntryPool<SetComputeResourceSetEntry>();
        private readonly EntryPool<SetScissorRectEntry> _setScissorRectEntryPool = new EntryPool<SetScissorRectEntry>();
        private readonly EntryPool<SetVertexBufferEntry> _setVertexBufferEntryPool = new EntryPool<SetVertexBufferEntry>();
        private readonly EntryPool<SetViewportEntry> _setViewportEntryPool = new EntryPool<SetViewportEntry>();
        private readonly EntryPool<UpdateBufferEntry> _updateBufferEntryPool = new EntryPool<UpdateBufferEntry>();
        private readonly EntryPool<UpdateTextureEntry> _updateTextureEntryPool = new EntryPool<UpdateTextureEntry>();
        private readonly EntryPool<UpdateTextureCubeEntry> _updateTextureCubeEntryPool = new EntryPool<UpdateTextureCubeEntry>();
        private readonly EntryPool<ResolveTextureEntry> _resolveTextureEntryPool = new EntryPool<ResolveTextureEntry>();
        private readonly EntryPool<DrawIndirectEntry> _drawIndirectEntryPool = new EntryPool<DrawIndirectEntry>();
        private readonly EntryPool<DrawIndexedIndirectEntry> _drawIndexedIndirectEntryPool = new EntryPool<DrawIndexedIndirectEntry>();
        private readonly EntryPool<DispatchIndirectEntry> _dispatchIndirectEntryPool = new EntryPool<DispatchIndirectEntry>();

        public IReadOnlyList<OpenGLCommandEntry> Commands => _commands;

        public void Reset()
        {
            _commands.Clear();
        }

        public void Begin()
        {
            _commands.Add(_beginEntryPool.Rent());
        }

        public void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            _commands.Add(_clearColorTargetEntryPool.Rent().Init(index, clearColor));
        }

        public void ClearDepthTarget(float depth)
        {
            _commands.Add(_clearDepthTargetEntryPool.Rent().Init(depth));
        }

        public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            _commands.Add(_drawEntryPool.Rent().Init(vertexCount, instanceCount, vertexStart, instanceStart));
        }

        public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            _commands.Add(_dispatchEntryPool.Rent().Init(groupCountX, groupCountY, groupCountZ));
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            _commands.Add(_drawIndexedEntryPool.Rent().Init(indexCount, instanceCount, indexStart, vertexOffset, instanceStart));
        }

        public void DrawIndirect(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            _commands.Add(_drawIndirectEntryPool.Rent().Init(indirectBuffer, offset, drawCount, stride));
        }

        public void DrawIndexedIndirect(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            _commands.Add(_drawIndexedIndirectEntryPool.Rent().Init(indirectBuffer, offset, drawCount, stride));
        }

        public void DispatchIndirect(Buffer indirectBuffer, uint offset)
        {
            _commands.Add(_dispatchIndirectEntryPool.Rent().Init(indirectBuffer, offset));
        }

        public void End()
        {
            _commands.Add(_endEntryPool.Rent());
        }

        public void SetFramebuffer(Framebuffer fb)
        {
            _commands.Add(_setFramebufferEntryPool.Rent().Init(fb));
        }

        public void SetIndexBuffer(Buffer buffer, IndexFormat format)
        {
            _commands.Add(_setIndexBufferEntryPool.Rent().Init(buffer, format));
        }

        public void SetPipeline(Pipeline pipeline)
        {
            _commands.Add(_setPipelineEntryPool.Rent().Init(pipeline));
        }

        public void SetGraphicsResourceSet(uint slot, ResourceSet rs)
        {
            _commands.Add(_setGraphicsResourceSetEntryPool.Rent().Init(slot, rs));
        }

        public void SetComputeResourceSet(uint slot, ResourceSet rs)
        {
            _commands.Add(_setComputeResourceSetEntryPool.Rent().Init(slot, rs));
        }

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            _commands.Add(_setScissorRectEntryPool.Rent().Init(index, x, y, width, height));
        }

        public void SetVertexBuffer(uint index, Buffer vb)
        {
            _commands.Add(_setVertexBufferEntryPool.Rent().Init(index, vb));
        }

        public void SetViewport(uint index, ref Viewport viewport)
        {
            _commands.Add(_setViewportEntryPool.Rent().Init(index, ref viewport));
        }

        public void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            _commands.Add(_updateBufferEntryPool.Rent().Init(buffer, bufferOffsetInBytes, stagingBlock));
        }

        public void UpdateTexture(
            Texture texture,
            IntPtr source,
            uint sizeInBytes,
            uint x,
            uint y,
            uint z,
            uint width,
            uint height,
            uint depth,
            uint mipLevel,
            uint arrayLayer)
        {
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            _commands.Add(_updateTextureEntryPool.Rent().Init(texture, stagingBlock, x, y, z, width, height, depth, mipLevel, arrayLayer));
        }

        public void UpdateTextureCube(
            Texture textureCube,
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
            _commands.Add(_updateTextureCubeEntryPool.Rent().Init(textureCube, stagingBlock, face, x, y, width, height, mipLevel, arrayLayer));
        }

        public void ResolveTexture(Texture source, Texture destination)
        {
            _commands.Add(_resolveTextureEntryPool.Rent().Init(source, destination));
        }

        public unsafe void ExecuteAll(OpenGLCommandExecutor executor)
        {
            foreach (OpenGLCommandEntry entry in _commands)
            {
                switch (entry)
                {
                    case BeginEntry be:
                        executor.Begin();
                        _beginEntryPool.Return(be);
                        break;
                    case ClearColorTargetEntry ccte:
                        executor.ClearColorTarget(ccte.Index, ccte.ClearColor);
                        _clearColorTargetEntryPool.Return(ccte);
                        break;
                    case ClearDepthTargetEntry cdte:
                        executor.ClearDepthTarget(cdte.Depth);
                        _clearDepthTargetEntryPool.Return(cdte);
                        break;
                    case DrawEntry de:
                        executor.Draw(de.VertexCount, de.InstanceCount, de.VertexStart, de.InstanceStart);
                        _drawEntryPool.Return(de);
                        break;
                    case DrawIndexedEntry die:
                        executor.DrawIndexed(die.IndexCount, die.InstanceCount, die.IndexStart, die.VertexOffset, die.InstanceCount);
                        _drawIndexedEntryPool.Return(die);
                        break;
                    case DrawIndirectEntry dInd:
                        executor.DrawIndirect(dInd.IndirectBuffer, dInd.Offset, dInd.DrawCount, dInd.Stride);
                        _drawIndirectEntryPool.Return(dInd);
                        break;
                    case DrawIndexedIndirectEntry dIdxInd:
                        executor.DrawIndexedIndirect(dIdxInd.IndirectBuffer, dIdxInd.Offset, dIdxInd.DrawCount, dIdxInd.Stride);
                        _drawIndexedIndirectEntryPool.Return(dIdxInd);
                        break;
                    case DispatchEntry dispatchEntry:
                        executor.Dispatch(dispatchEntry.GroupCountX, dispatchEntry.GroupCountY, dispatchEntry.GroupCountZ);
                        _dispatchEntryPool.Return(dispatchEntry);
                        break;
                    case DispatchIndirectEntry dispInd:
                        executor.DispatchIndirect(dispInd.IndirectBuffer, dispInd.Offset);
                        _dispatchIndirectEntryPool.Return(dispInd);
                        break;
                    case EndEntry ee:
                        executor.End();
                        _endEntryPool.Return(ee);
                        break;
                    case SetFramebufferEntry sfbe:
                        executor.SetFramebuffer(sfbe.Framebuffer);
                        _setFramebufferEntryPool.Return(sfbe);
                        break;
                    case SetIndexBufferEntry sibe:
                        executor.SetIndexBuffer(sibe.Buffer, sibe.Format);
                        _setIndexBufferEntryPool.Return(sibe);
                        break;
                    case SetPipelineEntry spe:
                        executor.SetPipeline(spe.Pipeline);
                        _setPipelineEntryPool.Return(spe);
                        break;
                    case SetGraphicsResourceSetEntry sgrse:
                        executor.SetGraphicsResourceSet(sgrse.Slot, sgrse.ResourceSet);
                        _setGraphicsResourceSetEntryPool.Return(sgrse);
                        break;
                    case SetComputeResourceSetEntry scrse:
                        executor.SetComputeResourceSet(scrse.Slot, scrse.ResourceSet);
                        _setComputeResourceSetEntryPool.Return(scrse);
                        break;
                    case SetScissorRectEntry ssre:
                        executor.SetScissorRect(ssre.Index, ssre.X, ssre.Y, ssre.Width, ssre.Height);
                        _setScissorRectEntryPool.Return(ssre);
                        break;
                    case SetVertexBufferEntry svbe:
                        executor.SetVertexBuffer(svbe.Index, svbe.Buffer);
                        _setVertexBufferEntryPool.Return(svbe);
                        break;
                    case SetViewportEntry sve:
                        executor.SetViewport(sve.Index, ref sve.Viewport);
                        _setViewportEntryPool.Return(sve);
                        break;
                    case UpdateBufferEntry ube:
                        fixed (byte* dataPtr = &ube.StagingBlock.Array[0])
                        {
                            executor.UpdateBuffer(
                                ube.Buffer,
                                ube.BufferOffsetInBytes,
                                (IntPtr)dataPtr,
                                ube.StagingBlock.SizeInBytes);
                            ube.StagingBlock.Free();
                            _updateBufferEntryPool.Return(ube);
                        }
                        break;
                    case UpdateTextureEntry ute:
                        fixed (byte* dataPtr = &ute.StagingBlock.Array[0])
                        {
                            executor.UpdateTexture(
                            ute.Texture,
                            (IntPtr)dataPtr,
                            ute.X,
                            ute.Y,
                            ute.Z,
                            ute.Width,
                            ute.Height,
                            ute.Depth,
                            ute.MipLevel,
                            ute.ArrayLayer);
                            _updateTextureEntryPool.Return(ute);
                        }
                        ute.StagingBlock.Free();
                        break;
                    case UpdateTextureCubeEntry utce:
                        fixed (byte* dataPtr = &utce.StagingBlock.Array[0])
                        {
                            executor.UpdateTextureCube(
                                utce.TextureCube,
                                (IntPtr)dataPtr,
                                utce.Face,
                                utce.X,
                                utce.Y,
                                utce.Width,
                                utce.Height,
                                utce.MipLevel,
                                utce.ArrayLayer);
                            utce.StagingBlock.Free();
                            _updateTextureCubeEntryPool.Return(utce);
                        }
                        break;
                    case ResolveTextureEntry rte:
                        executor.ResolveTexture(rte.Source, rte.Destination);
                        _resolveTextureEntryPool.Return(rte);
                        break;
                    default:
                        throw new InvalidOperationException("Command type not handled: " + executor.GetType().Name);
                }
            }
        }

        private class EntryPool<T> where T : OpenGLCommandEntry, new()
        {
            private readonly Queue<T> _entries = new Queue<T>();

            public T Rent()
            {
                if (_entries.Count > 0)
                {
                    return _entries.Dequeue();
                }
                else
                {
                    return new T();
                }
            }

            public void Return(T entry)
            {
                entry.ClearReferences();
                _entries.Enqueue(entry);
            }
        }
    }
}
#endif
