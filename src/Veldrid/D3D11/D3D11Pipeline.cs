using Vortice.Direct3D11;
using System.Diagnostics;
using System;

namespace Veldrid.D3D11
{
    internal class D3D11Pipeline : Pipeline
    {
        private string _name;
        private bool _disposed;

        public ID3D11BlendState BlendState { get; }
        public ID3D11DepthStencilState DepthStencilState { get; }
        public uint StencilReference { get; }
        public ID3D11RasterizerState RasterizerState { get; }
        public Vortice.Direct3D.PrimitiveTopology PrimitiveTopology { get; }
        public ID3D11InputLayout InputLayout { get; }
        public ID3D11VertexShader VertexShader { get; }
        public ID3D11GeometryShader GeometryShader { get; } // May be null.
        public ID3D11HullShader HullShader { get; } // May be null.
        public ID3D11DomainShader DomainShader { get; } // May be null.
        public ID3D11PixelShader PixelShader { get; }
        public ID3D11ComputeShader ComputeShader { get; }
        public new D3D11ResourceLayout[] ResourceLayouts { get; }
        public int[] VertexStrides { get; }

        public override bool IsComputePipeline { get; }

        public D3D11Pipeline(D3D11ResourceCache cache, ref GraphicsPipelineDescription description)
            : base(ref description)
        {
            byte[] vsBytecode = null;
            Shader[] stages = description.ShaderSet.Shaders;
            for (int i = 0; i < description.ShaderSet.Shaders.Length; i++)
            {
                if (stages[i].Stage == ShaderStages.Vertex)
                {
                    D3D11Shader d3d11VertexShader = ((D3D11Shader)stages[i]);
                    VertexShader = (ID3D11VertexShader)d3d11VertexShader.DeviceShader;
                    vsBytecode = d3d11VertexShader.Bytecode;
                }
                if (stages[i].Stage == ShaderStages.Geometry)
                {
                    GeometryShader = (ID3D11GeometryShader)((D3D11Shader)stages[i]).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.TessellationControl)
                {
                    HullShader = (ID3D11HullShader)((D3D11Shader)stages[i]).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.TessellationEvaluation)
                {
                    DomainShader = (ID3D11DomainShader)((D3D11Shader)stages[i]).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.Fragment)
                {
                    PixelShader = (ID3D11PixelShader)((D3D11Shader)stages[i]).DeviceShader;
                }
                if (stages[i].Stage == ShaderStages.Compute)
                {
                    ComputeShader = (ID3D11ComputeShader)((D3D11Shader)stages[i]).DeviceShader;
                }
            }

            cache.GetPipelineResources(
                ref description.BlendState,
                ref description.DepthStencilState,
                ref description.RasterizerState,
                description.Outputs.SampleCount != TextureSampleCount.Count1,
                description.ShaderSet.VertexLayouts,
                vsBytecode,
                out ID3D11BlendState blendState,
                out ID3D11DepthStencilState depthStencilState,
                out ID3D11RasterizerState rasterizerState,
                out ID3D11InputLayout inputLayout);

            BlendState = blendState;
            DepthStencilState = depthStencilState;
            StencilReference = description.DepthStencilState.StencilReference;
            RasterizerState = rasterizerState;
            PrimitiveTopology = D3D11Formats.VdToD3D11PrimitiveTopology(description.PrimitiveTopology);

            ResourceLayout[] genericLayouts = description.ResourceLayouts;
            ResourceLayouts = new D3D11ResourceLayout[genericLayouts.Length];
            for (int i = 0; i < ResourceLayouts.Length; i++)
            {
                ResourceLayouts[i] = Util.AssertSubtype<ResourceLayout, D3D11ResourceLayout>(genericLayouts[i]);
            }

            Debug.Assert(vsBytecode != null || ComputeShader != null);
            if (vsBytecode != null && description.ShaderSet.VertexLayouts.Length > 0)
            {
                InputLayout = inputLayout;
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
            : base(ref description)
        {
            IsComputePipeline = true;
            ComputeShader = (ID3D11ComputeShader)((D3D11Shader)description.ComputeShader).DeviceShader;
            ResourceLayout[] genericLayouts = description.ResourceLayouts;
            ResourceLayouts = new D3D11ResourceLayout[genericLayouts.Length];
            for (int i = 0; i < ResourceLayouts.Length; i++)
            {
                ResourceLayouts[i] = Util.AssertSubtype<ResourceLayout, D3D11ResourceLayout>(genericLayouts[i]);
            }
        }

        public override string Name
        {
            get => _name;
            set => _name = value;
        }

        public override bool IsDisposed => _disposed;

        public override void Dispose()
        {
            _disposed = true;
        }
    }
}
