cbuffer LightInfoBuffer : register(b4)
{
	float3 lightDir;
	float __padding;
}

cbuffer CameraInfoBuffer : register(b5)
{
	float3 cameraPosition_worldSpace;
	float __padding1;
    float3 cameraLookDirection;
    float __padding3;
}

#define MAX_POINT_LIGHTS 4

struct PointLightInfo
{
	float3 position;
	float range;
	float3 color;
	float __padding;
};

cbuffer PointLightsBuffer : register(b6)
{
	int numActiveLights;
	float3 __padding2;
	PointLightInfo pointLights[MAX_POINT_LIGHTS];
}

cbuffer MaterialPropertiesBuffer : register(b9)
{
	float3 specularIntensity;
	float specularPower;
}

struct PixelInput
{
	float4 position : SV_POSITION;
	float3 position_worldSpace : POSITION;
	float4 lightPosition : TEXCOORD0; //vertex with regard to light view
	float3 normal : NORMAL;
	float2 texCoord : TEXCOORD1;
};

Texture2D SurfaceTexture : register(t0);
SamplerState RegularSampler : register(s0);

Texture2D AlphaMap : register(t1);
SamplerState AlphaMapSampler : register(s1);

Texture2D ShadowMap : register(t2);
SamplerState ShadowMapSampler : register(s2);

float4 WithAlpha(float4 baseColor, float alpha)
{
    return float4(baseColor.rgb, alpha);
}

float4 PS(PixelInput input) : SV_Target
{
    float alphaMapSample = AlphaMap.Sample(AlphaMapSampler, input.texCoord).r;
    if (alphaMapSample == 0)
    {
        discard;
    }

	float4 surfaceColor = SurfaceTexture.Sample(RegularSampler, input.texCoord);
    // surfaceColor.a *= alphaMapSample;
	float4 ambientLight = float4(.4, .4, .4, 1);

	// Point Diffuse

	float4 pointDiffuse = float4(0, 0, 0, 1);
	float4 pointSpec = float4(0, 0, 0, 1);
	for (int i = 0; i < numActiveLights; i++)
	{
		PointLightInfo pli = pointLights[i];
		float3 lightDir = normalize(pli.position - input.position_worldSpace);
		float intensity = saturate(dot(input.normal, lightDir));
		float lightDistance = distance(pli.position, input.position_worldSpace);
		intensity = saturate(intensity * (1 - (lightDistance / pli.range)));

		pointDiffuse += intensity * float4(pli.color, 1) * surfaceColor;

		// Specular
		float3 vertexToEye = normalize(cameraPosition_worldSpace - input.position_worldSpace);
		float3 lightReflect = normalize(reflect(lightDir, input.normal));

		float specularFactor = dot(vertexToEye, lightReflect);
		if (specularFactor > 0)
		{
			specularFactor = pow(abs(specularFactor), specularPower);
			pointSpec += (1 - (lightDistance / pli.range)) * (float4(pli.color * specularIntensity * specularFactor, 1.0f));
		}
	}

	pointDiffuse = saturate(pointDiffuse);
	pointSpec = saturate(pointSpec);

	// Directional light calculations

	//re-homogenize position after interpolation
	input.lightPosition.xyz /= input.lightPosition.w;

	// if position is not visible to the light - dont illuminate it
	// results in hard light frustum
	if (input.lightPosition.x < -1.0f || input.lightPosition.x > 1.0f ||
		input.lightPosition.y < -1.0f || input.lightPosition.y > 1.0f ||
		input.lightPosition.z < 0.0f || input.lightPosition.z > 1.0f)
	{
        return WithAlpha((ambientLight * surfaceColor) + pointDiffuse + pointSpec, surfaceColor.a);
    }

	// transform clip space coords to texture space coords (-1:1 to 0:1)
	input.lightPosition.x = input.lightPosition.x / 2 + 0.5;
	input.lightPosition.y = input.lightPosition.y / -2 + 0.5;

	float3 L = -1 * normalize(lightDir);
	float diffuseFactor = dot(normalize(input.normal), L);

	float cosTheta = clamp(diffuseFactor, 0, 1);
	float bias = 0.0005 * tan(acos(cosTheta));
	bias = clamp(bias, 0, 0.01);

	input.lightPosition.z -= bias;

	//sample shadow map - point sampler
	float ShadowMapDepth = ShadowMap.Sample(ShadowMapSampler, input.lightPosition.xy).r;

	//if clip space z value greater than shadow map value then pixel is in shadow
	if (ShadowMapDepth < input.lightPosition.z)
	{
		return WithAlpha((ambientLight * surfaceColor) + pointDiffuse + pointSpec, surfaceColor.a);
	}

	//otherwise calculate ilumination at fragment
	diffuseFactor = clamp(diffuseFactor, 0, 1);

	float4 specularColor = float4(0, 0, 0, 0);

	float3 vertexToEye = normalize(cameraPosition_worldSpace - input.position_worldSpace);
	float3 lightReflect = normalize(reflect(lightDir, input.normal));
	float3 lightColor = float3(1, 1, 1);

	float specularFactor = dot(vertexToEye, lightReflect);
	if (specularFactor > 0)
	{
		specularFactor = pow(abs(specularFactor), specularPower);
		specularColor = float4(lightColor * specularIntensity * specularFactor, 1.0f);
	}

	return WithAlpha(specularColor + (ambientLight * surfaceColor) + (diffuseFactor * surfaceColor) + pointDiffuse + pointSpec, surfaceColor.a);
}