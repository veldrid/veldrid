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
    float4 position : POSITION;
    float2 texCoord : TEXCOORD0;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float2 texCoord : TEXCOORD0;
};

PixelInput VS(VertexInput input)
{
    PixelInput output;

    float4 worldPosition = mul(world, input.position);
    float4 viewPosition = mul(view, worldPosition);
    output.position = mul(projection, viewPosition);

    output.texCoord = input.texCoord;

    return output;
}
