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

    float4x4 worldviewT = transpose(worldview);
    float4x4 projectionT = transpose(projection);

    float4 worldviewPosition = mul(float4(input.position, 1), worldviewT);
    output.position = mul(worldviewPosition, projectionT);

    output.color = input.color;

    return output;
}
