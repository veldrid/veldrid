cbuffer ProjectionMatrixBuffer : register(b0)
{
    float4x4 projection;
}

cbuffer ViewMatrixBuffer : register(b1)
{
    float4x4 view;
}

cbuffer CameraInfoBuffer : register(b2)
{
    float3 cameraWorldPosition;
    float __unused1;
    float3 cameraLookDirection;
    float __unused2;
}

cbuffer WorldMatrixBuffer : register(b3)
{
    float4x4 world;
}

struct PixelInput
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

[maxvertexcount(4)]
void GS(point PixelInput input[1], inout TriangleStream<PixelInput> outputStream)
{
    float4 inPos = float4(input[0].position.xyz, 1);
    float3 worldCenter = mul(world, inPos).xyz;
    float3 globalUp = float3(0, 1, 0);
    float3 right = normalize(cross(cameraLookDirection, globalUp));
    float3 up = normalize(cross(right.xyz, cameraLookDirection));
    float3 worldPositions[4] =
    {
        worldCenter - right * .5 + up * .5,
        worldCenter + right * .5 + up * .5,
        worldCenter - right * .5 - up * .5,
        worldCenter + right * .5 - up * .5,
    };
    
    PixelInput output;

    for (int i = 0; i < 4; i++)
    {
        output.position = mul(projection, mul(view, float4(worldPositions[i], 1)));
        output.color = float4(1, 0, 0, 1);
        outputStream.Append(output);
    }
}