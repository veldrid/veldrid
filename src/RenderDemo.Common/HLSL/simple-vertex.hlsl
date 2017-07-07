cbuffer ProjectionMatrixBuffer : register(b0)
{
    float4x4 projection;
}

cbuffer WorldViewMatrixBuffer : register(b1)
{
    float4x4 worldview;
}

struct VertexInput
{
    float3 position : POSITION;
    float4 color : COLOR;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

PixelInput VS(VertexInput input)
{
    PixelInput output;

    float4 worldViewPosition = mul(worldview, float4(input.position, 1));
    output.position = mul(projection, worldViewPosition);

    output.color = input.color;

    return output;
}
