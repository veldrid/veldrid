#include <metal_stdlib>
using namespace metal;

struct VertexOut
{
    float4 SysPosition [[ position ]];
    float2 UV;
    float4 Position;
};

fragment float4 FS(
    VertexOut input [[ stage_in ]],
    texture2d<float> ReflectionMap [[ texture(0) ]],
    sampler ReflectionMapSampler [[ sampler(0) ]],
    texture2d<float> ColorMap [[ texture(1) ]],
    sampler ColorMapSampler [[ sampler(1) ]],
    bool frontFacing [[ front_facing ]])
{
    float4 outFragColor;
    float2 projCoord = float2((input.Position.x / input.Position.w) / 2 + 0.5, (input.Position.y / input.Position.w) / -2 + 0.5);
    float blurSize = 1.f / 512.f;
    float4 color = ColorMap.sample(ColorMapSampler, input.UV);
    outFragColor =color * 0.25f;
    if (frontFacing)
    {
        float4 reflection = float4(0.0f, 0.0f, 0.0f, 0.0f);
        for (int x = -3; x <= 3; x++)
        {
            for (int y = -3; y <= 3; y++)
            {
                reflection += ReflectionMap.sample(ReflectionMapSampler, float2(projCoord.x + x * blurSize, projCoord.y + y * blurSize)) / 49.0f;
            }
        }
        outFragColor +=reflection * 1.5f * (color.x);
    }

    return outFragColor;
}
