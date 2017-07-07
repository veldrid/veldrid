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
    float3 lookDirection;
    float __unused;
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

static const float4 cubePositions[8] =
{
    float4(-.5, .5, .5, 1),
    float4(.5, .5, .5, 1),
    float4(.5, -.5, .5, 1),
    float4(-.5, -.5, .5, 1),
    float4(-.5, .5, -.5, 1),
    float4(.5, .5, -.5, 1),
    float4(.5, -.5, -.5, 1),
    float4(-.5, -.5, -.5, 1)
};

static const int cubeIndices[24] =
{
    0, 1, 3, 2, // front
    5, 4, 6, 7, // back
    4, 0, 7, 3, // left
    1, 5, 2, 6, // right
    4, 5, 0, 1, // top
    3, 2, 7, 6 // bottom
};

[maxvertexcount(24)]
void GS(point PixelInput input[1], inout TriangleStream<PixelInput> outputStream)
{
    PixelInput output;

    float4 center = input[0].position;
    float step = (1.0 / 24.0);
    float g = 1.0 / 24.0;
    for (int i = 0; i < 24; i += 4)
    {
        output.position = mul(projection, mul(view, mul(world, (center + cubePositions[cubeIndices[i]]))));
        output.color = float4(1, g, 1, 1);
        outputStream.Append(output);
        g += step;
        output.position = mul(projection, mul(view, mul(world, (center + cubePositions[cubeIndices[i + 1]]))));
        output.color = float4(1, g, 1, 1);
        outputStream.Append(output);
        g += step;
        output.position = mul(projection, mul(view, mul(world, (center + cubePositions[cubeIndices[i + 2]]))));
        output.color = float4(1, g, 1, 1);
        outputStream.Append(output);
        g += step;
        output.position = mul(projection, mul(view, mul(world, (center + cubePositions[cubeIndices[i + 3]]))));
        output.color = float4(1, g, 1, 1);
        outputStream.Append(output);
        g += step;

        outputStream.RestartStrip();
    }
}