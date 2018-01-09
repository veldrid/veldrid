using System;
using System.Diagnostics;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLPipeline : Pipeline
    {
        private bool _disposed;

        public MTLRenderPipelineState RenderPipelineState { get; }
        public MTLComputePipelineState ComputePipelineState { get; }
        public MTLPrimitiveType PrimitiveType { get; }
        public MTLResourceLayout[] ResourceLayouts { get; }
        public uint VertexBufferCount { get; }
        public MTLCullMode CullMode { get; }
        public MTLWinding FrontFace { get; }
        public MTLDepthStencilState DepthStencilState { get; }
        public MTLDepthClipMode DepthClipMode { get; }
        public override bool IsComputePipeline { get; }
        public bool ScissorTestEnabled { get; }
        public MTLSize ThreadsPerThreadgroup { get; } = new MTLSize(1, 1, 1);
        public bool HasStencil { get; }
        public override string Name { get; set; }
        public uint StencilReference { get; }
        public RgbaFloat BlendColor { get; }

        public MTLPipeline(ref GraphicsPipelineDescription description, MTLGraphicsDevice gd)
            : base(ref description)
        {
            PrimitiveType = MTLFormats.VdToMTLPrimitiveTopology(description.PrimitiveTopology);
            ResourceLayouts = new MTLResourceLayout[description.ResourceLayouts.Length];
            for (int i = 0; i < ResourceLayouts.Length; i++)
            {
                ResourceLayouts[i] = Util.AssertSubtype<ResourceLayout, MTLResourceLayout>(description.ResourceLayouts[i]);
            }

            CullMode = MTLFormats.VdToMTLCullMode(description.RasterizerState.CullMode);
            FrontFace = MTLFormats.VdVoMTLFrontFace(description.RasterizerState.FrontFace);
            ScissorTestEnabled = description.RasterizerState.ScissorTestEnabled;

            MTLRenderPipelineDescriptor mtlDesc = MTLRenderPipelineDescriptor.New();
            foreach (var shader in description.ShaderSet.Shaders)
            {
                var mtlShader = Util.AssertSubtype<Shader, MTLShader>(shader);
                if (shader.Stage == ShaderStages.Vertex)
                {
                    mtlDesc.vertexFunction = mtlShader.Function;
                }
                else if (shader.Stage == ShaderStages.Fragment)
                {
                    mtlDesc.fragmentFunction = mtlShader.Function;
                }
            }

            // Vertex layouts
            VertexLayoutDescription[] vdVertexLayouts = description.ShaderSet.VertexLayouts;
            MTLVertexDescriptor vertexDescriptor = mtlDesc.vertexDescriptor;

            for (uint i = 0; i < vdVertexLayouts.Length; i++)
            {
                MTLVertexBufferLayoutDescriptor mtlLayout = vertexDescriptor.layouts[i];
                mtlLayout.stride = (UIntPtr)vdVertexLayouts[i].Stride;
                uint stepRate = vdVertexLayouts[i].InstanceStepRate;
                mtlLayout.stepFunction = stepRate == 0 ? MTLVertexStepFunction.PerVertex : MTLVertexStepFunction.PerInstance;
                mtlLayout.stepRate = (UIntPtr)Math.Max(1, stepRate);
            }

            uint element = 0;
            for (uint i = 0; i < vdVertexLayouts.Length; i++)
            {
                uint offset = 0;
                VertexLayoutDescription vdDesc = vdVertexLayouts[i];
                for (uint j = 0; j < vdDesc.Elements.Length; j++)
                {
                    VertexElementDescription elementDesc = vdDesc.Elements[j];
                    MTLVertexAttributeDescriptor mtlAttribute = vertexDescriptor.attributes[element];
                    mtlAttribute.bufferIndex = (UIntPtr)i;
                    mtlAttribute.format = MTLFormats.VdToMTLVertexFormat(elementDesc.Format);
                    mtlAttribute.offset = (UIntPtr)offset;
                    offset += FormatHelpers.GetSizeInBytes(elementDesc.Format);
                    element += 1;
                }
            }

            VertexBufferCount = (uint)vdVertexLayouts.Length;

            // Outputs
            OutputDescription outputs = description.Outputs;
            BlendStateDescription blendStateDesc = description.BlendState;
            BlendColor = blendStateDesc.BlendFactor;

            if (outputs.SampleCount != TextureSampleCount.Count1)
            {
                mtlDesc.sampleCount = (UIntPtr)FormatHelpers.GetSampleCountUInt32(outputs.SampleCount);
            }

            if (outputs.DepthAttachment != null)
            {
                PixelFormat depthFormat = outputs.DepthAttachment.Value.Format;
                MTLPixelFormat mtlDepthFormat = MTLFormats.VdToMTLPixelFormat(depthFormat, true);
                mtlDesc.depthAttachmentPixelFormat = mtlDepthFormat;
                if ((FormatHelpers.IsStencilFormat(depthFormat)))
                {
                    HasStencil = true;
                    mtlDesc.stencilAttachmentPixelFormat = mtlDepthFormat;
                }
            }
            for (uint i = 0; i < outputs.ColorAttachments.Length; i++)
            {
                BlendAttachmentDescription attachmentBlendDesc = blendStateDesc.AttachmentStates[i];
                MTLRenderPipelineColorAttachmentDescriptor colorDesc = mtlDesc.colorAttachments[i];
                colorDesc.pixelFormat = MTLFormats.VdToMTLPixelFormat(outputs.ColorAttachments[i].Format, false);
                colorDesc.blendingEnabled = attachmentBlendDesc.BlendEnabled;
                colorDesc.alphaBlendOperation = MTLFormats.VdToMTLBlendOp(attachmentBlendDesc.AlphaFunction);
                colorDesc.sourceAlphaBlendFactor = MTLFormats.VdToMTLBlendFactor(attachmentBlendDesc.SourceAlphaFactor);
                colorDesc.destinationAlphaBlendFactor = MTLFormats.VdToMTLBlendFactor(attachmentBlendDesc.DestinationAlphaFactor);

                colorDesc.rgbBlendOperation = MTLFormats.VdToMTLBlendOp(attachmentBlendDesc.ColorFunction);
                colorDesc.sourceRGBBlendFactor = MTLFormats.VdToMTLBlendFactor(attachmentBlendDesc.SourceColorFactor);
                colorDesc.destinationRGBBlendFactor = MTLFormats.VdToMTLBlendFactor(attachmentBlendDesc.DestinationColorFactor);
            }

            RenderPipelineState = gd.Device.newRenderPipelineStateWithDescriptor(mtlDesc);
            ObjectiveCRuntime.release(mtlDesc.NativePtr);

            if (outputs.DepthAttachment != null)
            {
                MTLDepthStencilDescriptor depthDescriptor = MTLUtil.AllocInit<MTLDepthStencilDescriptor>();
                depthDescriptor.depthCompareFunction = MTLFormats.VdToMTLCompareFunction(
                    description.DepthStencilState.DepthComparison);
                depthDescriptor.depthWriteEnabled = description.DepthStencilState.DepthWriteEnabled;

                bool stencilEnabled = description.DepthStencilState.StencilTestEnabled;
                if (stencilEnabled)
                {
                    StencilReference = description.DepthStencilState.StencilReference;

                    StencilBehaviorDescription vdFrontDesc = description.DepthStencilState.StencilFront;
                    MTLStencilDescriptor front = MTLUtil.AllocInit<MTLStencilDescriptor>();
                    front.readMask = stencilEnabled ? description.DepthStencilState.StencilReadMask : 0u;
                    front.writeMask = stencilEnabled ? description.DepthStencilState.StencilWriteMask : 0u;
                    front.depthFailureOperation = MTLFormats.VdToMTLStencilOperation(vdFrontDesc.DepthFail);
                    front.stencilFailureOperation = MTLFormats.VdToMTLStencilOperation(vdFrontDesc.Fail);
                    front.depthStencilPassOperation = MTLFormats.VdToMTLStencilOperation(vdFrontDesc.Pass);
                    front.stencilCompareFunction = MTLFormats.VdToMTLCompareFunction(vdFrontDesc.Comparison);
                    depthDescriptor.frontFaceStencil = front;

                    StencilBehaviorDescription vdBackDesc = description.DepthStencilState.StencilBack;
                    MTLStencilDescriptor back = MTLUtil.AllocInit<MTLStencilDescriptor>();
                    back.readMask = stencilEnabled ? description.DepthStencilState.StencilReadMask : 0u;
                    back.writeMask = stencilEnabled ? description.DepthStencilState.StencilWriteMask : 0u;
                    back.depthFailureOperation = MTLFormats.VdToMTLStencilOperation(vdBackDesc.DepthFail);
                    back.stencilFailureOperation = MTLFormats.VdToMTLStencilOperation(vdBackDesc.Fail);
                    back.depthStencilPassOperation = MTLFormats.VdToMTLStencilOperation(vdBackDesc.Pass);
                    back.stencilCompareFunction = MTLFormats.VdToMTLCompareFunction(vdBackDesc.Comparison);
                    depthDescriptor.backFaceStencil = back;

                    ObjectiveCRuntime.release(front.NativePtr);
                    ObjectiveCRuntime.release(back.NativePtr);
                }

                DepthStencilState = gd.Device.newDepthStencilStateWithDescriptor(depthDescriptor);
                ObjectiveCRuntime.release(depthDescriptor.NativePtr);
            }

            DepthClipMode = description.DepthStencilState.DepthTestEnabled ? MTLDepthClipMode.Clip : MTLDepthClipMode.Clamp;
        }

        public MTLPipeline(ref ComputePipelineDescription description, MTLGraphicsDevice gd)
            : base(ref description)
        {
            IsComputePipeline = true;
            ResourceLayouts = new MTLResourceLayout[description.ResourceLayouts.Length];
            for (int i = 0; i < ResourceLayouts.Length; i++)
            {
                ResourceLayouts[i] = Util.AssertSubtype<ResourceLayout, MTLResourceLayout>(description.ResourceLayouts[i]);
            }

            ThreadsPerThreadgroup = new MTLSize(
                description.ThreadGroupSizeX,
                description.ThreadGroupSizeY,
                description.ThreadGroupSizeZ);

            MTLComputePipelineDescriptor mtlDesc = MTLUtil.AllocInit<MTLComputePipelineDescriptor>();
            MTLShader mtlShader = Util.AssertSubtype<Shader, MTLShader>(description.ComputeShader);
            mtlDesc.computeFunction = mtlShader.Function;
            MTLPipelineBufferDescriptorArray buffers = mtlDesc.buffers;
            uint bufferIndex = 0;
            foreach (MTLResourceLayout layout in ResourceLayouts)
            {
                foreach (ResourceKind kind in layout.ResourceKinds)
                {
                    if (kind == ResourceKind.UniformBuffer
                        || kind == ResourceKind.StructuredBufferReadOnly)
                    {
                        MTLPipelineBufferDescriptor bufferDesc = buffers[bufferIndex];
                        bufferDesc.mutability = MTLMutability.Immutable;
                        bufferIndex += 1;
                    }
                    else if (kind == ResourceKind.StructuredBufferReadWrite)
                    {
                        MTLPipelineBufferDescriptor bufferDesc = buffers[bufferIndex];
                        bufferDesc.mutability = MTLMutability.Mutable;
                        bufferIndex += 1;
                    }
                }
            }

            ComputePipelineState = gd.Device.newComputePipelineStateWithDescriptor(mtlDesc);

            ObjectiveCRuntime.release(mtlDesc.NativePtr);
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                if (RenderPipelineState.NativePtr != IntPtr.Zero)
                {
                    ObjectiveCRuntime.release(RenderPipelineState.NativePtr);
                }
                else
                {
                    Debug.Assert(ComputePipelineState.NativePtr != IntPtr.Zero);
                    ObjectiveCRuntime.release(ComputePipelineState.NativePtr);
                }
                _disposed = true;
            }
        }
    }
}