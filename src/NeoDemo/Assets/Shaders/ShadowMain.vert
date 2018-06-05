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

struct CameraInfo
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

struct WorldAndInverseMats
{
    mat4 World;
    mat4 InverseWorld;
};

struct MaterialProperties
{
    vec3 SpecularIntensity;
    float SpecularPower;
    vec3 _padding0;
    float Reflectivity;
};

struct ClipPlaneInfo
{
    vec4 ClipPlane;
    int Enabled;
};

layout(set = 0, binding = 0) uniform Projection
{
    mat4 _Projection;
};

layout(set = 0, binding = 1) uniform View
{
    mat4 _View;
};

layout(set = 1, binding = 0) uniform LightViewProjection1
{
    mat4 _LightViewProjection1;
};

layout(set = 1, binding = 1) uniform LightViewProjection2
{
    mat4 _LightViewProjection2;
};

layout(set = 1, binding = 2) uniform LightViewProjection3
{
    mat4 _LightViewProjection3;
};

layout(set = 2, binding = 0) uniform WorldAndInverse
{
    WorldAndInverseMats _WorldAndInverse;
};

layout(set = 3, binding = 2) uniform ReflectionViewProj
{
    mat4 _ReflectionViewProj;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;
layout(location = 0) out vec3 fsin_Position_WorldSpace;
layout(location = 1) out vec4 fsin_LightPosition1;
layout(location = 2) out vec4 fsin_LightPosition2;
layout(location = 3) out vec4 fsin_LightPosition3;
layout(location = 4) out vec3 fsin_Normal;
layout(location = 5) out vec2 fsin_TexCoord;
layout(location = 6) out float fsin_FragDepth;
layout(location = 7) out vec4 fsin_ReflectionPosition;

void main()
{
    vec4 worldPosition = _WorldAndInverse.World * vec4(Position, 1);
    vec4 viewPosition = _View * worldPosition;
    gl_Position = _Projection * viewPosition;
    fsin_Position_WorldSpace = worldPosition.xyz;
    vec4 outNormal = _WorldAndInverse.InverseWorld * vec4(Normal, 1);
    fsin_Normal = normalize(outNormal.xyz);
    fsin_TexCoord = TexCoord;
    fsin_LightPosition1 = _WorldAndInverse.World * vec4(Position, 1);
    fsin_LightPosition1 = _LightViewProjection1 * fsin_LightPosition1;
    fsin_LightPosition2 = _WorldAndInverse.World * vec4(Position, 1);
    fsin_LightPosition2 = _LightViewProjection2 * fsin_LightPosition2;
    fsin_LightPosition3 = _WorldAndInverse.World * vec4(Position, 1);
    fsin_LightPosition3 = _LightViewProjection3 * fsin_LightPosition3;
    fsin_FragDepth = gl_Position.z;
    fsin_ReflectionPosition = _WorldAndInverse.World * vec4(Position, 1);
    fsin_ReflectionPosition = _ReflectionViewProj * fsin_ReflectionPosition;
}
