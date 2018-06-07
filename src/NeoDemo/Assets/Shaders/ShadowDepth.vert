#version 450

struct Veldrid_NeoDemo_Objects_WorldAndInverse
{
    mat4 World;
    mat4 InverseWorld;
};

layout(set = 0, binding = 0) uniform ViewProjection
{
    mat4 _ViewProjection;
};

layout(set = 1, binding = 0) uniform WorldAndInverse
{
    Veldrid_NeoDemo_Objects_WorldAndInverse _WorldAndInverse;
};

layout(location = 0) in vec3 vsin_Position;
layout(location = 1) in vec3 vsin_Normal;
layout(location = 2) in vec2 vsin_TexCoord;

void main()
{
    gl_Position = _ViewProjection * _WorldAndInverse.World * vec4(vsin_Position, 1);
    gl_Position.y += vsin_TexCoord.y * .0001f;
}
