#if OPENGL_MANAGED_COMMAND_ENTRY_LIST
using System;
using System.Collections.Generic;

namespace Veldrid.OpenGL.ManagedEntryList
{
    internal class OpenGLManagedCommandEntryList : OpenGLCommandEntryList
    {
        private readonly List<OpenGLCommandEntry> _commands = new List<OpenGLCommandEntry>();
        private readonly StagingMemoryPool _memoryPool = new StagingMemoryPool();

        public IReadOnlyList<OpenGLCommandEntry> Commands => _commands;

        public void Reset()
        {
            _commands.Clear();
        }

        public void Begin()
        {
            _commands.Add(new BeginEntry());
        }

        public void ClearColorTarget(uint index, RgbaFloat clearColor)
        {
            _commands.Add(new ClearColorTargetEntry(index, clearColor));
        }

        public void ClearDepthTarget(float depth)
        {
            _commands.Add(new ClearDepthTargetEntry(depth));
        }

        public void Draw(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            _commands.Add(new DrawEntry(vertexCount, instanceCount, vertexStart, instanceStart));
        }

        public void DrawIndexed(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            _commands.Add(new DrawIndexedEntry(indexCount, instanceCount, indexStart, vertexOffset, instanceStart));
        }

        public void DrawIndirect(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            _commands.Add(new DrawIndirectEntry(indirectBuffer, offset, drawCount, stride));
        }

        public void DrawIndexedIndirect(Buffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            _commands.Add(new DrawIndexedIndirectEntry(indirectBuffer, offset, drawCount, stride));
        }

        public void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            _commands.Add(new DispatchEntry(groupCountX, groupCountY, groupCountZ));
        }

        public void DispatchIndirect(Buffer indirectBuffer, uint offset)
        {
            _commands.Add(new DispatchIndirectEntry(indirectBuffer, offset));
        }

        public void End()
        {
            _commands.Add(new EndEntry());
        }

        public void SetFramebuffer(Framebuffer fb)
        {
            _commands.Add(new SetFramebufferEntry(fb));
        }

        public void SetIndexBuffer(Buffer buffer, IndexFormat format)
        {
            _commands.Add(new SetIndexBufferEntry(buffer, format));
        }

        public void SetPipeline(Pipeline pipeline)
        {
            _commands.Add(new SetPipelineEntry(pipeline));
        }

        public void SetGraphicsResourceSet(uint slot, ResourceSet rs)
        {
            _commands.Add(new SetGraphicsResourceSetEntry(slot, rs));
        }

        public void SetComputeResourceSet(uint slot, ResourceSet rs)
        {
            _commands.Add(new SetComputeResourceSetEntry(slot, rs));
        }

        public void SetScissorRect(uint index, uint x, uint y, uint width, uint height)
        {
            _commands.Add(new SetScissorRectEntry(index, x, y, width, height));
        }

        public void SetVertexBuffer(uint index, Buffer vb)
        {
            _commands.Add(new SetVertexBufferEntry(index, vb));
        }

        public void SetViewport(uint index, ref Viewport viewport)
        {
            _commands.Add(new SetViewportEntry(index, ref viewport));
        }

        public void UpdateBuffer(Buffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            StagingBlock stagingBlock = _memoryPool.Stage(source, sizeInBytes);
            _commands.Add(new UpdateBufferEntry(buffer, bufferOffsetInBytes, stagingBlock));
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
            _commands.Add(new UpdateTextureEntry(texture, stagingBlock, x, y, z, width, height, depth, mipLevel, arrayLayer));
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
            _commands.Add(
                new UpdateTextureCubeEntry(textureCube, stagingBlock, face, x, y, width, height, mipLevel, arrayLayer));
        }

        public void ResolveTexture(Texture source, Texture destination)
        {
            _commands.Add(new ResolveTextureEntry(source, destination));

        }

        public unsafe void ExecuteAll(OpenGLCommandExecutor executor)
        {
            foreach (OpenGLCommandEntry entry in _commands)
            {
                switch (entry)
                {
                    case BeginEntry be:
                        executor.Begin();
                        break;
                    case ClearColorTargetEntry ccte:
                        executor.ClearColorTarget(ccte.Index, ccte.ClearColor);
                        break;
                    case ClearDepthTargetEntry cdte:
                        executor.ClearDepthTarget(cdte.Depth);
                        break;
                    case DrawEntry de:
                        executor.Draw(de.VertexCount, de.InstanceCount, de.VertexCount, de.InstanceCount);
                        break;
                    case DrawIndexedEntry dIdx:
                        executor.DrawIndexed(dIdx.IndexCount, dIdx.InstanceCount, dIdx.IndexStart, dIdx.VertexOffset, dIdx.InstanceCount);
                        break;
                    case DrawIndirectEntry dInd:
                        executor.DrawIndirect(dInd.IndirectBuffer, dInd.Offset, dInd.DrawCount, dInd.Stride);
                        break;
                    case DrawIndexedIndirectEntry dIdxInd:
                        executor.DrawIndexedIndirect(dIdxInd.IndirectBuffer, dIdxInd.Offset, dIdxInd.DrawCount, dIdxInd.Stride);
                        break;
                    case DispatchEntry dispatch:
                        executor.Dispatch(dispatch.GroupCountX, dispatch.GroupCountY, dispatch.GroupCountZ);
                        break;
                    case DispatchIndirectEntry dispInd:
                        executor.DispatchIndirect(dispInd.IndirectBuffer, dispInd.Offset);
                        break;
                    case EndEntry ee:
                        executor.End();
                        break;
                    case SetFramebufferEntry sfbe:
                        executor.SetFramebuffer(sfbe.Framebuffer);
                        break;
                    case SetIndexBufferEntry sibe:
                        executor.SetIndexBuffer(sibe.Buffer, sibe.Format);
                        break;
                    case SetPipelineEntry spe:
                        executor.SetPipeline(spe.Pipeline);
                        break;
                    case SetGraphicsResourceSetEntry srse:
                        executor.SetGraphicsResourceSet(srse.Slot, srse.ResourceSet);
                        break;
                    case SetScissorRectEntry ssre:
                        executor.SetScissorRect(ssre.Index, ssre.X, ssre.Y, ssre.Width, ssre.Height);
                        break;
                    case SetVertexBufferEntry svbe:
                        executor.SetVertexBuffer(svbe.Index, svbe.Buffer);
                        break;
                    case SetViewportEntry sve:
                        executor.SetViewport(sve.Index, ref sve.Viewport);
                        break;
                    case UpdateBufferEntry ube:
                        fixed (byte* dataPtr = &ube.StagingBlock.Array[0])
                        {
                            executor.UpdateBuffer(ube.Buffer, ube.BufferOffsetInBytes, (IntPtr)dataPtr, ube.StagingBlock.SizeInBytes);
                        }
                        ube.StagingBlock.Free();
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
                        }
                        utce.StagingBlock.Free();
                        break;
                    case ResolveTextureEntry rte:
                        executor.ResolveTexture(rte.Source, rte.Destination);
                        break;
                    default:
                        throw new InvalidOperationException("Command type not handled: " + executor.GetType().Name);
                }
            }
        }
    }
}
#endif
