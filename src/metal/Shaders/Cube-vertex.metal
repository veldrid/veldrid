#include <metal_stdlib>
using namespace metal;

struct VertexIn
{
    float3 Position [[ attribute(0) ]];
    float2 TexCoord [[ attribute(1) ]];
};

struct VertexOut
{
    float4 Position [[ position ]];
    float2 TexCoord;
};

vertex VertexOut VS(
    VertexIn input [[ stage_in ]],
    constant float4x4 &projection [[ buffer(1) ]],
    constant float4x4 &view [[ buffer(2) ]],
    constant float4x4 &world [[ buffer(3) ]] )
{
    VertexOut output;
    output.Position = projection * view * world * float4(input.Position, 1);
    output.TexCoord = input.TexCoord;
    return output;
}
