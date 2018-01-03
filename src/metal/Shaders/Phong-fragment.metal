#include <metal_stdlib>
using namespace metal;

struct VertexOut
{
    float4 Position [[ position ]];
    float3 Normal;
    float3 Color;
    float3 EyePos;
    float3 LightVec;
};

fragment float4 FS(
    VertexOut input [[ stage_in ]])
{
    float3 Eye = normalize(-input.EyePos);
    float3 Reflected = normalize(reflect(-input.LightVec, input.Normal));
    float4 IAmbient = float4(0.1f, 0.1f, 0.1f, 1.0f);
    float4 IDiffuse = float4(max(dot(input.Normal, input.LightVec), 0.f), max(dot(input.Normal, input.LightVec), 0.f), max(dot(input.Normal, input.LightVec), 0.f), max(dot(input.Normal, input.LightVec), 0.f));
    float specular = 0.75f;
    float4 ISpecular = float4(0.0f, 0.0f, 0.0f, 0.0f);
    if (dot(input.EyePos, input.Normal) < 0.0)
    {
        ISpecular = float4(0.5f, 0.5f, 0.5f, 1.0f) * pow(max(dot(Reflected, Eye), 0.0f), 16.0f) * specular;
    }
    return (IAmbient + IDiffuse) * float4(input.Color, 1.0f) + ISpecular;
}
