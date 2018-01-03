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
    float4 Position [[ position ]];
    float3 Normal;
    float3 Color;
    float3 EyePos;
    float3 LightVec;
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
    float4 v4Pos = float4(input.Position, 1);
    VertexOut output;
    output.Normal = input.Normal;
    output.Color = input.Color;
    output.Position = uniformInfo.Projection * uniformInfo.View * (uniformInfo.Model * v4Pos);
    float4 eyePos = uniformInfo.View * uniformInfo.View * v4Pos;
    output.EyePos = eyePos.xyz;
    output.LightVec = normalize(uniformInfo.LightPos.xyz - output.EyePos);
    return output;
}
