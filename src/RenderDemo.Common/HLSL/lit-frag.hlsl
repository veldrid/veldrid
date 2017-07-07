cbuffer LightBuffer : register(b2)
{
    float4 diffuseColor;
    float3 lightDirection;
    float __buffer;
}

struct PixelInput
{
    float4 position : SV_POSITION;
    float3 normal : NORMAL;
    float2 texCoord : TEXCOORD0;
};

Texture2D shaderTexture;
SamplerState MeshTextureSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    MaxLOD = 0.0f;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4 PS(PixelInput input) : SV_Target
{
    float4 ambientColor = float4(.4, .4, .4, 1);

    float4 color = shaderTexture.Sample(MeshTextureSampler, input.texCoord);
    float3 lightDir = -normalize(lightDirection);
    float effectiveness = dot(input.normal, lightDir);
    float lightEffectiveness = saturate(effectiveness);
    float4 lightColor = saturate(diffuseColor * lightEffectiveness);
    return saturate((lightColor * color) + (ambientColor * color));
}
