struct PixelInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR0;
};


float4 PS(PixelInput input) : SV_Target
{
    return input.color;
}
