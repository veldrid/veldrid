#version 450

layout (set = 0, binding = 0) uniform CameraInfo
{
    mat4 View;
    mat4 InvView;
    mat4 Projection;
    mat4 InvProjection;
    vec3 CameraPosition_WorldSpace;
    float _padding1;
    vec3 CameraLookDirection;
    float _padding2;
};

layout (set = 1, binding = 0) uniform FBInfo
{
    uint FB_Width;
    uint FB_Height;
    uint _FBInfo_padding0;
    uint _FBInfo_padding1;
};

layout (set = 2, binding = 0) uniform textureCube SkyTex;
layout (set = 2, binding = 1) uniform sampler SkySamp;

layout (location = 0) in vec2 fsin_UV;
layout (location = 0) out vec4 fsout_color;

void main()
{
    float x = fsin_UV.x * 2.0f - 1.0f;
    float y = 1.0f - fsin_UV.y * 2.0f;
    float z = 1.0f;
    vec3 ray_nds = vec3(x, y, z);
    vec4 ray_clip = vec4(ray_nds.xy, -1.0, 1.0);
    vec4 ray_eye = InvProjection * ray_clip;
    ray_eye = vec4(ray_eye.xy, -1.0, 0.0);
    vec3 ray_wor = (InvView * ray_eye).xyz;
    ray_wor = normalize(ray_wor);

    fsout_color = texture(samplerCube(SkyTex, SkySamp), ray_wor);
}