cbuffer LightInfoBuffer : register(b4)
{
    float3 lightDir;
	float __padding;
}

struct PixelInput
{
    float4 position : SV_POSITION;
    float3 position_worldSpace : POSITION;
    float4 lightPosition : TEXCOORD0; //vertex with regard to light view
    float3 normal : NORMAL;
    float2 texCoord : TEXCOORD1;
};

Texture2D surfaceTexture;
SamplerState RegularSampler : register(s0);

Texture2D shadowMap;
SamplerState ShadowMapSampler : register(s1);

float4 PS(PixelInput input) : SV_Target
{
    float4 surfaceColor = surfaceTexture.Sample(RegularSampler, input.texCoord);
    float4 ambient = float4(.4, .4, .4, .4);

    //re-homogenize position after interpolation
    input.lightPosition.xyz /= input.lightPosition.w;
 
    // if position is not visible to the light - dont illuminate it
    // results in hard light frustum
    if( input.lightPosition.x < -1.0f || input.lightPosition.x > 1.0f ||
        input.lightPosition.y < -1.0f || input.lightPosition.y > 1.0f ||
        input.lightPosition.z < 0.0f  || input.lightPosition.z > 1.0f)
        {
            return ambient * surfaceColor;
        }
 
    //transform clip space coords to texture space coords (-1:1 to 0:1)
    input.lightPosition.x = input.lightPosition.x / 2 + 0.5;
    input.lightPosition.y = input.lightPosition.y / -2 + 0.5;
 
	float shadowMapBias = 0.005f;
	input.lightPosition.z -= shadowMapBias;

    //sample shadow map - point sampler
    float shadowMapDepth = shadowMap.Sample(ShadowMapSampler, input.lightPosition.xy).r;
 
    //if clip space z value greater than shadow map value then pixel is in shadow
    if ( shadowMapDepth < input.lightPosition.z ) 
    {
        return ambient * surfaceColor;
    }

    //otherwise calculate ilumination at fragment
    float3 L = -1 * lightDir;
    float ndotl = dot( normalize(input.normal), L);
    return ambient * surfaceColor + surfaceColor * ndotl;
}