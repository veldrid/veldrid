using System;

namespace Veldrid
{
    /// <summary>
    /// Describes a graphics <see cref="Pipeline"/>, for creation using a <see cref="ResourceFactory"/>.
    /// </summary>
    public struct GraphicsPipelineDescription : IEquatable<GraphicsPipelineDescription>
    {
        /// <summary>
        /// A description of the blend state, which controls how color values are blended into each color target.
        /// </summary>
        public BlendStateDescription BlendState;
        /// <summary>
        /// A description of the depth stencil state, which controls depth tests, writing, and comparisons.
        /// </summary>
        public DepthStencilStateDescription DepthStencilState;
        /// <summary>
        /// A description of the rasterizer state, which controls culling, clipping, scissor, and polygon-fill behavior.
        /// </summary>
        public RasterizerStateDescription RasterizerState;
        /// <summary>
        /// The <see cref="PrimitiveTopology"/> to use, which controls how a series of input vertices is interpreted by the
        /// <see cref="Pipeline"/>.
        /// </summary>
        public PrimitiveTopology PrimitiveTopology;
        /// <summary>
        /// A description of the shader set to be used.
        /// </summary>
        public ShaderSetDescription ShaderSet;
        /// <summary>
        /// An array of <see cref="ResourceLayout"/>, which controls the layout of shader resources in the <see cref="Pipeline"/>.
        /// </summary>
        public ResourceLayout[] ResourceLayouts;
        /// <summary>
        /// A description of the output attachments used by the <see cref="Pipeline"/>.
        /// </summary>
        public OutputDescription Outputs;
        /// <summary>
        /// Specifies which model the rendering backend should use for binding resources.
        /// If <code>null</code>, the pipeline will use the value specified in <see cref="GraphicsDeviceOptions"/>.
        /// </summary>
        public ResourceBindingModel? ResourceBindingModel;

        /// <summary>
        /// Constructs a new <see cref="GraphicsPipelineDescription"/>.
        /// </summary>
        /// <param name="blendState">A description of the blend state, which controls how color values are blended into each
        /// color target.</param>
        /// <param name="depthStencilStateDescription">A description of the depth stencil state, which controls depth tests,
        /// writing, and comparisons.</param>
        /// <param name="rasterizerState">A description of the rasterizer state, which controls culling, clipping, scissor, and
        /// polygon-fill behavior.</param>
        /// <param name="primitiveTopology">The <see cref="PrimitiveTopology"/> to use, which controls how a series of input
        /// vertices is interpreted by the <see cref="Pipeline"/>.</param>
        /// <param name="shaderSet">A description of the shader set to be used.</param>
        /// <param name="resourceLayouts">An array of <see cref="ResourceLayout"/>, which controls the layout of shader resoruces
        /// in the <see cref="Pipeline"/>.</param>
        /// <param name="outputs">A description of the output attachments used by the <see cref="Pipeline"/>.</param>
        public GraphicsPipelineDescription(
            BlendStateDescription blendState,
            DepthStencilStateDescription depthStencilStateDescription,
            RasterizerStateDescription rasterizerState,
            PrimitiveTopology primitiveTopology,
            ShaderSetDescription shaderSet,
            ResourceLayout[] resourceLayouts,
            OutputDescription outputs)
        {
            BlendState = blendState;
            DepthStencilState = depthStencilStateDescription;
            RasterizerState = rasterizerState;
            PrimitiveTopology = primitiveTopology;
            ShaderSet = shaderSet;
            ResourceLayouts = resourceLayouts;
            Outputs = outputs;
            ResourceBindingModel = null;
        }

        /// <summary>
        /// Constructs a new <see cref="GraphicsPipelineDescription"/>.
        /// </summary>
        /// <param name="blendState">A description of the blend state, which controls how color values are blended into each
        /// color target.</param>
        /// <param name="depthStencilStateDescription">A description of the depth stencil state, which controls depth tests,
        /// writing, and comparisons.</param>
        /// <param name="rasterizerState">A description of the rasterizer state, which controls culling, clipping, scissor, and
        /// polygon-fill behavior.</param>
        /// <param name="primitiveTopology">The <see cref="PrimitiveTopology"/> to use, which controls how a series of input
        /// vertices is interpreted by the <see cref="Pipeline"/>.</param>
        /// <param name="shaderSet">A description of the shader set to be used.</param>
        /// <param name="resourceLayout">A <see cref="ResourceLayout"/>, which controls the layout of shader resoruces in the
        /// <see cref="Pipeline"/>.</param>
        /// <param name="outputs">A description of the output attachments used by the <see cref="Pipeline"/>.</param>
        public GraphicsPipelineDescription(
            BlendStateDescription blendState,
            DepthStencilStateDescription depthStencilStateDescription,
            RasterizerStateDescription rasterizerState,
            PrimitiveTopology primitiveTopology,
            ShaderSetDescription shaderSet,
            ResourceLayout resourceLayout,
            OutputDescription outputs)
        {
            BlendState = blendState;
            DepthStencilState = depthStencilStateDescription;
            RasterizerState = rasterizerState;
            PrimitiveTopology = primitiveTopology;
            ShaderSet = shaderSet;
            ResourceLayouts = new[] { resourceLayout };
            Outputs = outputs;
            ResourceBindingModel = null;
        }

        /// <summary>
        /// Constructs a new <see cref="GraphicsPipelineDescription"/>.
        /// </summary>
        /// <param name="blendState">A description of the blend state, which controls how color values are blended into each
        /// color target.</param>
        /// <param name="depthStencilStateDescription">A description of the depth stencil state, which controls depth tests,
        /// writing, and comparisons.</param>
        /// <param name="rasterizerState">A description of the rasterizer state, which controls culling, clipping, scissor, and
        /// polygon-fill behavior.</param>
        /// <param name="primitiveTopology">The <see cref="PrimitiveTopology"/> to use, which controls how a series of input
        /// vertices is interpreted by the <see cref="Pipeline"/>.</param>
        /// <param name="shaderSet">A description of the shader set to be used.</param>
        /// <param name="resourceLayouts">An array of <see cref="ResourceLayout"/>, which controls the layout of shader resoruces
        /// in the <see cref="Pipeline"/>.</param>
        /// <param name="outputs">A description of the output attachments used by the <see cref="Pipeline"/>.</param>
        /// <param name="resourceBindingModel">The <see cref="ResourceBindingModel"/> to use for this pipeline. Overrides
        /// the value specified in <see cref="GraphicsDeviceOptions"/>.</param>
        public GraphicsPipelineDescription(
            BlendStateDescription blendState,
            DepthStencilStateDescription depthStencilStateDescription,
            RasterizerStateDescription rasterizerState,
            PrimitiveTopology primitiveTopology,
            ShaderSetDescription shaderSet,
            ResourceLayout[] resourceLayouts,
            OutputDescription outputs,
            ResourceBindingModel resourceBindingModel)
        {
            BlendState = blendState;
            DepthStencilState = depthStencilStateDescription;
            RasterizerState = rasterizerState;
            PrimitiveTopology = primitiveTopology;
            ShaderSet = shaderSet;
            ResourceLayouts = resourceLayouts;
            Outputs = outputs;
            ResourceBindingModel = resourceBindingModel;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements and all array elements are equal; false otherswise.</returns>
        public bool Equals(GraphicsPipelineDescription other)
        {
            return BlendState.Equals(other.BlendState)
                && DepthStencilState.Equals(other.DepthStencilState)
                && RasterizerState.Equals(other.RasterizerState)
                && PrimitiveTopology == other.PrimitiveTopology
                && ShaderSet.Equals(other.ShaderSet)
                && Util.ArrayEquals(ResourceLayouts, other.ResourceLayouts)
                && (ResourceBindingModel.HasValue && other.ResourceBindingModel.HasValue
                    ? ResourceBindingModel.Value == other.ResourceBindingModel.Value
                    : ResourceBindingModel.HasValue == other.ResourceBindingModel.HasValue)
                && Outputs.Equals(other.Outputs);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(
                BlendState.GetHashCode(),
                DepthStencilState.GetHashCode(),
                RasterizerState.GetHashCode(),
                (int)PrimitiveTopology,
                ShaderSet.GetHashCode(),
                HashHelper.Array(ResourceLayouts),
                ResourceBindingModel.GetHashCode(),
                Outputs.GetHashCode());
        }
    }
}
