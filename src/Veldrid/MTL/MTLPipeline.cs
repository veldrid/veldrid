using System;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLPipeline : Pipeline
    {
        public MetalBindings.MTLRenderPipelineState RenderPipelineState { get; }
        public MTLPrimitiveType PrimitiveType { get; }
        public MTLResourceLayout[] ResourceLayouts { get; }
        public uint VertexBufferCount { get; }
        public MTLCullMode CullMode { get; }
        public MTLWinding FrontFace { get; }
        public MTLDepthStencilState DepthStencilState { get; }
        public MTLDepthClipMode DepthClipMode { get; }

        public MTLPipeline(ref GraphicsPipelineDescription description, MTLGraphicsDevice gd)
            : base(ref description)
        {
            IsComputePipeline = description.ShaderSet.Shaders.Length == 1
                && description.ShaderSet.Shaders[0].Stage == ShaderStages.Compute;
            if (IsComputePipeline)
            {
                throw new NotImplementedException();
            }

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
                DepthStencilState = gd.Device.newDepthStencilStateWithDescriptor(depthDescriptor);
                ObjectiveCRuntime.release(depthDescriptor.NativePtr);
            }
            
            DepthClipMode = description.DepthStencilState.DepthTestEnabled ? MTLDepthClipMode.Clip : MTLDepthClipMode.Clamp;
        }

        public override bool IsComputePipeline { get; }

        public bool ScissorTestEnabled { get; }

        public override string Name { get; set; }

        public override void Dispose()
        {
        }
    }
}