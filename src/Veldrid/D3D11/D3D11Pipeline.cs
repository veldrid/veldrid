using SharpDX.Direct3D11;
using System.Diagnostics;

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
        public D3D11ResourceLayout[] ResourceLayouts { get; }
        public int[] VertexStrides { get; }

        public D3D11Pipeline(D3D11ResourceCache cache, ref PipelineDescription description)
        {
            BlendState = cache.GetBlendState(ref description.BlendState);
            DepthStencilState = cache.GetDepthStencilState(ref description.DepthStencilState);
            RasterizerState = cache.GetRasterizerState(ref description.RasterizerState);
            PrimitiveTopology = D3D11Formats.VdToD3D11PrimitiveTopology(description.PrimitiveTopology);

            byte[] vsBytecode = null;
            ShaderStageDescription[] stages = description.ShaderSet.ShaderStages;
            for (int i = 0; i < description.ShaderSet.ShaderStages.Length; i++)
            {
                if (stages[i].Stage == ShaderStages.Vertex)
                {
                    D3D11Shader d3d11VertexShader = ((D3D11Shader)stages[i].Shader);
                    VertexShader = (VertexShader)d3d11VertexShader.DeviceShader;
                    vsBytecode = d3d11VertexShader.Bytecode;
                }
                if (stages[i].Stage == ShaderStages.Geometry)
                {
                    GeometryShader = (GeometryShader)((D3D11Shader)stages[i].Shader).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.TessellationControl)
                {
                    HullShader = (HullShader)((D3D11Shader)stages[i].Shader).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.TessellationEvaluation)
                {
                    DomainShader = (DomainShader)((D3D11Shader)stages[i].Shader).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.Fragment)
                {
                    PixelShader = (PixelShader)((D3D11Shader)stages[i].Shader).DeviceShader;
                }
            }

            ResourceLayout[] genericLayouts = description.ResourceLayouts;
            ResourceLayouts = new D3D11ResourceLayout[genericLayouts.Length];
            for (int i = 0; i < ResourceLayouts.Length; i++)
            {
                ResourceLayouts[i] = Util.AssertSubtype<ResourceLayout, D3D11ResourceLayout>(genericLayouts[i]);
            }

            Debug.Assert(vsBytecode != null);
            InputLayout = cache.GetInputLayout(description.ShaderSet.VertexLayouts, vsBytecode);
            int numVertexBuffers = description.ShaderSet.VertexLayouts.Length;
            VertexStrides = new int[numVertexBuffers];
            for (int i = 0; i < numVertexBuffers; i++)
            {
                VertexStrides[i] = (int)description.ShaderSet.VertexLayouts[i].Stride;
            }
        }

        public override void Dispose()
        {
        }
    }
}