#version 450 core
layout(location = 0) in vec3 vsin_position;
layout(location = 1) in vec2 vsin_uv;
layout(location = 0) out vec2 fsin_uv;

layout(set = 0, binding = 0) uniform UniformState
{
    mat4 Projection;
    mat4 View;
    mat4 World;
};

void main()
{
    gl_Position = Projection * View * World * vec4(vsin_position, 1);
    fsin_uv = vsin_uv;
}