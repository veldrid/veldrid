#include <metal_stdlib>
using namespace metal;

struct VertexIn
{
    float2 Position [[ attribute(0) ]];
    float4 Color [[ attribute(1) ]];
};

struct VertexOut
{
    float4 Position [[ position ]];
    float4 Color;
};

vertex VertexOut VS(
    VertexIn input [[ stage_in ]])
{
    VertexOut output;
    output.Position = float4(input.Position, 0, 1);
    output.Color = input.Color;
    return output;
}
