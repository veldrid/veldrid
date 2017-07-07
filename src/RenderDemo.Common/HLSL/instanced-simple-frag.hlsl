struct PixelInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

float4 PS(PixelInput input) : SV_Target
{
    float4 color = input.color;
    return color;
}
