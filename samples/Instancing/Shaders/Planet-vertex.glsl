#version 450

layout(set = 0, binding = 1) uniform ProjView
{
    mat4 View;
    mat4 Proj;
};

layout(location = 0) in vec3 Position;
layout(location = 1) in vec3 Normal;
layout(location = 2) in vec2 TexCoord;

layout(location = 0) out vec3 fsin_Position_WorldSpace;
layout(location = 1) out vec3 fsin_Normal;
layout(location = 2) out vec2 fsin_TexCoord;

void main()
{
    fsin_Position_WorldSpace = Position;
    vec4 pos = vec4(Position, 1);
    gl_Position = Proj * View * pos;
    fsin_Normal = Normal;
    fsin_TexCoord = TexCoord * vec2(10, 6);
}
