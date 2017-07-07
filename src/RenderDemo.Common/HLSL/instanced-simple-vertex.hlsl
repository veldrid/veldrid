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
    // Per-Vertex
    float3 position : POSITION;
	// Per-Instance
	float3 offset : TEXCOORD0;
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

	float4 worldPos = mul(world, float4(input.position + input.offset, 1));
	float4 viewPos = mul(view, worldPos);
	float4 projPos = mul(projection, viewPos);
	output.position = projPos;

    output.color = input.color;

    return output;
}
