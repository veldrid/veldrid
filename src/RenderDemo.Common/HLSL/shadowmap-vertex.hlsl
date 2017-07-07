cbuffer ProjectionMatrixBuffer : register(b0)
{
    float4x4 projection;
}

cbuffer ViewMatrixBuffer : register(b1)
{
    float4x4 view;
}

cbuffer WorldMatrixBuffer : register(b2)
{
    float4x4 world;
}

struct VertexInput
{
    float3 position : POSITION;
    float3 normal : NORMAL;
    float2 texCoord : TEXCOORD0;
};

struct PixelInput
{
    float4 position : SV_POSITION;
};

PixelInput VS(VertexInput input)
{
    PixelInput output;

    float4 worldPosition = mul(world, float4(input.position, 1));
    float4 viewPosition = mul(view, worldPosition);
    output.position = mul(projection, viewPosition);

    return output;
}
