using System;
using Veldrid.CommandRecording;

namespace Veldrid.OpenGL
{
    internal class OpenGLCommandBuffer : CommandBuffer
    {
        private readonly OpenGLGraphicsDevice _gd;
        private readonly NoAllocCommandEntryList _entryList;
        private readonly object _stateLock = new object();
        private RecordingState _state = RecordingState.Recording;

        public OpenGLCommandBuffer(OpenGLGraphicsDevice gd, ref CommandBufferDescription description)
            : base(gd.Features)
        {
            _gd = gd;
            _entryList = new NoAllocCommandEntryList();
        }

        public override string Name { get; set; }

        public override void Dispose()
        {
            lock (_stateLock)
            {
                if (_state == RecordingState.Executing)
                {
                    // TODO: Dispose when done executing.
                }

                _entryList.Dispose();
                _state = RecordingState.Disposed;
            }
        }

        public override void InsertDebugMarker(string name)
        {
            BeginRecording();
            _entryList.InsertDebugMarker(name);
        }

        public override void PopDebugGroup()
        {
            BeginRecording();
            _entryList.PopDebugGroup();
        }

        public override void PushDebugGroup(string name)
        {
            BeginRecording();
            _entryList.PushDebugGroup(name);
        }

        internal override void BeginRenderPassCore(in RenderPassDescription rpi)
        {
            BeginRecording();
            _entryList.BeginRenderPass(rpi);
        }

        private protected override void EndRenderPassCore()
        {
            BeginRecording();
            _entryList.EndRenderPass();
        }

        internal override void DispatchIndirectCore(DeviceBuffer indirectBuffer, uint offset)
        {
            BeginRecording();
            _entryList.DispatchIndirect(indirectBuffer, offset);
        }

        private protected override void BindComputeResourceSetCore(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets)
        {
            BeginRecording();
            _entryList.SetComputeResourceSet(slot, resourceSet, dynamicOffsets);
        }

        private protected override void BindGraphicsResourceSetCore(uint slot, ResourceSet resourceSet, Span<uint> dynamicOffsets)
        {
            BeginRecording();
            _entryList.SetGraphicsResourceSet(slot, resourceSet, dynamicOffsets);
        }

        private protected override void BindIndexBufferCore(DeviceBuffer buffer, IndexFormat format, uint offset)
        {
            BeginRecording();
            _entryList.SetIndexBuffer(buffer, format, offset);
        }

        private protected override void BindPipelineCore(Pipeline pipeline)
        {
            BeginRecording();
            _entryList.SetPipeline(pipeline);
        }

        private protected override void BindVertexBufferCore(uint index, DeviceBuffer buffer, uint offset)
        {
            BeginRecording();
            _entryList.SetVertexBuffer(index, buffer, offset);
        }

        private protected override void CopyBufferCore(DeviceBuffer source, uint sourceOffset, DeviceBuffer destination, uint destinationOffset, uint sizeInBytes)
        {
            BeginRecording();
            _entryList.CopyBuffer(source, sourceOffset, destination, destinationOffset, sizeInBytes);
        }

        private protected override void CopyTextureCore(Texture source, uint srcX, uint srcY, uint srcZ, uint srcMipLevel, uint srcBaseArrayLayer, Texture destination, uint dstX, uint dstY, uint dstZ, uint dstMipLevel, uint dstBaseArrayLayer, uint width, uint height, uint depth, uint layerCount)
        {
            BeginRecording();
            _entryList.CopyTexture(source, srcX, srcY, srcZ, srcMipLevel, srcBaseArrayLayer, destination, dstX, dstY, dstZ, dstMipLevel, dstBaseArrayLayer, width, height, depth, layerCount);
        }

        private protected override void DispatchCore(uint groupCountX, uint groupCountY, uint groupCountZ)
        {
            BeginRecording();
            _entryList.Dispatch(groupCountX, groupCountY, groupCountZ);
        }

        private protected override void DrawCore(uint vertexCount, uint instanceCount, uint vertexStart, uint instanceStart)
        {
            BeginRecording();
            _entryList.Draw(vertexCount, instanceCount, vertexStart, instanceStart);
        }

        private protected override void DrawIndexedCore(uint indexCount, uint instanceCount, uint indexStart, int vertexOffset, uint instanceStart)
        {
            BeginRecording();
            _entryList.DrawIndexed(indexCount, instanceCount, indexStart, vertexOffset, instanceStart);
        }

        private protected override void DrawIndexedIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            BeginRecording();
            _entryList.DrawIndexedIndirect(indirectBuffer, offset, drawCount, stride);
        }

        private protected override void DrawIndirectCore(DeviceBuffer indirectBuffer, uint offset, uint drawCount, uint stride)
        {
            BeginRecording();
            _entryList.DrawIndirect(indirectBuffer, offset, drawCount, stride);
        }

        private protected override void GenerateMipmapsCore(Texture texture)
        {
            BeginRecording();
            _entryList.GenerateMipmaps(texture);
        }

        private protected override void MemoryBarrierCore(ShaderStages sourceStage, ShaderStages destinationStage)
        {
            throw new NotImplementedException();
        }

        private protected override void MemoryBarrierCore(Texture texture, uint baseMipLevel, uint levelCount, uint baseArrayLayer, uint layerCount, ShaderStages sourceStage, ShaderStages destinationStage)
        {
            BeginRecording();
            _entryList.MemoryBarrier(texture, baseMipLevel, levelCount, baseArrayLayer, layerCount, sourceStage, destinationStage);
        }

        private protected override void SetScissorRectCore(uint index, uint x, uint y, uint width, uint height)
        {
            BeginRecording();
            _entryList.SetScissorRect(index, x, y, width, height);
        }

        private protected override void SetViewportCore(uint index, Viewport viewport)
        {
            BeginRecording();
            _entryList.SetViewport(index, ref viewport);
        }

        private protected override void UpdateBufferCore(DeviceBuffer buffer, uint bufferOffsetInBytes, IntPtr source, uint sizeInBytes)
        {
            BeginRecording();
            _entryList.UpdateBuffer(buffer, bufferOffsetInBytes, source, sizeInBytes);
        }

        internal NoAllocCommandEntryList GetEntryList() => _entryList;

        private void BeginRecording()
        {
            lock (_stateLock)
            {
                if (_state == RecordingState.Executing)
                {
                    throw new VeldridException(
                        $"Cannot record additional commands into this CommandBuffer. It is currently executing.");
                }
                else if (_state != RecordingState.Recording)
                {
                    _entryList.Reset();
                    _state = RecordingState.Recording;
                }
            }
        }

        public void BeginExecuting()
        {
            lock (_stateLock)
            {
                _state = RecordingState.Executing;
            }
        }

        public void EndExecuting()
        {
            lock (_stateLock)
            {
                _state = RecordingState.ReadyToExecute;
            }
        }

        private enum RecordingState
        {
            Recording,
            ReadyToExecute,
            Executing,
            Disposed,
        }
    }
}
