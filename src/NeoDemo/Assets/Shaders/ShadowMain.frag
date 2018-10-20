#version 450

struct DepthCascadeLimits
{
    float NearLimit;
    float MidLimit;
    float FarLimit;
    float _padding;
};

struct DirectionalLightInfo
{
    vec3 Direction;
    float _padding;
    vec4 Color;
};

struct CameraInfos
{
    vec3 CameraPosition_WorldSpace;
    float _padding1;
    vec3 CameraLookDirection;
    float _padding2;
};

struct PointLightInfo
{
    vec3 Position;
    float _padding0;
    vec3 Color;
    float _padding1;
    float Range;
    float _padding2;
    float _padding3;
    float _padding4;
};

struct PointLightsInfo
{
    PointLightInfo PointLights[4];
    int NumActiveLights;
    float _padding0;
    float _padding1;
    float _padding2;
};

struct MaterialPropertiesInfo
{
    vec3 SpecularIntensity;
    float SpecularPower;
    vec3 _padding0;
    float Reflectivity;
};

struct ClipPlaneInformation
{
    vec4 ClipPlane;
    int Enabled;
};

layout(set = 1, binding = 3) uniform DepthLimits
{
    DepthCascadeLimits _DepthLimits;
};

layout(set = 1, binding = 4) uniform LightInfo
{
    DirectionalLightInfo _LightInfo;
};

layout(set = 1, binding = 5) uniform CameraInfo
{
    CameraInfos _CameraInfo;
};

layout(set = 1, binding = 6) uniform PointLights
{
    PointLightsInfo _PointLights;
};

layout(set = 2, binding = 1) uniform MaterialProperties
{
    MaterialPropertiesInfo _MaterialProperties;
};

layout(set = 2, binding = 2) uniform texture2D SurfaceTexture;
layout(set = 2, binding = 3) uniform sampler RegularSampler;
layout(set = 2, binding = 4) uniform texture2D AlphaMap;
layout(set = 2, binding = 5) uniform sampler AlphaMapSampler;
layout(set = 2, binding = 6) uniform texture2D ShadowMapNear;
layout(set = 2, binding = 7) uniform texture2D ShadowMapMid;
layout(set = 2, binding = 8) uniform texture2D ShadowMapFar;
layout(set = 2, binding = 9) uniform sampler ShadowMapSampler;
layout(set = 3, binding = 0) uniform texture2D ReflectionMap;
layout(set = 3, binding = 1) uniform sampler ReflectionSampler;
layout(set = 3, binding = 3) uniform ClipPlaneInfo
{
    ClipPlaneInformation _ClipPlaneInfo;
};

bool InRange(float val, float min, float max)
{
    return val >= min && val <= max;
}

float SampleDepthMap(int index, vec2 coord)
{
    if (index == 0)
    {
        return texture(sampler2D(ShadowMapNear, ShadowMapSampler), coord).x;
    }
    else if (index == 1)
    {
        return texture(sampler2D(ShadowMapMid, ShadowMapSampler), coord).x;
    }
    else
    {
        return texture(sampler2D(ShadowMapFar, ShadowMapSampler), coord).x;
    }
}

vec4 WithAlpha(vec4 baseColor, float alpha)
{
    return vec4(baseColor.xyz, alpha);
}

layout(location = 0) in vec3 Position_WorldSpace;
layout(location = 1) in vec4 LightPosition1;
layout(location = 2) in vec4 LightPosition2;
layout(location = 3) in vec4 LightPosition3;
layout(location = 4) in vec3 Normal;
layout(location = 5) in vec2 TexCoord;
layout(location = 6) in float FragDepth;
layout(location = 7) in vec4 ReflectionPosition;
layout(location = 0) out vec4 _outputColor_;

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;
layout(constant_id = 101) const bool TextureCoordinatesInvertedY = false;
layout(constant_id = 102) const bool ReverseDepthRange = true;

vec2 ClipToUV(vec4 clip)
{
    vec2 ret = vec2((clip.x / clip.w) / 2 + 0.5, (clip.y / clip.w) / -2 + 0.5);
    if (ClipSpaceInvertedY || TextureCoordinatesInvertedY)
    {
        ret.y = 1 - ret.y;
    }

    return ret;
}

bool IsDepthNearer(float a, float b)
{
    if (ReverseDepthRange) { return a > b; }
    else { return a < b; }
}

void main()
{
    if (_ClipPlaneInfo.Enabled == 1)
    {
        if (dot(_ClipPlaneInfo.ClipPlane, vec4(Position_WorldSpace, 1)) < 0)
        {
            discard;
        }
    }

    float alphaMapSample = texture(sampler2D(AlphaMap, AlphaMapSampler), TexCoord).x;
    if (alphaMapSample == 0)
    {
        discard;
    }

    vec4 surfaceColor = texture(sampler2D(SurfaceTexture, RegularSampler), TexCoord);
    if (_MaterialProperties.Reflectivity > 0)
    {
        vec2 reflectionTexCoords = ClipToUV(ReflectionPosition);
        vec4 reflectionSample = texture(sampler2D(ReflectionMap, ReflectionSampler), reflectionTexCoords);
        surfaceColor = (surfaceColor * (1 - _MaterialProperties.Reflectivity)) + (reflectionSample * _MaterialProperties.Reflectivity);
    }

    vec4 ambientLight = vec4(0.05f, 0.05f, 0.05f, 1.f);
    vec3 lightDir = -_LightInfo.Direction;
    vec4 directionalColor = ambientLight * surfaceColor;
    float shadowBias = 0.0005f;
    if (ReverseDepthRange)
    {
        shadowBias *= -1;
    }
    float lightIntensity = 0.f;
    vec4 directionalSpecColor = vec4(0, 0, 0, 0);
    vec3 vertexToEye = normalize(Position_WorldSpace - _CameraInfo.CameraPosition_WorldSpace);
    vec3 lightReflect = normalize(reflect(_LightInfo.Direction, Normal));
    float specularFactor = dot(vertexToEye, lightReflect);
    if (specularFactor > 0)
    {
        specularFactor = pow(abs(specularFactor), _MaterialProperties.SpecularPower);
        directionalSpecColor = vec4(_LightInfo.Color.xyz * _MaterialProperties.SpecularIntensity * specularFactor, 1.0f);
    }

    float depthTest = FragDepth;
    vec2 shadowCoords_0 = ClipToUV(LightPosition1);
    vec2 shadowCoords_1 = ClipToUV(LightPosition2);
    vec2 shadowCoords_2 = ClipToUV(LightPosition3);
    float lightDepthValues_0 = LightPosition1.z / LightPosition1.w;
    float lightDepthValues_1 = LightPosition2.z / LightPosition2.w;
    float lightDepthValues_2 = LightPosition3.z / LightPosition3.w;
    int shadowIndex = 3;
    vec2 shadowCoords = vec2(0, 0);
    float lightDepthValue = 0;
    if (IsDepthNearer(depthTest, _DepthLimits.NearLimit) && InRange(shadowCoords_0.x, 0, 1) && InRange(shadowCoords_0.y, 0, 1))
    {
        shadowIndex = 0;
        shadowCoords = shadowCoords_0;
        lightDepthValue = lightDepthValues_0;
    }
    else if (IsDepthNearer(depthTest, _DepthLimits.MidLimit) && InRange(shadowCoords_1.x, 0, 1) && InRange(shadowCoords_1.y, 0, 1))
    {
        shadowIndex = 1;
        shadowCoords = shadowCoords_1;
        lightDepthValue = lightDepthValues_1;
    }
    else if (IsDepthNearer(depthTest, _DepthLimits.FarLimit) && InRange(shadowCoords_2.x, 0, 1) && InRange(shadowCoords_2.y, 0, 1))
    {
        shadowIndex = 2;
        shadowCoords = shadowCoords_2;
        lightDepthValue = lightDepthValues_2;
    }

    if (shadowIndex != 3)
    {
        float shadowMapDepth = SampleDepthMap(shadowIndex, shadowCoords);
        float biasedDistToLight = (lightDepthValue - shadowBias);
        if (IsDepthNearer(biasedDistToLight, shadowMapDepth))
        {
            lightIntensity = clamp(dot(Normal, lightDir), 0, 1);
            if (lightIntensity > 0.0f)
            {
                directionalColor = surfaceColor * (lightIntensity * _LightInfo.Color);
            }
        }
        else
        {
            directionalColor = ambientLight * surfaceColor;
            directionalSpecColor = vec4(0, 0, 0, 0);
        }
    }
    else
    {
        lightIntensity = clamp(dot(Normal, lightDir), 0, 1);
        if (lightIntensity > 0.0f)
        {
            directionalColor = surfaceColor * lightIntensity * _LightInfo.Color;
        }
    }

    vec4 pointDiffuse = vec4(0, 0, 0, 1);
    vec4 pointSpec = vec4(0, 0, 0, 1);
    for (int i = 0; i < _PointLights.NumActiveLights; i++)
    {
        PointLightInfo pli = _PointLights.PointLights[i];
        vec3 ptLightDir = normalize(pli.Position - Position_WorldSpace);
        float intensity = clamp(dot(Normal, ptLightDir), 0, 1);
        float lightDistance = distance(pli.Position, Position_WorldSpace);
        intensity = clamp(intensity * (1 - (lightDistance / pli.Range)), 0, 1);
        pointDiffuse += intensity * vec4(pli.Color, 1) * surfaceColor;
        vec3 lightReflect0 = normalize(reflect(ptLightDir, Normal));
        float specularFactor0 = dot(vertexToEye, lightReflect0);
        if (specularFactor0 > 0 && pli.Range > lightDistance)
        {
            specularFactor0 = pow(abs(specularFactor0), _MaterialProperties.SpecularPower);
            pointSpec += (1 - (lightDistance / pli.Range)) * (vec4(pli.Color * _MaterialProperties.SpecularIntensity * specularFactor0, 1.0f));
        }
    }

    _outputColor_ = WithAlpha(clamp(directionalSpecColor + directionalColor + pointSpec + pointDiffuse, 0, 1), alphaMapSample);
}
