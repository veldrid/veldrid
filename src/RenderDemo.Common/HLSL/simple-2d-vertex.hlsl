cbuffer WorldMatrixBuffer : register(b0)
{
    float4x4 world;
}

cbuffer ProjectionMatrixBuffer : register(b1)
{
    float4x4 projection;
}


struct VertexInput
{
    float3 position : POSITION;
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
    output.position = float4(input.position, 1);
    output.position = mul(world, output.position);
    output.position = mul(projection, output.position);
    output.texCoord = input.texCoord;
    return output;
}
