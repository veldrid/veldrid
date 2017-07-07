struct PixelInput
{
    float4 position : SV_POSITION;
    float2 texCoord : TEXCOORD0;
};

Texture2D surfaceTexture : register(t0);
SamplerState RegularSampler : register(s0);

float4 PS(PixelInput input) : SV_Target
{
    float r = surfaceTexture.Sample(RegularSampler, input.texCoord).r;
    return float4(r, r, r, 1);
}
