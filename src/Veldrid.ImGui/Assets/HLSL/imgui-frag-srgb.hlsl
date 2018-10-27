struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float4 col : COLOR0;
    float2 uv  : TEXCOORD0;
};

Texture2D FontTexture : register(t0);
sampler FontSampler : register(s0);

// http://chilliant.blogspot.com/2012/08/srgb-approximations-for-hlsl.html
float3 LinearToSrgb(float3 lin)
{
    float3 S1 = sqrt(lin);
    float3 S2 = sqrt(S1);
    float3 S3 = sqrt(S2);
    return 0.585122381 * S1 + 0.783140355 * S2 - 0.368262736 * S3;
}

float4 FS(PS_INPUT input) : SV_Target
{
    float4 out_col = input.col * FontTexture.Sample(FontSampler, input.uv);
    out_col = float4(LinearToSrgb(out_col.rgb), out_col.a);
    return out_col;
}
