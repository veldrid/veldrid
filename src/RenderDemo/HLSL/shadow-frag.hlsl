cbuffer LightBuffer : register(b2)
{
    float4 diffuseColor;
    float3 lightDirection;
    float __buffer;
}

struct PixelInput
{
    float4 position : SV_POSITION;
    float4 lightPosition : TEXCOORD0; //vertex with regard to light view
    float3 normal : NORMAL;
    float2 texCoord : TEXCOORD0;
};

Texture2D shaderTexture;
SamplerState MeshTextureSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    MaxLOD = 0.0f;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4 PS(PixelInput input) : SV_Target
{
    //re-homogenize position after interpolation
    input.lightPosition.xyz /= input.lightPosition.w;
 
    //if position is not visible to the light - dont illuminate it
    //results in hard light frustum
    if( input.lightPosition.x < -1.0f || input.lightPosition.x > 1.0f ||
        input.lightPosition.y < -1.0f || input.lightPosition.y > 1.0f ||
        input.lightPosition.z < 0.0f  || input.lightPosition.z > 1.0f ) return ambient;
 
    //transform clip space coords to texture space coords (-1:1 to 0:1)
    input.lightPosition.x = input.lightPosition.x/2 + 0.5;
    input.lightPosition.y = input.lightPosition.y/-2 + 0.5;
 
    //sample shadow map - point sampler
    float shadowMapDepth = shadowMap.Sample(pointSampler, input.lightPosition.xy).r;
 
    //if clip space z value greater than shadow map value then pixel is in shadow
    if ( shadowMapDepth < input.lightPosition.z) return ambient;
 
    //otherwise calculate ilumination at fragment
    float3 L = normalize(lightPos - input.position.xyz);
    float ndotl = dot( normalize(input.normal), L);
    return ambient + diffuse*ndotl;
}