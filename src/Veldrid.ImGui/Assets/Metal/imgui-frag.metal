#include <metal_stdlib>
using namespace metal;

struct PS_INPUT
{
    float4 pos [[ position ]];
    float4 col;
    float2 uv;
};

constant bool OutputLinear [[ function_constant(1) ]];

// http://chilliant.blogspot.com/2012/08/srgb-approximations-for-hlsl.html
float3 LinearToSrgb(float3 linear)
{
  float3 S1 = sqrt(linear);
  float3 S2 = sqrt(S1);
  float3 S3 = sqrt(S2);
  return 0.585122381 * S1 + 0.783140355 * S2 - 0.368262736 * S3;
}

fragment float4 FS(
    PS_INPUT input [[ stage_in ]],
    texture2d<float> FontTexture [[ texture(0) ]],
    sampler FontSampler [[ sampler(0) ]])
{
    float4 out_col = input.col * FontTexture.sample(FontSampler, input.uv);
    if (!OutputLinear)
    {
        out_col = float4(LinearToSrgb(out_col.rgb), out_col.a);
    }

    return out_col;
}
