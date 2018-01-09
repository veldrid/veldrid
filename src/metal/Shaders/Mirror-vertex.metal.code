#include <metal_stdlib>
using namespace metal;

struct VertexIn
{
    float3 Position [[ attribute(0) ]];
    float2 UV [[ attribute(1) ]];
    float3 Color [[ attribute(2) ]];
    float3 Normal [[ attribute(3) ]];
};

struct VertexOut
{
    float4 SysPosition [[ position ]];
    float2 UV;
    float4 Position;
};

struct UniformInfo
{
    float4x4 Projection;
    float4x4 View;
    float4x4 Model;
    float4 LightPos;
};

vertex VertexOut VS(
    VertexIn input [[ stage_in ]],
    constant UniformInfo &uniformInfo [[ buffer(1) ]])
{
    VertexOut output;
    output.UV = input.UV;
    output.SysPosition = uniformInfo.Projection * uniformInfo.View * uniformInfo.Model * float4(input.Position, 1);
    output.Position = uniformInfo.Projection * uniformInfo.View * uniformInfo.Model * float4(input.Position, 1);
    return output;
}
