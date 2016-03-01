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

    float4x4 worldT = transpose(world);
    float4x4 viewT = transpose(view);
    float4x4 projectionT = transpose(projection);

    float4 worldPosition = mul(input.position, worldT);
    float4 viewPosition = mul(worldPosition, viewT);
    output.position = mul(viewPosition, projectionT);

    output.texCoord = input.texCoord;

    return output;
}
