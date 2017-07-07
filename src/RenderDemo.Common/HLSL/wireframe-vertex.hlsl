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
    float4 color : COLOR0;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float4 color :  COLOR0;
};

PixelInput VS(VertexInput input)
{
    PixelInput output;

    float4 worldPosition = mul(world, float4(input.position, 1));
    float4 viewPosition = mul(view, worldPosition);
    output.position = mul(projection, viewPosition);

    output.color = input.color;

    return output;
}
