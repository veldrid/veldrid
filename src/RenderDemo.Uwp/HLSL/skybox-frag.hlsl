sampler sampler0;
TextureCubeArray Skybox;

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD;
};

float4 PS(PS_INPUT input) : SV_Target
{
    return Skybox.Sample(sampler0, float4(input.TexCoord, 1));
}
