#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

struct ColoredVertex
{
    vec4 Color;
    vec2 Position;
    vec2 _padding0;
};

layout(std430, set = 0, binding = 0) readonly buffer InputVertices
{
    ColoredVertex _InputVertices[];
};

layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(_InputVertices[gl_VertexIndex].Position, 0, 1);
    fsin_Color = _InputVertices[gl_VertexIndex].Color;
}
