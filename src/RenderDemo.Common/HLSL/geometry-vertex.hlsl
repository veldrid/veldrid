struct VertexInput
{
    float3 position : POSITION;
};

struct PixelInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

PixelInput VS(VertexInput input)
{
    PixelInput output;
    output.position = float4(input.position, 0);
    output.color = float4(0, 0, 0, 0);

    return output;
}
