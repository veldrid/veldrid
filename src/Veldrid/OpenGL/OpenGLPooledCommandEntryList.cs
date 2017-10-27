using System;
using System.Collections.Generic;

namespace Veldrid.OpenGL
{
    internal class OpenGLPooledCommandEntryList : OpenGLCommandEntryList
    {
        private readonly List<OpenGLCommandEntry> _commands = new List<OpenGLCommandEntry>();
        private readonly StagingMemoryPool _memoryPool = new StagingMemoryPool();

        private readonly EntryPool<BeginEntry> _beginEntryPool = new EntryPool<BeginEntry>();
        private readonly EntryPool<ClearColorTargetEntry> _clearColorTargetEntryPool = new EntryPool<ClearColorTargetEntry>();
        private readonly EntryPool<ClearDepthTargetEntry> _clearDepthTargetEntryPool = new EntryPool<ClearDepthTargetEntry>();
        private readonly EntryPool<DrawEntry> _drawEntryPool = new EntryPool<DrawEntry>();
        private readonly EntryPool<EndEntry> _endEntryPool = new EntryPool<EndEntry>();
        private readonly EntryPool<SetFramebufferEntry> _setFramebufferEntryPool = new EntryPool<SetFramebufferEntry>();
        private readonly EntryPool<SetIndexBufferEntry> _setIndexBufferEntryPool = new EntryPool<SetIndexBufferEntry>();
        private readonly EntryPool<SetPipelineEntry> _setPipelineEntryPool = new EntryPool<SetPipelineEntry>();
        private readonly EntryPool<SetResourceSetEntry> _setResourceSetEntryPool = new EntryPool<SetResourceSetEntry>();
        private readonly EntryPool<SetScissorRectEntry> _setScissorRectEntryPool = new EntryPool<SetScissorRectEntry>();
        private readonly EntryPool<SetVertexBufferEntry> _setVertexBufferEntryPool = new EntryPool<SetVertexBufferEntry>();
        private readonly EntryPool<SetViewportEntry> _setViewportEntryPool = new EntryPool<SetViewportEntry>();
        private readonly EntryPool<UpdateBufferEntry> _updateBufferEntryPool = new EntryPool<UpdateBufferEntry>();
        private readonly EntryPool<UpdateTexture2DEntry> _updateTexture2DEntryPool = new EntryPool<UpdateTexture2DEntry>();
        private readonly EntryPool<UpdateTextureCubeEntry> _updateTextureCubeEntryPool = new EntryPool<UpdateTextureCubeEntry>();

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

        public void Draw(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            _commands.Add(_drawEntryPool.Rent().Init(indexCount, instanceCount, indexStart, vertexOffset, instanceStart));
        }

        public void End()
        {
            _commands.Add(_endEntryPool.Rent());
        }

        public void SetFramebuffer(Framebuffer fb)
        {
            _commands.Add(_setFramebufferEntryPool.Rent().Init(fb));
        }

        public void SetIndexBuffer(IndexBuffer ib)
        {
            _commands.Add(_setIndexBufferEntryPool.Rent().Init(ib));
        }

        public void SetPipeline(Pipeline pipeline)
        {
            _commands.Add(_setPipelineEntryPool.Rent().Init(pipeline));
        }

        public void SetResourceSet(uint slot, ResourceSet rs)
        {
            _commands.Add(_setResourceSetEntryPool.Rent().Init(slot, rs));
        }

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            _commands.Add(_setScissorRectEntryPool.Rent().Init(index, x, y, width, height));
        }

        public void SetVertexBuffer(uint index, VertexBuffer vb)
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
            _commands.Add(_updateTexture2DEntryPool.Rent().Init(texture2D, stagingBlock, x, y, width, height, mipLevel, arrayLayer));
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
            _commands.Add(_updateTextureCubeEntryPool.Rent().Init(textureCube, stagingBlock, face, x, y, width, height, mipLevel, arrayLayer));
        }

        public void ExecuteAll(OpenGLCommandExecutor executor)
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
                        executor.Draw(de.IndexCount, de.InstanceCount, de.IndexStart, de.VertexOffset, de.InstanceCount);
                        _drawEntryPool.Return(de);
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
                        executor.SetIndexBuffer(sibe.IndexBuffer);
                        _setIndexBufferEntryPool.Return(sibe);
                        break;
                    case SetPipelineEntry spe:
                        executor.SetPipeline(spe.Pipeline);
                        _setPipelineEntryPool.Return(spe);
                        break;
                    case SetResourceSetEntry srse:
                        executor.SetResourceSet(srse.Slot, srse.ResourceSet);
                        _setResourceSetEntryPool.Return(srse);
                        break;
                    case SetScissorRectEntry ssre:
                        executor.SetScissorRect(ssre.Index, ssre.X, ssre.Y, ssre.Width, ssre.Height);
                        _setScissorRectEntryPool.Return(ssre);
                        break;
                    case SetVertexBufferEntry svbe:
                        executor.SetVertexBuffer(svbe.Index, svbe.VertexBuffer);
                        _setVertexBufferEntryPool.Return(svbe);
                        break;
                    case SetViewportEntry sve:
                        executor.SetViewport(sve.Index, ref sve.Viewport);
                        _setViewportEntryPool.Return(sve);
                        break;
                    case UpdateBufferEntry ube:
                        executor.UpdateBuffer(ube.Buffer, ube.BufferOffsetInBytes, ube.StagingBlock);
                        _updateBufferEntryPool.Return(ube);
                        break;
                    case UpdateTexture2DEntry ut2de:
                        executor.UpdateTexture2D(
                            ut2de.Texture2D,
                            ut2de.StagingBlock,
                            ut2de.X,
                            ut2de.Y,
                            ut2de.Width,
                            ut2de.Height,
                            ut2de.MipLevel,
                            ut2de.ArrayLayer);
                        _updateTexture2DEntryPool.Return(ut2de);
                        break;
                    case UpdateTextureCubeEntry utce:
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
                        _updateTextureCubeEntryPool.Return(utce);
                        break;
                    default:
                        throw new NotImplementedException("Command type not handled: " + executor.GetType().Name);
                }
            }
        }

        private class EntryPool<T> where T: OpenGLCommandEntry, new()
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