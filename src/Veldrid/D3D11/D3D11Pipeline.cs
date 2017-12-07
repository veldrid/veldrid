using SharpDX.Direct3D11;
using System.Diagnostics;
using System;

namespace Veldrid.D3D11
{
    internal class D3D11Pipeline : Pipeline
    {
        public BlendState BlendState { get; }
        public DepthStencilState DepthStencilState { get; }
        public RasterizerState RasterizerState { get; }
        public SharpDX.Direct3D.PrimitiveTopology PrimitiveTopology { get; }
        public InputLayout InputLayout { get; }
        public VertexShader VertexShader { get; }
        public GeometryShader GeometryShader { get; } // May be null.
        public HullShader HullShader { get; } // May be null.
        public DomainShader DomainShader { get; } // May be null.
        public PixelShader PixelShader { get; }
        public ComputeShader ComputeShader { get; }
        public D3D11ResourceLayout[] ResourceLayouts { get; }
        public int[] VertexStrides { get; }

        public override bool IsComputePipeline { get; }

        public D3D11Pipeline(D3D11ResourceCache cache, ref GraphicsPipelineDescription description)
        {
            BlendState = cache.GetBlendState(ref description.BlendState);
            DepthStencilState = cache.GetDepthStencilState(ref description.DepthStencilState);
            RasterizerState = cache.GetRasterizerState(
                ref description.RasterizerState,
                description.Outputs.SampleCount != TextureSampleCount.Count1);
            PrimitiveTopology = D3D11Formats.VdToD3D11PrimitiveTopology(description.PrimitiveTopology);

            byte[] vsBytecode = null;
            Shader[] stages = description.ShaderSet.Shaders;
            for (int i = 0; i < description.ShaderSet.Shaders.Length; i++)
            {
                if (stages[i].Stage == ShaderStages.Vertex)
                {
                    D3D11Shader d3d11VertexShader = ((D3D11Shader)stages[i]);
                    VertexShader = (VertexShader)d3d11VertexShader.DeviceShader;
                    vsBytecode = d3d11VertexShader.Bytecode;
                }
                if (stages[i].Stage == ShaderStages.Geometry)
                {
                    GeometryShader = (GeometryShader)((D3D11Shader)stages[i]).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.TessellationControl)
                {
                    HullShader = (HullShader)((D3D11Shader)stages[i]).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.TessellationEvaluation)
                {
                    DomainShader = (DomainShader)((D3D11Shader)stages[i]).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.Fragment)
                {
                    PixelShader = (PixelShader)((D3D11Shader)stages[i]).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.Compute)
                {
                    ComputeShader = (ComputeShader)((D3D11Shader)stages[i]).DeviceShader;
                }
            }

            ResourceLayout[] genericLayouts = description.ResourceLayouts;
            ResourceLayouts = new D3D11ResourceLayout[genericLayouts.Length];
            for (int i = 0; i < ResourceLayouts.Length; i++)
            {
                ResourceLayouts[i] = Util.AssertSubtype<ResourceLayout, D3D11ResourceLayout>(genericLayouts[i]);
            }

            Debug.Assert(vsBytecode != null || ComputeShader != null);
            if (vsBytecode != null && description.ShaderSet.VertexLayouts.Length > 0)
            {
                InputLayout = cache.GetInputLayout(description.ShaderSet.VertexLayouts, vsBytecode);
                int numVertexBuffers = description.ShaderSet.VertexLayouts.Length;
                VertexStrides = new int[numVertexBuffers];
                for (int i = 0; i < numVertexBuffers; i++)
                {
                    VertexStrides[i] = (int)description.ShaderSet.VertexLayouts[i].Stride;
                }
            }
            else
            {
                VertexStrides = Array.Empty<int>();
            }
        }

        public D3D11Pipeline(D3D11ResourceCache cache, ref ComputePipelineDescription description)
        {
            IsComputePipeline = true;
            ComputeShader = (ComputeShader)((D3D11Shader)description.ComputeShader).DeviceShader;
            ResourceLayout[] genericLayouts = description.ResourceLayouts;
            ResourceLayouts = new D3D11ResourceLayout[genericLayouts.Length];
            for (int i = 0; i < ResourceLayouts.Length; i++)
            {
                ResourceLayouts[i] = Util.AssertSubtype<ResourceLayout, D3D11ResourceLayout>(genericLayouts[i]);
            }
        }

        public override void Dispose()
        {
        }
    }
}