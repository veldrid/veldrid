// Global

cbuffer ProjectionMatrixBuffer : register(b0)
{
    float4x4 projection;
}

cbuffer ViewMatrixBuffer : register(b1)
{
    float4x4 view;
}

cbuffer LightProjectionMatrixBuffer : register(b2)
{
    float4x4 lightProjection;
}

cbuffer LightViewMatrixBuffer : register(b3)
{
    float4x4 lightView;
}

cbuffer LightInfoBuffer : register(b4)
{
    float4 lightPos;
}

struct PointLightInfo
{
	float3 position;
	float range;
	float3 color;
	float __padding;
};

// Per-Object

cbuffer WorldMatrixBuffer : register(b7)
{
    float4x4 world;
}

cbuffer InverseTransposeWorldMatrixBuffer : register(b8)
{
    float4x4 inverseTransposeWorld;
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
    float3 position_worldSpace : POSITION;
    float4 lightPosition : TEXCOORD0; //vertex with regard to light view
    float3 normal : NORMAL;
    float2 texCoord : TEXCOORD1;
};

PixelInput VS( VertexInput input )
{
    PixelInput output;
    float4 worldPosition = mul(world, float4(input.position, 1));
    float4 viewPosition = mul(view, worldPosition);
    output.position = mul(projection, viewPosition);

	output.position_worldSpace = worldPosition.xyz;

    output.normal = mul((float3x3)inverseTransposeWorld, input.normal);
    output.normal = normalize(output.normal);
 
    output.texCoord = input.texCoord;

    //store worldspace projected to light clip space with
    //a texcoord semantic to be interpolated across the surface
	output.lightPosition = mul(world, float4(input.position, 1));
	output.lightPosition = mul(lightView, output.lightPosition);
	output.lightPosition = mul(lightProjection, output.lightPosition);
 
    return output;
}